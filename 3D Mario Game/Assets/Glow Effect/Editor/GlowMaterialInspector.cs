using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class GlowMaterialInspector : MaterialEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!isVisible)
            return;

        var material = target as Material;
        ShowGlowMaterialKeywords(material);
    }

    public static void ShowGlowMaterialKeywords(Material material)
    {
        if (material.shaderKeywords == null || material.shaderKeywords.Length == 0) {
            material.shaderKeywords = new string[] { "GLOWEFFECT_USE_MAINTEX_OFF", "GLOWEFFECT_USE_GLOWTEX_OFF", "GLOWEFFECT_USE_GLOWCOLOR_OFF", 
                                                      "GLOWEFFECT_USE_VERTEXCOLOR_OFF", "GLOWEFFECT_MULTIPLY_COLOR_OFF" };
            EditorUtility.SetDirty(material);
        }

        ShowToggleGUI(material, "Glow using Main Texture", "GLOWEFFECT_USE_MAINTEX");
        ShowToggleGUI(material, "Glow using Glow Texture", "GLOWEFFECT_USE_GLOWTEX");
        ShowToggleGUI(material, "Glow using Glow Color", "GLOWEFFECT_USE_GLOWCOLOR");
        ShowToggleGUI(material, "Glow using Vertex Color", "GLOWEFFECT_USE_VERTEXCOLOR");
        ShowToggleGUI(material, "Multiply Glow by Glow Color Multiplyer", "GLOWEFFECT_MULTIPLY_COLOR");
    }

    private static void ShowToggleGUI(Material material, string friendlyName, string keyword)
    {
        var shaderKeywords = material.shaderKeywords.OfType<string>().ToList();
        int index = -1;
        for (int i = 0; i < shaderKeywords.Count; ++i) {
            if (shaderKeywords[i].Contains(keyword)) {
                index = i;
                break;
            }
        }
        bool keywordEnabled = shaderKeywords.Contains(keyword);
        EditorGUI.BeginChangeCheck();
        keywordEnabled = EditorGUILayout.Toggle(friendlyName, keywordEnabled);
        if (EditorGUI.EndChangeCheck()) {
            if (keywordEnabled) {
                shaderKeywords[index] = keyword;
            } else {
                shaderKeywords[index] = string.Format("{0}_OFF", keyword);
            }
            material.shaderKeywords = shaderKeywords.ToArray();
            EditorUtility.SetDirty(material);
        }
    }
}