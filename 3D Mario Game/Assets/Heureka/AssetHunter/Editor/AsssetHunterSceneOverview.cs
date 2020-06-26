using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HeurekaGames.AssetHunter
{
    public class AssetHunterSceneOverview : EditorWindow
    {
        private static AssetHunterSceneOverview m_window;
        private static Color m_IntialGUIColor;
        private Vector2 scrollPos;

        [SerializeField]
        private float btnMinWidthSmall = 80;

        private List<String> m_allScenesInProject;
        private List<String> m_allScenesInBuildSettings;
        private List<String> m_allEnabledScenesInBuildSettings;
        private List<String> m_allUnreferencedScenes;
        private List<String> m_allDisabledScenesInBuildSettings;

        public static AssetHunterSceneOverview Instance
        {
            get
            {
                if (m_window != null)
                    return m_window;
                else
                {
                    return doInit();
                }
                ;
            }
        }

        void OnInspectorUpdate()
        {
            if (!m_window)
                doInit();
        }

        private static AssetHunterSceneOverview doInit()
        {
            m_IntialGUIColor = GUI.color;

            m_window = EditorWindow.GetWindow<AssetHunterSceneOverview>();
            m_window.Show();

            m_window.GetSceneInfo();

            return m_window;
        }

        private void GetSceneInfo()
        {
            m_allScenesInProject = AssetHunterHelper.GetAllSceneNames().ToList<string>();
            m_allScenesInBuildSettings = AssetHunterHelper.GetAllSceneNamesInBuild().ToList<string>();
            m_allEnabledScenesInBuildSettings = AssetHunterHelper.GetEnabledSceneNamesInBuild().ToList<string>();
            m_allDisabledScenesInBuildSettings = SubtractSceneArrays(m_allScenesInBuildSettings, m_allEnabledScenesInBuildSettings);
            m_allUnreferencedScenes = SubtractSceneArrays(m_allScenesInProject, m_allScenesInBuildSettings);
        }

        //Get the subset of scenes where we subtract "secondary" from "main"
        private List<String> SubtractSceneArrays(List<String> main, List<String> secondary)
        {
            return main.Except<string>(secondary).ToList<string>();
        }

        private void OnFocus()
        {
            GetSceneInfo();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            //Show all used types
            EditorGUILayout.BeginVertical();

            //Make sure this window has focus to update contents
            AssetHunterSceneOverview.Instance.Repaint();

            EditorGUILayout.Separator();
            GUILayout.Label("See which scenes are used and which are unused with your current build settings ", EditorStyles.boldLabel);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            GUI.color = AssetHunterHelper.AH_RED;
            GUILayout.Label("---------------------------Unreferenced scenes--------------------------", EditorStyles.whiteLargeLabel);
            GUILayout.Label("These scenes are not referenced anywhere in build settings", EditorStyles.whiteLabel);
            drawScenes(m_allUnreferencedScenes, true);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            GUI.color = AssetHunterHelper.AH_YELLOW1;
            GUILayout.Label("---------------------------Disabled scenes------------------------------", EditorStyles.whiteLargeLabel);
            GUILayout.Label("These scenes are added to build settings but disabled", EditorStyles.whiteLabel);
            drawScenes(m_allDisabledScenesInBuildSettings, false); EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            GUI.color = AssetHunterHelper.AH_GREEN;
            GUILayout.Label("---------------------------Enabled scenes------------------------------", EditorStyles.whiteLargeLabel);
            GUILayout.Label("These scenes are added to build settings and enabled", EditorStyles.whiteLabel);
            drawScenes(m_allEnabledScenesInBuildSettings, false);



            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void drawScenes(List<string> scenelist, bool allowDeleteAll)
        {
            GUI.color = m_IntialGUIColor;
            //EditorGUILayout.Separator();

            if (scenelist.Count > 0)
            {
                GUI.color = AssetHunterHelper.AH_RED;

                if (allowDeleteAll)
                    if (GUILayout.Button("Delete all", GUILayout.Width(btnMinWidthSmall)))
                    {
                        //TODO ARE YOU SURE
                        foreach (string scenePath in scenelist)
                            AssetDatabase.DeleteAsset(scenePath);

                        scenelist.Clear();

                        return;
                    }

                EditorGUILayout.Separator();
                foreach (string scenePath in scenelist)
                {
                    EditorGUILayout.BeginHorizontal();

                    GUI.color = m_IntialGUIColor;
                    if (GUILayout.Button("Select", GUILayout.Width(btnMinWidthSmall)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(scenePath, typeof(UnityEngine.Object));
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }

                    GUI.color = m_IntialGUIColor;
                    EditorGUILayout.LabelField(scenePath);

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
                EditorGUILayout.LabelField("*NO SUCH SCENES IN PROJECT*");
        }
    }
}