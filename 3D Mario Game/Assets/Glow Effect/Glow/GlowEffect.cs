using UnityEngine;

namespace GlowEffect
{
    public class GlowEffect : MonoBehaviour
    {
        public enum GlowMode { Glow, AlphaGlow, SimpleGlow, SimpleAlphaGlow }
        public enum BlendMode { Additive, Multiply, Screen, Subtract }

        public Material glowMaterial;
        public Shader glowReplaceShader;

        /**
         * Glow: The standard way of generating glow by using a separate camera and replacement shaders. This method looks the best but it takes the most resources.
         * Simple Glow: Similar to Standard Glow except it uses less draw calls and less memory. This effect does everything in one pass instead of multiple passes to generate a blur for the glow.
         * Alpha Glow:  Recommended method for mobile devices. This method uses the object's alpha channel in order to determine the amount of glow to apply. 
         *              This method looks comparable to standard glow however it uses the object's alpha channel. This method does not require a separate camera.
         * Simple Alpha Glow: Smilar to Alpha Glow except it further reduces the amount of draw calls and memory by generating the glow in one pass. This method looks the worst out of the four and should be used as the last resort.
         **/
        public GlowMode glowMode;

        // Additive, Multiply, Screen 
        public BlendMode blendMode;

        // The size that the render texture is resized to when performing the glow. Must be a power of two
        public int downsampleSize = 256;

        // The number of times the glow texture should be blurred. The more blur iterations the wider the glow. This value is only used if useSimpleGlow is false.
        public int blurIterations = 4;
        public int BlurIterations { get { return blurIterations; } set { blurIterations = value; } }

        // The distance of the samples taken for the blurred glow. Too big of a value will cause noise in the blur. This value is only used if useSimpleGlow is false. 
        public float blurSpread = 1.0f;
        public float BlurSpread { get { return blurSpread; } set { blurSpread = value; UpdateGlowMaterial(); } }

        // Multiplies the glow color by this value.
        public float glowStrength = 1.2f;
        public float GlowStrength { get { return glowStrength; } set { glowStrength = value; UpdateGlowMaterial(); } }

        // Cull the layer assigned to this value with the standard glow camera.
        public LayerMask ignoreLayers;

        // Multiplies the glow color by this color.
        public Color glowColorMultiplier = Color.white;
        public Color GlowColorMultiplier { get { return glowColorMultiplier; } set { glowColorMultiplier = value; UpdateGlowMaterial(); } }

        private Camera origCamera;
        private Camera shaderCamera;
        private int shaderCullingMask;
        private Rect normalizedRect;
        private RenderTexture replaceRenderTexture;

        public void Awake()
        {
            origCamera = GetComponent<Camera>();
        }

        public void Start()
        {
            // Disable if we don't support image effects
            if (!SystemInfo.supportsImageEffects) {
                Debug.Log("Disabling the Glow Effect. Image effects are not supported (do you have Unity Pro?)");
                enabled = false;
            }

            normalizedRect = new Rect(0, 0, 1, 1);
        }

        public void OnEnable()
        {
            switch (blendMode) {
                case BlendMode.Additive:
                    Shader.EnableKeyword("GLOWEFFECT_BLEND_ADDITIVE");
                    break;
                case BlendMode.Multiply:
                    Shader.EnableKeyword("GLOWEFFECT_BLEND_MULTIPLY");
                    break;
                case BlendMode.Screen:
                    Shader.EnableKeyword("GLOWEFFECT_BLEND_SCREEN");
                    break;
                case BlendMode.Subtract:
                    Shader.EnableKeyword("GLOWEFFECT_BLEND_SUBTRACT");
                    break;
            }

            if ((int)glowMode % 2 == 0) { // glow or simple glow
                replaceRenderTexture = new RenderTexture((int)origCamera.pixelWidth, (int)origCamera.pixelHeight, 16, RenderTextureFormat.ARGB32);
                replaceRenderTexture.wrapMode = TextureWrapMode.Clamp;
                replaceRenderTexture.useMipMap = false;
                replaceRenderTexture.filterMode = FilterMode.Bilinear;
                replaceRenderTexture.Create();

                glowMaterial.SetTexture("_Glow", replaceRenderTexture);

                shaderCamera = new GameObject("Glow Effect", typeof(Camera)).GetComponent<Camera>();
                shaderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                shaderCullingMask = ~ignoreLayers;
            }

            UpdateGlowMaterial();
        }

        private void UpdateGlowMaterial()
        {
            glowMaterial.SetFloat("_BlurSpread", blurSpread);
            glowMaterial.SetFloat("_GlowStrength", glowStrength);
            glowMaterial.SetColor("_GlowColorMultiplier", glowColorMultiplier);
        }

        public void OnDisable()
        {
            glowMaterial.mainTexture = null;
            origCamera.targetTexture = null;
            DestroyObject(shaderCamera);
            DisableShaderKeywords();
        }

        public void OnPreRender()
        {
            if ((int)glowMode % 2 == 0) {
                shaderCamera.CopyFrom(origCamera);
                shaderCamera.backgroundColor = Color.clear;
                shaderCamera.clearFlags = CameraClearFlags.SolidColor;
                shaderCamera.renderingPath = RenderingPath.Forward;
                shaderCamera.targetTexture = replaceRenderTexture;
                shaderCamera.rect = normalizedRect;
                shaderCamera.cullingMask = shaderCullingMask;
                shaderCamera.RenderWithShader(glowReplaceShader, "RenderType");
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            CalculateGlow(source, destination);
        }

        private void CalculateGlow(RenderTexture source, RenderTexture destination)
        {
            if ((int)glowMode < 2) {
                // blur
                RenderTexture blurA = RenderTexture.GetTemporary(downsampleSize, downsampleSize, 0, RenderTextureFormat.ARGB32);
                RenderTexture blurB = RenderTexture.GetTemporary(downsampleSize, downsampleSize, 0, RenderTextureFormat.ARGB32);
                if (blurIterations % 2 == 0) {
                    glowMaterial.SetTexture("_Glow", blurA);
                } else {
                    glowMaterial.SetTexture("_Glow", blurB);
                }
                if ((int)glowMode % 2 == 1) {
                    Graphics.Blit(source, blurB, glowMaterial, 2);
                } else {
                    Graphics.Blit(replaceRenderTexture, blurB, glowMaterial, 1);
                }
                for (int i = 1; i < blurIterations; ++i) {
                    if (i % 2 == 0) {
                        blurB.DiscardContents();
                        Graphics.Blit(blurA, blurB, glowMaterial, 1);
                    } else {
                        blurA.DiscardContents();
                        Graphics.Blit(blurB, blurA, glowMaterial, 1);
                    }
                }
                // calculate glow
                Graphics.Blit(source, destination, glowMaterial, 0);
                RenderTexture.ReleaseTemporary(blurA);
                RenderTexture.ReleaseTemporary(blurB);
            } else {
                Graphics.Blit(source, destination, glowMaterial, (((int)glowMode % 2 == 1) ? 4 : 3));
            }
        }

        private void DisableShaderKeywords()
        {
            Shader.DisableKeyword("GLOWEFFECT_BLEND_ADDITIVE");
            Shader.DisableKeyword("GLOWEFFECT_BLEND_SCREEN");
            Shader.DisableKeyword("GLOWEFFECT_BLEND_MULTIPLY");
            Shader.DisableKeyword("GLOWEFFECT_BLEND_SUBTRACT");

            Shader.DisableKeyword("GLOWEFFECT_USE_MAINTEX");
            Shader.DisableKeyword("GLOWEFFECT_USE_MAINTEX_OFF");
            Shader.DisableKeyword("GLOWEFFECT_USE_GLOWTEX");
            Shader.DisableKeyword("GLOWEFFECT_USE_GLOWTEX_OFF");
            Shader.DisableKeyword("GLOWEFFECT_USE_GLOWCOLOR");
            Shader.DisableKeyword("GLOWEFFECT_USE_GLOWCOLOR_OFF");
            Shader.DisableKeyword("GLOWEFFECT_USE_VERTEXCOLOR");
            Shader.DisableKeyword("GLOWEFFECT_USE_VERTEXCOLOR_OFF");

            Shader.DisableKeyword("GLOWEFFECT_MULTIPLY_COLOR");
            Shader.DisableKeyword("GLOWEFFECT_MULTIPLY_COLOR_OFF");
        }
    }
}