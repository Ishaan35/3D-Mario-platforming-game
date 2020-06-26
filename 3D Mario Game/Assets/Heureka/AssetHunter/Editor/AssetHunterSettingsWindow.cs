using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HeurekaGames.AssetHunter
{
    public class AssetHunterSettingsWindow : EditorWindow
    {
        private static AssetHunterSettingsWindow m_window;
        private static Color m_IntialGUIColor;
        private Vector2 scrollPos;

        [SerializeField]
        public AssetHunterSettings settings;
        private float btnMinWidthLarge = 200;
        private float btnMinWidthSmall = 80;

        private string m_excludeSubstringInput = string.Empty;

        public static AssetHunterSettingsWindow Instance
        {
            get
            {
                if (m_window != null)
                    return m_window;
                else
                {
                    return initializeWindow();
                };
            }
        }

        void OnInspectorUpdate()
        {
            if (!m_window)
                initializeWindow();
        }

        private static AssetHunterSettingsWindow initializeWindow()
        {
            m_IntialGUIColor = GUI.color;

            m_window = EditorWindow.GetWindow<AssetHunterSettingsWindow>();
            m_window.Show();

            return m_window;
        }

        private void OnGUI()
        {

            if (settings == null)
            {
                settings = AssetHunterSettings.LoadSettings();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            //Show all used types
            EditorGUILayout.BeginVertical();
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            //Make sure this window has focus to update contents
            AssetHunterSettingsWindow.Instance.Repaint();

            EditorGUILayout.Separator();
            GUILayout.Label("This is the settingswindow for Asset Hunter! " + System.Environment.NewLine + "-Choose folders, types or filenames to exclude when scanning the project", EditorStyles.boldLabel);
            GUILayout.Label("NB: If your project window is in \"Two column layout\" you need to select folders in the right hand side of that window", EditorStyles.miniLabel);

            GUILayout.Label("----------------------------------------------------------------------------", EditorStyles.boldLabel);
            //Force memorycleanup
            settings.m_MemoryCleanupActive = GUILayout.Toggle(settings.m_MemoryCleanupActive, "Force memory cleanup");
            GUILayout.Label("Enable this if you experience memory crashes (Much slower)", EditorStyles.miniLabel);
            GUILayout.Label("----------------------------------------------------------------------------", EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            //Do we have a folder selected
            bool bFolderSelected = System.IO.Directory.Exists(selectedPath);
            //Is it valid
            bool validSelection = (/*bFolderSelected && */settings.ValidateDirectory(Selection.activeObject));

            //Select folder to exclude
            EditorGUILayout.BeginHorizontal();

            GUI.color = (validSelection ? AssetHunterHelper.AH_BLUE : AssetHunterHelper.AH_GREY);

            if (GUILayout.Button(validSelection ? "Exclude selected folder" : "No valid folder selected", GUILayout.Width(btnMinWidthLarge)))
            {
                if (validSelection)
                {
                    AssetHunterSettings.ExcludeDirectory(Selection.activeObject);
                }
            }

            GUI.color = m_IntialGUIColor;

            if (validSelection)
                GUILayout.Label(selectedPath, EditorStyles.miniBoldLabel);

            GUI.color = m_IntialGUIColor;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            //Select type to exclude
            EditorGUILayout.BeginHorizontal();
            AssetHunterSerializableSystemType selectedType = null;

            if (Selection.activeObject)
                selectedType = new AssetHunterSerializableSystemType(Selection.activeObject.GetType());

            //Do we have a valid asset selected
            validSelection = (selectedType != null && !bFolderSelected && settings.ValidateType(selectedType));

            GUI.color = (validSelection ? AssetHunterHelper.AH_BLUE : AssetHunterHelper.AH_GREY);

            if (GUILayout.Button(validSelection ? "Exclude selected type" : "No valid type selected", GUILayout.Width(btnMinWidthLarge)))
            {
                if (validSelection)
                {
                    AssetHunterSettings.ExcludeType(selectedType);
                }
            }

            if (validSelection)
                GUILayout.Label(selectedType.SystemType.ToString(), EditorStyles.miniBoldLabel);

            GUI.color = m_IntialGUIColor;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            //Exluded filename substrings
            EditorGUILayout.BeginHorizontal();

            validSelection = !string.IsNullOrEmpty(m_excludeSubstringInput) && settings.ValidateSubstring(m_excludeSubstringInput);
            GUI.color = (validSelection ? AssetHunterHelper.AH_BLUE : AssetHunterHelper.AH_GREY);

            bool bHasHitEnter = false;

            Event e = Event.current;
            if (e.keyCode == KeyCode.Return) bHasHitEnter = true;

            if (bHasHitEnter || GUILayout.Button(validSelection ? "Exclude substring" : "No valid search string", GUILayout.Width(btnMinWidthLarge)))
            {
                if (validSelection)
                {
                    AssetHunterSettings.ExcludeSubstring(m_excludeSubstringInput);
                    //settings.ExcludeSubstring(m_excludeSubstringInput);
                    m_excludeSubstringInput = string.Empty;
                }
            }

            GUI.color = m_IntialGUIColor;
            m_excludeSubstringInput = GUILayout.TextField(m_excludeSubstringInput);

            //GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (validSelection)
                GUILayout.Label(string.Format("Will exclude any asset with \"{0}\" in its path/name (might make asset hunter perform slower)", m_excludeSubstringInput));

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            //Select type to exclude
            EditorGUILayout.BeginHorizontal();

            //Do we have a valid asset selected
            validSelection = Selection.activeObject != null && (settings.ValidateAssetGUID(Selection.activeObject));

            GUI.color = (validSelection ? AssetHunterHelper.AH_BLUE : AssetHunterHelper.AH_GREY);

            if (GUILayout.Button(validSelection ? "Exclude selected asset" : "No project asset selected", GUILayout.Width(btnMinWidthLarge)))
            {
                if (validSelection)
                {
                    AssetHunterSettings.ExcludeIndividualAssetWithObject(Selection.activeObject);
                }
            }

            if (validSelection)
                GUILayout.Label(Selection.activeObject.name, EditorStyles.miniBoldLabel);

            GUI.color = m_IntialGUIColor;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            GUILayout.Label("---------------------------Excluded Folders------------------------------", EditorStyles.boldLabel);

            if (settings.m_DirectoryExcludes.Count >= 1)
                for (int i = settings.m_DirectoryExcludes.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = AssetHunterHelper.AH_RED;

                    if (GUILayout.Button("Remove", GUILayout.Width(btnMinWidthSmall)))
                    {
                        settings.RemoveDirectoryAtIndex(i);
                        continue;
                    }
                    GUI.color = m_IntialGUIColor;
                    EditorGUILayout.ObjectField(settings.m_DirectoryExcludes[i], typeof(UnityEngine.Object), false);
                    EditorGUILayout.EndHorizontal();
                }
            else
            {
                EditorGUILayout.LabelField("No folders are currently excluded");
            }

            EditorGUILayout.Separator();
            GUILayout.Label("---------------------------Excluded Types--------------------------------", EditorStyles.boldLabel);

            if (settings.m_AssetTypeExcludes.Count >= 1)
                for (int i = settings.m_AssetTypeExcludes.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = AssetHunterHelper.AH_RED;
                    if (GUILayout.Button("Remove", GUILayout.Width(btnMinWidthSmall)))
                    {
                        settings.RemoveTypeAtIndex(i);
                        continue;
                    }
                    GUI.color = m_IntialGUIColor;
                    GUILayout.Label(settings.m_AssetTypeExcludes[i].Name);
                    EditorGUILayout.EndHorizontal();
                }
            else
            {
                EditorGUILayout.LabelField("No types are currently excluded");
            }

            EditorGUILayout.Separator();

            GUILayout.Label("---------------------------Excluded Substrings---------------------------", EditorStyles.boldLabel);
            if (settings.m_AssetSubstringExcludes.Count >= 1)
                for (int i = settings.m_AssetSubstringExcludes.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = AssetHunterHelper.AH_RED;
                    if (GUILayout.Button("Remove", GUILayout.Width(btnMinWidthSmall)))
                    {
                        settings.RemoveSubstringAtIndex(i);
                        continue;
                    }
                    GUI.color = m_IntialGUIColor;
                    GUILayout.Label(string.Format("\"{0}\"", settings.m_AssetSubstringExcludes[i]));
                    EditorGUILayout.EndHorizontal();
                }
            else
            {
                EditorGUILayout.LabelField("No substrings are currently excluded");
            }


            GUILayout.Label("-----------------------Individually excluded assets----------------------", EditorStyles.boldLabel);
            if (settings.m_AssetGUIDExcludes.Count >= 1)
            {
                bool isDirty = false;
                for (int i = settings.m_AssetGUIDExcludes.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = AssetHunterHelper.AH_RED;
                    if (GUILayout.Button("Remove", GUILayout.Width(btnMinWidthSmall)))
                    {
                        settings.RemoveAssetIDAtIndex(i);
                        continue;
                    }
                    GUI.color = m_IntialGUIColor;

                    string path = AssetDatabase.GUIDToAssetPath(settings.m_AssetGUIDExcludes[i]);
                    Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));

                    if (obj != null)
                        EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), false);
                    else
                        isDirty = true;
                        //GUILayout.Label(string.Format("\"{0}\"", "Asset cannot be found"));

                    EditorGUILayout.EndHorizontal();
                }

                if (Event.current.type != EventType.Layout && isDirty)
                    settings.CleanExcludedAssets();
            }
            else
            {
                EditorGUILayout.LabelField("No project assets are currently excluded");
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}