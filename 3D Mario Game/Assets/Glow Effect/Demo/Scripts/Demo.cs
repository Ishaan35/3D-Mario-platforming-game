using UnityEngine;
using System.Collections;
using System;

namespace GlowEffect
{
    public class Demo : MonoBehaviour
    {
        public bool enableGlow = true;
        public GlowEffect.GlowMode glowMode = GlowEffect.GlowMode.Glow;
        public GlowEffect.BlendMode blendMode = GlowEffect.BlendMode.Additive;
        public int downsamplePower = 8; // 2 ^ downSamplePower = downsampleSize
        public float glowStrength = 2;
        public int blurIterations = 8;
        public float blurSpread = 1.2f;

        public GameObject glowGroup;
        public GameObject alphaGlowGroup;

        public GlowEffect glowEffect;
        private Rect glowControlsVisibleRect;
        private Rect glowControlsNotVisibleRect;
        private bool showGlowControls;

        // for fps (modified from http://wiki.unity3d.com/index.php?title=FramesPerSecond):
        private float updateInterval = 0.5f;
        private float accum = 0;
        private int frames = 0;
        private float timeleft;
        private float fps;

        public void Start()
        {
            glowControlsVisibleRect = new Rect(0, 0, 320, 500);
            glowControlsNotVisibleRect = new Rect(0, 0, 320, 100);
            showGlowControls = true;

            timeleft = updateInterval;

            UpdateGlow();
        }

        public void OnGUI()
        {
            GUILayout.BeginArea((showGlowControls ? glowControlsVisibleRect : glowControlsNotVisibleRect));
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Toggle Controls")) {
                showGlowControls = !showGlowControls;
            }
            if (GUILayout.Button((Time.timeScale == 0.0f ? "Play" : "Pause"))) {
                Time.timeScale = (Time.timeScale == 0.0f ? 1.0f : 0.0f);
            }
            if (GUILayout.Button((Time.timeScale == 0.2f ? "Normal Speed" : "Slow Motion"))) {
                Time.timeScale = (Time.timeScale == 0.2f ? 1.0f : 0.2f);
            }
            GUILayout.EndHorizontal();

            if (showGlowControls) {
                bool prevBool = enableGlow;
                GUILayout.BeginHorizontal(GUILayout.Width(200));
                GUILayout.Label(string.Format("Enable Glow: {0}", enableGlow));
                GUILayout.FlexibleSpace();
                enableGlow = GUILayout.Toggle(enableGlow, "");
                if (prevBool != enableGlow) {
                    UpdateGlow();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal(GUILayout.Width(240));
                GUILayout.Label(string.Format("Glow Mode: {0}", Enum.GetName(typeof(GlowEffect.GlowMode), glowMode)));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<-")) {
                    glowMode = (GlowEffect.GlowMode)(((int)glowMode - 1) % 4);
                    if ((int)glowMode < 0) glowMode = GlowEffect.GlowMode.SimpleAlphaGlow;
                    UpdateGlow();
                }
                if (GUILayout.Button("->")) {
                    glowMode = (GlowEffect.GlowMode)(((int)glowMode + 1) % 4);
                    UpdateGlow();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal(GUILayout.Width(240));
                GUILayout.Label(string.Format("Blend Mode: {0}", Enum.GetName(typeof(GlowEffect.BlendMode), blendMode)));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<-")) {
                    blendMode = (GlowEffect.BlendMode)(((int)blendMode - 1) % 4);
                    if ((int)blendMode < 0) blendMode = GlowEffect.BlendMode.Subtract;
                    UpdateGlow();
                }
                if (GUILayout.Button("->")) {
                    blendMode = (GlowEffect.BlendMode)(((int)blendMode + 1) % 4);
                    UpdateGlow();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Downsample size: {0}", Mathf.Pow(2, downsamplePower)));
                GUILayout.FlexibleSpace();
                int prevInt = downsamplePower;
                downsamplePower = Mathf.RoundToInt(GUILayout.HorizontalSlider(downsamplePower, 5, 9, GUILayout.Width(125)));
                if (prevInt != downsamplePower) {
                    UpdateGlow();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Blur Iterations: {0}", blurIterations));
                GUILayout.FlexibleSpace();
                prevInt = blurIterations;
                blurIterations = Mathf.RoundToInt(GUILayout.HorizontalSlider(blurIterations, 1, 20, GUILayout.Width(125)));
                if (prevInt != blurIterations) {
                    UpdateGlow();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Blur Spread: {0}", decimal.Round((decimal)blurSpread, 2)));
                GUILayout.FlexibleSpace();
                float prevFloat = blurSpread;
                blurSpread = GUILayout.HorizontalSlider(blurSpread, 1, 2.5f, GUILayout.Width(125));
                if (prevFloat != blurSpread) {
                    UpdateGlow();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Glow Strength: {0}", decimal.Round((decimal)glowStrength, 2)));
                GUILayout.Width(50);
                prevFloat = glowStrength;
                glowStrength = GUILayout.HorizontalSlider(glowStrength, 0.5f, 10, GUILayout.Width(125));
                if (prevFloat != glowStrength) {
                    UpdateGlow();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.Label(string.Format("{0:F2} FPS", fps));
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void UpdateGlow()
        {
            glowEffect.enabled = enableGlow;
            if (enableGlow) {
                ActiveRecursively(glowGroup.transform, (int)glowMode % 2 == 0);
                ActiveRecursively(alphaGlowGroup.transform, (int)glowMode % 2 == 1);
                glowEffect.glowMode = glowMode;
                glowEffect.blendMode = blendMode;
                glowEffect.downsampleSize = (int)Mathf.Pow(2, downsamplePower);
                glowEffect.blurIterations = blurIterations;
                glowEffect.blurSpread = blurSpread;
                glowEffect.glowStrength = glowStrength;

                glowEffect.enabled = false;
                glowEffect.enabled = true;
            }
        }

        private void ActiveRecursively(Transform obj, bool active)
        {
            foreach (Transform child in obj) {
                ActiveRecursively(child, active);
            }
            obj.gameObject.SetActive(active);
        }

        // for fps:
        public void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;
            if (timeleft <= 0.0) {
                fps = accum / frames;
                timeleft = updateInterval;
                accum = 0.0F;
                frames = 0;
            }
        }
    }
}