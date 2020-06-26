using UnityEngine;
using UnityEditor;

namespace GlowEffect
{
    [CustomEditor(typeof(GlowEffect))]
    public class GlowEffectInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var glowEffect = target as GlowEffect;
            if (glowEffect == null) {
                return; // How'd this happen?
            }

            EditorGUI.BeginChangeCheck();

            // Glow Mode:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Glow Mode", GUILayout.Width(150));
            glowEffect.glowMode = (GlowEffect.GlowMode)EditorGUILayout.EnumPopup(glowEffect.glowMode);
            EditorGUILayout.EndHorizontal();

            // Glow Material:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Material", GUILayout.Width(150));
            glowEffect.glowMaterial = (Material)EditorGUILayout.ObjectField(glowEffect.glowMaterial, typeof(Material), false);
            EditorGUILayout.EndHorizontal();

            if (glowEffect.glowMode == GlowEffect.GlowMode.Glow || glowEffect.glowMode == GlowEffect.GlowMode.SimpleGlow) {
                // Glow Shader:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Replacement Shader", GUILayout.Width(150));
                glowEffect.glowReplaceShader = (Shader)EditorGUILayout.ObjectField(glowEffect.glowReplaceShader, typeof(Shader), false);
                EditorGUILayout.EndHorizontal();
            }

            // Blend Mode:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Blend Mode", GUILayout.Width(150));
            glowEffect.blendMode = (GlowEffect.BlendMode)EditorGUILayout.EnumPopup(glowEffect.blendMode);
            EditorGUILayout.EndHorizontal();

            // Downsample size. Must be a power of two
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Downsample Size", GUILayout.Width(150));
            glowEffect.downsampleSize = (int)Mathf.Pow(2, Mathf.RoundToInt(GUILayout.HorizontalSlider((int)Mathf.Log(glowEffect.downsampleSize, 2), 1, 12, GUILayout.MaxWidth(1000))));
            EditorGUILayout.LabelField(glowEffect.downsampleSize.ToString(), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            // Blur iterations
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Blur Iterations", GUILayout.Width(150));
            glowEffect.blurIterations = EditorGUILayout.IntSlider(glowEffect.blurIterations, 1, 20);
            EditorGUILayout.EndHorizontal();

            // Blur spread
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Blur Spread", GUILayout.Width(150));
            glowEffect.blurSpread = EditorGUILayout.Slider(glowEffect.blurSpread, 0, 3);
            EditorGUILayout.EndHorizontal();

            // Glow strength
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Glow Strength", GUILayout.Width(150));
            glowEffect.glowStrength = EditorGUILayout.Slider(glowEffect.glowStrength, 0.5f, 10f);
            EditorGUILayout.EndHorizontal();

            // Glow color multiplier
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Glow Color Multiplier", GUILayout.Width(150));
            glowEffect.glowColorMultiplier = EditorGUILayout.ColorField(glowEffect.glowColorMultiplier);
            EditorGUILayout.EndHorizontal();

            // Ignore layers (only applies to standard glow)
            if (glowEffect.glowMode == GlowEffect.GlowMode.Glow || glowEffect.glowMode == GlowEffect.GlowMode.SimpleGlow) {
                // Use a serialized property to display the layer mask correctly
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ignore Layers", GUILayout.Width(150));
                var serializedProperty = serializedObject.FindProperty("ignoreLayers");
                EditorGUILayout.PropertyField(serializedProperty, new GUIContent());
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(glowEffect);
            }
        }
    }
}