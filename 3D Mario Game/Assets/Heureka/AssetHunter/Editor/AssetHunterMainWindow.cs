using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HeurekaGames.AssetHunter
{
    public class AssetHunterMainWindow : EditorWindow, ISerializationCallbackReceiver
    {
        public enum AssetHunterWindowState
        {
            BuildReport,
            UnusedAssets,
            UnusedScripts
        }

        [SerializeField]
        public AssetHunterSettings settings;

        private AssetHunterWindowState m_WindowState = AssetHunterWindowState.UnusedAssets;
        private static AssetHunterMainWindow m_window;
        private Vector2 scrollPos;

        //If we should should the excluded folders/types foldout
        private bool bShowExcludeFoldout;

        [UnityEngine.SerializeField]
        public List<AssetHunterProjectFolderInfo> m_ProjectFolderList = new List<AssetHunterProjectFolderInfo>();

        //Dictionary and list to help serialization of unusedTypeDict
        public List<AssetHunterSerializableSystemType> m_unusedTypeListKeysSerializer = new List<AssetHunterSerializableSystemType>();
        public List<bool> m_unusedTypeListValuesSerializer = new List<bool>();
        public SortedDictionary<AssetHunterSerializableSystemType, bool> m_unusedTypeDict = new SortedDictionary<AssetHunterSerializableSystemType, bool>(new SerializableSystemTypeComparer());

        //Dictionary and list to help serialization of unusedTypeDict
        public List<AssetHunterSerializableSystemType> m_usedTypeListKeysSerializer = new List<AssetHunterSerializableSystemType>();
        public List<bool> m_usedTypeListValuesSerializer = new List<bool>();
        public SortedDictionary<AssetHunterSerializableSystemType, bool> m_usedTypeDict = new SortedDictionary<AssetHunterSerializableSystemType, bool>(new SerializableSystemTypeComparer());

        //Dictionary that holds the dependencies of assets and the scenes that contain them
        public List<string> m_assetSceneDependencyKeysSerializer = new List<string>();
        public List<StringListWrapper> m_assetSceneDependencyValueSerializer = new List<StringListWrapper>();
        public Dictionary<string, List<string>> m_assetSceneDependencies = new Dictionary<string, List<string>>();


        //Workaround to serialize dictionaries in Unity
        #region DictionarySerialization

        public void OnBeforeSerialize()
        {
            m_unusedTypeListKeysSerializer.Clear();
            m_unusedTypeListValuesSerializer.Clear();
            foreach (var kvp in m_unusedTypeDict)
            {
                m_unusedTypeListKeysSerializer.Add(kvp.Key);
                m_unusedTypeListValuesSerializer.Add(kvp.Value);
            }

            m_usedTypeListKeysSerializer.Clear();
            m_usedTypeListValuesSerializer.Clear();
            foreach (var kvp in m_usedTypeDict)
            {
                m_usedTypeListKeysSerializer.Add(kvp.Key);
                m_usedTypeListValuesSerializer.Add(kvp.Value);
            }

            m_assetSceneDependencyKeysSerializer.Clear();
            m_assetSceneDependencyValueSerializer.Clear();
            foreach (var kvp in m_assetSceneDependencies)
            {
                m_assetSceneDependencyKeysSerializer.Add(kvp.Key);
                m_assetSceneDependencyValueSerializer.Add(new StringListWrapper(kvp.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            m_folderMarkedForDeletion = null;
            m_assetMarkedForDeletion = null;

            m_unusedTypeDict.Clear();
            for (int i = 0; i != Mathf.Min(m_unusedTypeListKeysSerializer.Count, m_unusedTypeListValuesSerializer.Count); i++)
            {
                m_unusedTypeDict.Add(m_unusedTypeListKeysSerializer[i], m_unusedTypeListValuesSerializer[i]);
            }

            m_usedTypeDict.Clear();
            for (int i = 0; i != Mathf.Min(m_usedTypeListKeysSerializer.Count, m_usedTypeListValuesSerializer.Count); i++)
            {
                m_usedTypeDict.Add(m_usedTypeListKeysSerializer[i], m_usedTypeListValuesSerializer[i]);
            }

            m_assetSceneDependencies.Clear();
            for (int i = 0; i != Mathf.Min(m_assetSceneDependencyKeysSerializer.Count, m_assetSceneDependencyValueSerializer.Count); i++)
            {
                m_assetSceneDependencies.Add(m_assetSceneDependencyKeysSerializer[i], m_assetSceneDependencyValueSerializer[i].list);
            }
        }
        #endregion

        private bool m_showTypesFoldout = true;
        private bool m_showAssemblyFoldout = false;

        [SerializeField]
        private AssetObjectInfo m_assetMarkedForDeletion = null;
        [SerializeField]
        public AssetHunterProjectFolderInfo m_folderMarkedForDeletion = null;

        public bool m_BuildLogExists { get; set; }
        public bool m_BuildLogLoaded { get; set; }

        private Texture2D m_UIWarning;
        private Texture2D m_UISmallLogo;
        private Texture2D m_UIWideLogo;
        private Texture2D m_UIAchievementIcon;
        private Texture2D m_UISceneSelect;
        private Texture2D m_UIFolderSelect;
        private Texture2D m_UISettings;

        [SerializeField]
        private AssetHunterBuildReport m_BuildLog;

        [SerializeField]
        private bool m_newBuildReady;
        private bool m_TypeChangeDetected;

        //BtnWidth
        float btnMinWidth = 180;
        int btnImageSize = 16;

        //Save initial Color
        private static Color m_IntialGUIColor;
        //private List<Type> m_UsedScriptList;
        //private List<Type> m_UnusedScriptList;

        //Add menu named "Asset Hunter" to the window menu  
        [UnityEditor.MenuItem("Window/Asset Hunter _%h", priority = 1)]
        public static void OpenAssetHúnter()
        {
            if (!m_window)
                initializeWindow();
        }

        private static AssetHunterMainWindow initializeWindow()
        {
            m_IntialGUIColor = GUI.color;

            m_window = EditorWindow.GetWindow<AssetHunterMainWindow>();
            m_window.Show();
            loadEditorResources();

            m_window.m_BuildLogExists = AssetHunterHelper.HasBuildLogAvaliable();

            return m_window;
        }

        private static void loadEditorResources()
        {
            //Small logo
            string[] GUIDsmallLogo = AssetDatabase.FindAssets("AssetHunterLogoSmall t:texture2D", null);
            if (GUIDsmallLogo.Length >= 1)
            {
                m_window.m_UISmallLogo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDsmallLogo[0]), typeof(Texture2D)) as Texture2D;
                m_window.titleContent.image = m_window.m_UISmallLogo;
                m_window.titleContent.text = "Asset Hunter";
            }

            //Warning
            string[] GUIDwarning = AssetDatabase.FindAssets("AssetHunterWarning t:texture2D", null);
            if (GUIDwarning.Length >= 1)
            {
                m_window.m_UIWarning = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDwarning[0]), typeof(Texture2D)) as Texture2D;
            }

            //wide logo
            string[] GUIDwideLogo = AssetDatabase.FindAssets("AssetHunterLogoWide t:texture2D", null);
            if (GUIDwideLogo.Length >= 1)
            {
                m_window.m_UIWideLogo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDwideLogo[0]), typeof(Texture2D)) as Texture2D;
            }

            //Achievement
            string[] GUIDachievement = AssetDatabase.FindAssets("AssetHunterAchievementUnlocked t:texture2D", null);
            if (GUIDachievement.Length >= 1)
            {
                m_window.m_UIAchievementIcon = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDachievement[0]), typeof(Texture2D)) as Texture2D;
            }

            //SceneSelect
            string[] GUIDsceneSelect = AssetDatabase.FindAssets("AssetHunterSceneSelect t:texture2D", null);
            if (GUIDsceneSelect.Length >= 1)
            {
                m_window.m_UISceneSelect = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDsceneSelect[0]), typeof(Texture2D)) as Texture2D;
            }

            //FolderSelect
            string[] GUIDfolderSelect = AssetDatabase.FindAssets("AssetHunterFolderSelect t:texture2D", null);
            if (GUIDfolderSelect.Length >= 1)
            {
                m_window.m_UIFolderSelect = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDfolderSelect[0]), typeof(Texture2D)) as Texture2D;
            }

            //Settings
            string[] GUIDsettings = AssetDatabase.FindAssets("AssetHunterSettings t:texture2D", null);
            if (GUIDsettings.Length >= 1)
            {
                m_window.m_UISettings = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDsettings[0]), typeof(Texture2D)) as Texture2D;
            }

            string path = AssetHunterSettingsCreator.GetAssetPath();
            AssetHunterMainWindow.Instance.settings = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as AssetHunterSettings;
                
        }

        void OnInspectorUpdate()
        {
            if (!m_window)
                initializeWindow();
        }

        void OnGUI()
        {

            showDefaultHeader();
            showDefaultBody();

            if (m_BuildLogLoaded)
            {
                if (m_WindowState == AssetHunterWindowState.UnusedAssets)
                    showUnusedAssetsUI();
                else if (m_WindowState == AssetHunterWindowState.BuildReport)
                    showBuildReportUI();
            }

            //No need for build to look at unused scripts
            /*if (m_WindowState == AssetHunterWindowState.UnusedScripts)
                OnUnusedScriptsUIUpdate();*/
        }

        private void showDefaultHeader()
        {
            EditorGUILayout.LabelField("Asset Hunter v2.4", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            //BUTTON Settings
            EditorGUILayout.Space();
            // GUI.color = AssetHunterHelper.AH_BLUE;
            if (GUILayout.Button(new GUIContent("Edit settings", m_UISettings), GUILayout.Width(btnMinWidth - 70), GUILayout.Height(20)))
            {
                EditorWindow.GetWindow<AssetHunterSettingsWindow>(true, "Asset Hunter Settings");
            }

            //BUTTON Open Log
            EditorGUILayout.Space();
            //GUI.color = AssetHunterHelper.AH_RED;    
            if (GUILayout.Button("Open log", GUILayout.MinWidth(100)))
            {
                System.Diagnostics.Process.Start(AssetHunterHelper.GetLogFolderPath());
            }

            //BUTTON delete empty folders
            EditorGUILayout.Space();
            //GUI.color = AssetHunterHelper.AH_BLUE;
            if (GUILayout.Button("Delete empty folders", GUILayout.MinWidth(120)))
            {
                if (EditorUtility.DisplayDialog("Delete empty folder", "Are you sure you want to delete all empty folders", "Yes", "No"))
                {
                    string path = Application.dataPath;
                    int deleteCount = 0;
                    deleteEmptyDirectories(path, ref deleteCount);

                    Debug.LogWarning(deleteCount + " empty folders was deleted by Asset Hunter");
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            //BUTTON Settings
            EditorGUILayout.Space();
            if (GUILayout.Button("Scene overview", GUILayout.Width(btnMinWidth - 70), GUILayout.Height(20)))
            {
                EditorWindow.GetWindow<AssetHunterSceneOverview>(true, "Asset Hunter Scene Overview");
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUI.color = m_IntialGUIColor;

            //Show logo
            if (m_window && m_window.m_UIWideLogo)
                GUILayout.Label(m_window.m_UIWideLogo);

            //Only show the foldout if we actually have any manually excluded folders or types
            if (settings != null && settings.HasExcludes())
                bShowExcludeFoldout = EditorGUILayout.Foldout(bShowExcludeFoldout, "Show manual excludes");

            EditorGUILayout.EndVertical();
        }

        private void showDefaultBody()
        {
            EditorGUILayout.BeginVertical();

            //Draw excluded types foldout
            if (bShowExcludeFoldout)
            {
                GUILayout.Label("-------------------------------------------------------------------------");
                if (settings.HasDirectoryExcludes())
                {
                    GUI.color = AssetHunterHelper.AH_BLUE;
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.LabelField("Excluded Directories", EditorStyles.whiteBoldLabel);
                    GUI.color = m_IntialGUIColor;
                    EditorGUI.indentLevel = 2;
                    foreach (UnityEngine.Object obj in settings.m_DirectoryExcludes)
                        EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(obj), EditorStyles.miniLabel);
                }
                if (settings.HasTypeExcludes())
                {
                    GUI.color = AssetHunterHelper.AH_BLUE;
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.LabelField("Excluded Types", EditorStyles.whiteBoldLabel);
                    GUI.color = m_IntialGUIColor;
                    EditorGUI.indentLevel = 2;
                    foreach (AssetHunterSerializableSystemType sType in settings.m_AssetTypeExcludes)
                    {
                        EditorGUILayout.LabelField(sType.Name);
                    }
                }
                if (settings.HasSubStringExcludes())
                {
                    GUI.color = AssetHunterHelper.AH_BLUE;
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.LabelField("Excluded Strings", EditorStyles.whiteBoldLabel);
                    GUI.color = m_IntialGUIColor;
                    EditorGUI.indentLevel = 2;
                    foreach (string substring in settings.m_AssetSubstringExcludes)
                    {
                        EditorGUILayout.LabelField(substring);
                    }
                }
                GUILayout.Label("-------------------------------------------------------------------------");
                GUILayout.Space(10);
            }

            //If there is no valid build log
            if (!m_BuildLogExists)
            {
                m_BuildLogExists = AssetHunterHelper.HasBuildLogAvaliable();

                if (!m_BuildLogExists)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(m_UIWarning);
                    GUILayout.Label("Asset Hunter needs a recent build in order to work", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Label("Create a build (Ctrl+Shift+B)");

                    //requires unity 5 to work
                    if (GUILayout.Button("Open Build Settings"))
                    {
                        EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                    }

                    GUILayout.FlexibleSpace();
                    return;
                }
            }

            string buildLogButtonText;

            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();

            GUILayout.Label("-------------------------------Build Info--------------------------------");

            //If build log up to date
            if (!m_newBuildReady)
            {
                buildLogButtonText = m_BuildLogLoaded ? "Log updated (refresh)" : "Load Build Log (Required)";
                GUI.color = m_BuildLogLoaded ? AssetHunterHelper.AH_GREEN : AssetHunterHelper.AH_RED;
            }

            //If build log outdated
            else
            {
                buildLogButtonText = "Log outdated(Refresh)";
                GUI.color = AssetHunterHelper.AH_YELLOW1;
            }

            //Load the Editor build log
            if (GUILayout.Button(buildLogButtonText, GUILayout.Width(375)))
            {
                loadEditorLog();
                return;
            }
            //GUILayout.FlexibleSpace();
            EditorGUILayout.Space();

            GUILayout.Label("------------------------------Select Mode------------------------------");
            EditorGUILayout.BeginHorizontal();

            //Choose window state
            GUI.color = (m_WindowState == AssetHunterWindowState.UnusedAssets) ? AssetHunterHelper.AH_GREY : m_IntialGUIColor;
            if (GUILayout.Button(AssetHunterWindowState.UnusedAssets.ToString(), GUILayout.MinWidth(btnMinWidth)))
            {
                changeState(AssetHunterWindowState.UnusedAssets);
            }
            EditorGUILayout.Space();

            GUI.color = (m_WindowState == AssetHunterWindowState.BuildReport) ? AssetHunterHelper.AH_GREY : m_IntialGUIColor;
            if (GUILayout.Button(AssetHunterWindowState.BuildReport.ToString(), GUILayout.MinWidth(btnMinWidth)))
            {
                //Shot buildreport
                changeState(AssetHunterWindowState.BuildReport);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            #region scriptdetection
            /*EditorGUILayout.BeginHorizontal();

            GUI.color = AssetHunterHelper.AH_RED;
            if (GUILayout.Button(AssetHunterWindowState.UnusedScripts.ToString() + " (WIP - USE WITH CONSIDERATION)", GUILayout.MinWidth(btnMinWidth * 2 + 14)))
            {

                GUI.color = m_IntialGUIColor;
                //Get added scripts
                //List<Type> scriptAssemblyTypes = AssetHunterHelper.GetScriptAssembly();

                //Find all enabled scenes in buildsettings
                EditorBuildSettingsScene[] activeScenes = EditorBuildSettings.scenes.Where(val => val.enabled == true).ToArray<EditorBuildSettingsScene>();


                //All script dependencies for all enabled levels in buildsettings
                m_UsedScriptList = AssetHunterHelper.GetScriptLevelDependencies(activeScenes);

                //Find ALL scripts in scriptAssembly
                //TODO ADD THIS TO ATTEMPT TO LOCATE UNUSED CODE
                //m_UnusedScriptList = AssetHunterHelper.GetAddedComponents();

                //Remove "Used Scripts" from list
                m_UnusedScriptList.RemoveAll(val => m_UsedScriptList.Contains(val));

                changeState(AssetHunterWindowState.UnusedScripts);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();*/

            #endregion

            //Reset GUI Color
            GUI.color = m_IntialGUIColor;
        }

        private void changeState(AssetHunterWindowState newState)
        {
            m_WindowState = newState;
        }

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (m_window)
            {
                m_window.m_newBuildReady = true;
            }
        }

        private void showBuildReportUI()
        {
            GUILayout.Label("Select UnusedAssets to view the project assets not used in last build");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            //Show all used types
            showTypesUI(m_usedTypeDict);
            //Show included assemblies
            showDependenciesUI();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //Show the used assets
            showBuildAssetInfoUI();

            if (m_TypeChangeDetected)
            {
                //Type change detected
            };

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
        }

        private void showUnusedAssetsUI()
        {
            if (m_ProjectFolderList == null || m_ProjectFolderList.Count <= 0)
                return;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            if (m_unusedTypeDict.Count >= 1)
            {
                GUILayout.Label("Select BuildReport to view the project assets included in the last build");
                showTypesUI(m_unusedTypeDict);

                if (m_TypeChangeDetected)
                {
                    m_ProjectFolderList[0].RecalcChildAssets(m_unusedTypeDict);
                };

                showUnusedAssets();
            }
            else
            {
                GUILayout.Label("Good job!!! Your project is completely clean. Give yourself a pat on the back", EditorStyles.boldLabel);
                GUILayout.Label(m_UIAchievementIcon);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        #region scriptDetection

        /*private void OnUnusedScriptsUIUpdate()
        {
            if (m_UsedScriptList == null || m_UsedScriptList.Count <= 0)
                return;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("List of unused scripts", EditorStyles.boldLabel);
            showUnusedScriptsUI();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }*/

        /*private void showUnusedScriptsUI()
        {

            //TODO
            //HERE IS A WAY TO FIND ALL CLASSES, Used, and Unused
            //http://answers.unity3d.com/questions/30382/editor-assembly.html

            foreach (Type t in m_UnusedScriptList)
            {
                GUILayout.BeginHorizontal();
                MonoScript script = null;

                if (GUILayout.Button(t.ToString(), GUILayout.Width(btnMinWidth * 2 + 14)))
                {
                    MonoScript[] scripts = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));
                    foreach (MonoScript m in scripts)
                    {
                        if (m.GetClass() == t)
                            script = m;
                    }
                    Selection.activeObject = script;
                }
                GUILayout.EndHorizontal();
            }
        }*/

        #endregion

        private void showUnusedAssets()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Collapse All", GUILayout.Width(btnMinWidth)))
            {
                foreach (AssetHunterProjectFolderInfo folder in m_ProjectFolderList)
                {
                    folder.FoldOut = false;
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Expand All", GUILayout.Width(btnMinWidth)))
            {
                foreach (AssetHunterProjectFolderInfo folder in m_ProjectFolderList)
                {
                    folder.FoldOut = true;
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            int indentLevel = 0;

            drawAssetFolderInfoRecursively(m_ProjectFolderList[0], indentLevel);

            if (m_assetMarkedForDeletion != null)
            {
                if (EditorUtility.DisplayDialog("Delete asset", "Are you sure you want to delete " + m_assetMarkedForDeletion.m_Name, "Yes", "No"))
                {
                    m_assetMarkedForDeletion.Delete(m_unusedTypeDict);
                    //Delete potential empty folders
                    int deleteCount = 0;
                    deleteEmptyDirectories(getSystemFolderPath(m_assetMarkedForDeletion.m_ParentPath), ref deleteCount);
                    m_assetMarkedForDeletion = null;
                }
                else
                    m_assetMarkedForDeletion = null;
            }
            else if (m_folderMarkedForDeletion != null)
            {
                int dialogChoice = EditorUtility.DisplayDialogComplex(
                      "Delete all unused assets in folder",
                      "You can delete all assets below this folder, or you can choose to create a backup Unitypackage with all your deleted assets (which will be slow).\n\nRegardless you should always keep backups or preferably have your project under version control so actions such as mass-delete can be reverted",
                      "Delete All",
                      "Backup Unitypackage (Slow!)",
                      "Cancel");

                switch (dialogChoice)
                {
                    //Normal delete
                    case 0:
                        deleteAllInFolder(m_folderMarkedForDeletion, false);
                        break;
                    //Delete with backup
                    case 1:
                        deleteAllInFolder(m_folderMarkedForDeletion, true);
                        break;
                    //Cancel
                    case 2:
                        m_folderMarkedForDeletion = null;
                        break;
                    default:
                        Debug.LogError("Unrecognized option.");
                        break;
                }
            }
        }

        //Delete all folder recursively
        private void deleteAllInFolder(AssetHunterProjectFolderInfo folder, bool shouldBackup)
        {
            m_folderMarkedForDeletion = null;

            List<AssetObjectInfo> objectsToDelete = new List<AssetObjectInfo>();
            getObjectsMarkedForDeletion(folder, ref objectsToDelete);

            //Create backup
            if (shouldBackup)
                createBackup(objectsToDelete, folder.GetTopFolderName());

            //Delete assets
            deleteSelected(objectsToDelete);

            //Delete potential empty folders
            int deleteCount = 0;
            deleteEmptyDirectories(getSystemFolderPath(folder.DirectoryName), ref deleteCount);

            refreshUnusedAssets();
        }

        private string getSystemFolderPath(string assetPath)
        {
            return Application.dataPath.Substring(0, Application.dataPath.IndexOf("/Assets") + 1) + assetPath;
        }

        private static bool deleteEmptyDirectories(string startLocation, ref int deleteCount)
        {
            foreach (var directory in System.IO.Directory.GetDirectories(startLocation, "*.*", System.IO.SearchOption.TopDirectoryOnly))
            {
                if (deleteEmptyDirectories(directory, ref deleteCount))
                    deleteCount++;
            }

            //Exclude file extensions that can be deleted
            if (System.IO.Directory.GetFiles(startLocation).Where(path => !path.EndsWith(".meta")
                        && (!path.ToLowerInvariant().EndsWith("thumbs.db"))
                        && (!path.ToLowerInvariant().EndsWith(".orig"))
                        && (!path.ToLowerInvariant().Contains(@".ds_store"))
                        && (!path.ToLowerInvariant().Contains(@".workspace.mel"))
                        && (!path.ToLowerInvariant().Contains(@".mayaswatches"))).Count() == 0
                        && System.IO.Directory.GetDirectories(startLocation).Length == 0)
            {
                FileUtil.DeleteFileOrDirectory(startLocation);
                AssetDatabase.Refresh();
                return true;
            }

            return false;
        }

        private void deleteSelected(List<AssetObjectInfo> objectsToDelete)
        {
            for (int i = 0; i < objectsToDelete.Count; i++)
            {
                UnityEditor.EditorUtility.DisplayProgressBar("Deleting unused assets", "Currently deleting " + i + "/" + objectsToDelete.Count, (float)i / (float)objectsToDelete.Count);
                objectsToDelete[i].Delete(m_unusedTypeDict);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
        }

        private void createBackup(List<AssetObjectInfo> objectsToDelete, string mainDirectoryName)
        {

            //Re-implement this if user should be allowed to select folder
            /*string[] backupFiles = new string[objectsToDelete.Count];
            for (int i = 0; i < objectsToDelete.Count; i++)
            {
                backupFiles[i] = objectsToDelete[i].m_Path;
            }
            string backupFileName = "Backup_" + DateTime.Now.ToString("yyyy_MMdd_HHmmss") + ".unitypackage";

            string tmpNewFolder = Application.dataPath + "\\..\\AssetHunterBackupXXX";
            System.IO.Directory.CreateDirectory(tmpNewFolder);

            string realNewFolder = EditorUtility.SaveFolderPanel("Save backup", tmpNewFolder, "");
            if (realNewFolder.Length != 0)
            {
                UnityEditor.AssetDatabase.ExportPackage(backupFiles, realNewFolder+ System.IO.Path.DirectorySeparatorChar + backupFileName, ExportPackageOptions.Default);
            }
            EditorUtility.RevealInFinder(tmpNewFolder);*/

            UnityEditor.EditorUtility.DisplayProgressBar("Creating backup unitypackage (This might be quite slow)", "Here's a static progress bar to look at (it wont move, be patient)", .3f);

            string[] backupFiles = new string[objectsToDelete.Count];
            for (int i = 0; i < objectsToDelete.Count; i++)
            {
                backupFiles[i] = objectsToDelete[i].m_Path;
            }

            string backupFileName = mainDirectoryName + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".unitypackage";
            string newFolderName = "AssetHunterBackup";
            string newDir = Application.dataPath + System.IO.Path.DirectorySeparatorChar + ".." + System.IO.Path.DirectorySeparatorChar + newFolderName;

            System.IO.Directory.CreateDirectory(newDir);

            UnityEditor.AssetDatabase.ExportPackage(backupFiles, newFolderName + System.IO.Path.DirectorySeparatorChar + backupFileName, ExportPackageOptions.Default);

            UnityEditor.EditorUtility.ClearProgressBar();

            EditorUtility.RevealInFinder(newDir + System.IO.Path.DirectorySeparatorChar);
        }

        private void getObjectsMarkedForDeletion(AssetHunterProjectFolderInfo aFInfo, ref List<AssetObjectInfo> objectsToDelete)
        {
            int childAssetCount = aFInfo.AssetList.Count;
            int childFolderCount = aFInfo.ChildFolderIndexers.Count;

            for (int i = childAssetCount - 1; i > -1; i--)
            {
                if ((m_unusedTypeDict.ContainsKey(aFInfo.AssetList[i].m_Type) && m_unusedTypeDict[aFInfo.AssetList[i].m_Type] == true))
                    objectsToDelete.Add(aFInfo.AssetList[i]);
            }

            for (int i = childFolderCount - 1; i > -1; i--)
            {
                getObjectsMarkedForDeletion(m_ProjectFolderList[aFInfo.ChildFolderIndexers[i]], ref objectsToDelete);
            }
        }

        private void showTypesUI(SortedDictionary<AssetHunterSerializableSystemType, bool> typeList)
        {
            EditorGUI.indentLevel = 0;
            m_showTypesFoldout = EditorGUILayout.Foldout(m_showTypesFoldout, "Show Types");

            m_TypeChangeDetected = false;
            if (m_showTypesFoldout)
            {
                EditorGUI.indentLevel = 1;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select all", GUILayout.ExpandWidth(false)))
                    foreach (var key in typeList.Keys.ToList())
                        typeList[key] = true;

                if (GUILayout.Button("Deselect all", GUILayout.ExpandWidth(false)))
                    foreach (var key in typeList.Keys.ToList())
                        typeList[key] = false;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();


                foreach (var key in typeList.Keys.ToList())
                {
                    bool lastVal = typeList[key];
                    typeList[key] = EditorGUILayout.ToggleLeft(key.Name, typeList[key]);
                    if (lastVal != typeList[key])
                        m_TypeChangeDetected = true;
                }
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
        }

        private void showBuildAssetInfoUI()
        {

            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel = 0;

            GUILayout.Label("--------------------------------LEGEND--------------------------------");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(m_UISceneSelect, GUILayout.Width(25), GUILayout.Height(25));
            GUILayout.Label("Select all scenes that reference this asset: ", GUILayout.Width(300));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(m_UIFolderSelect, GUILayout.Width(25), GUILayout.Height(25));
            GUILayout.Label("Locate asset in project view: ", GUILayout.Width(300));
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("-------------------------------------------------------------------------");

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Assets included in build", EditorStyles.boldLabel);

            EditorGUI.indentLevel = 1;
            for (int i = 0; i < m_BuildLog.AssetCount; i++)
            {
                if (m_usedTypeDict[m_BuildLog.GetAssetAtIndex(i).Type] == true)
                {
                    BuildReportAsset asset = m_BuildLog.GetAssetAtIndex(i);
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Space(10);
                    if (GUILayout.Button(m_UISceneSelect, GUIStyle.none, GUILayout.Width(btnImageSize), GUILayout.Height(btnImageSize)))
                    {
                        //Toggle if we should show scene dependency
                        asset.ToggleShowSceneDependency();

                        if (m_assetSceneDependencies.ContainsKey(asset.Path))
                        {
                            List<string> scenes = m_assetSceneDependencies[asset.Path];

                            UnityEngine.Object[] selectedObjects = new UnityEngine.Object[scenes.Count];

                            for (int j = 0; j < scenes.Count; j++)
                            {
                                selectedObjects[j] = AssetDatabase.LoadAssetAtPath(scenes[j], typeof(UnityEngine.Object));
                            }

                            Selection.activeObject = null;
                            Selection.objects = selectedObjects;
                            asset.SetSceneDependencies(selectedObjects);
                        }
                    }
                    GUILayout.Space(10);

                    if (GUILayout.Button(m_UIFolderSelect, GUIStyle.none, GUILayout.Width(btnImageSize), GUILayout.Height(btnImageSize)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath(asset.Path, asset.Type.SystemType);
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }

                    EditorGUILayout.LabelField(asset.Name, GUILayout.Width(350));

                    EditorGUILayout.LabelField(asset.SizeString, GUILayout.Width(75));
                    GUILayout.Label(EditorGUIUtility.ObjectContent(null, asset.Type.SystemType).image, GUILayout.Width(btnImageSize), GUILayout.Height(btnImageSize));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    if (asset.ShouldShowDependencies)
                    {
                        EditorGUI.indentLevel = 2;
                        //If the asset is actually reference in scenes
                        if (asset.GetSceneDependencies() != null && asset.GetSceneDependencies().Length >= 1)
                        {
                            asset.FoldOut = EditorGUILayout.Foldout(asset.FoldOut, "View scenes with dependency");
                            if (asset.FoldOut)
                            {
                                foreach (string sceneGUID in asset.GetSceneDependencies())
                                {
                                    if (sceneGUID != null)
                                    {
                                        string path = AssetDatabase.GUIDToAssetPath(sceneGUID);

                                        // Load the asset into reference
                                        UnityEngine.Object sceneObj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

                                        EditorGUILayout.ObjectField(sceneObj, typeof(UnityEngine.Object), false);
                                    }
                                }
                            }
                        }
                        //If no scenes reference the asset
                        else
                        {
                            Color initialColor = GUI.color;
                            GUI.color = AssetHunterHelper.AH_RED;
                            EditorGUILayout.LabelField("Asset not referenced in any scene, most likely a part of the \"resources\" foldes", EditorStyles.whiteLabel);
                            GUI.color = initialColor;
                        }
                    }
                    EditorGUI.indentLevel = 1;
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        private void showDependenciesUI()
        {
            m_showAssemblyFoldout = EditorGUILayout.Foldout(m_showAssemblyFoldout, "Show Assemblies");

            if (m_showAssemblyFoldout)
            {

                EditorGUILayout.BeginVertical();
                EditorGUI.indentLevel = 0;

                EditorGUILayout.LabelField("Assemblies included in build", EditorStyles.boldLabel);
                EditorGUI.indentLevel = 1;
                for (int i = 0; i < m_BuildLog.IncludedDependencies.Count; i++)
                {
                    EditorGUILayout.LabelField(m_BuildLog.IncludedDependencies[i]);
                }

                GUILayout.Space(20);
                //GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
        }

        private void drawAssetFolderInfoRecursively(AssetHunterProjectFolderInfo assetFolder, int indentLevel)
        {
            EditorGUI.indentLevel = indentLevel;

            if (!assetFolder.ShouldBeListed(m_unusedTypeDict))
                return;
            else
            {
                int assetCount = assetFolder.GetAssetCountInChildren();
                EditorGUILayout.BeginHorizontal();

                Color initialColor = GUI.color;
                GUI.color = AssetHunterHelper.AH_RED;
                float buttonSizeSelect = 60;
                float buttonSizeDelete = 100;

                if (GUILayout.Button("Delete all " + assetCount, GUILayout.Width(buttonSizeDelete)))
                {
                    m_folderMarkedForDeletion = assetFolder;
                }

                //Add space to align UI elements
                GUILayout.Space(buttonSizeSelect);

                //Create new style to have a bold foldout
                GUIStyle style = EditorStyles.foldout;
                FontStyle previousStyle = style.fontStyle;
                style.fontStyle = FontStyle.Bold;

                //Show foldout
                assetFolder.FoldOut = EditorGUILayout.Foldout(assetFolder.FoldOut, assetFolder.DirectoryName + " ("/* + assetFolder.FileSizeString + "/"*/ + assetFolder.FileSizeAccumulatedString + ")", style);

                //Reset style
                style.fontStyle = previousStyle;

                //Reset color
                GUI.color = initialColor;

                EditorGUILayout.EndHorizontal();
                if (assetFolder.FoldOut)
                {
                    foreach (AssetObjectInfo aInfo in assetFolder.AssetList)
                    {
                        if ((m_unusedTypeDict.ContainsKey(aInfo.m_Type) && m_unusedTypeDict[aInfo.m_Type] == false))
                            continue;

                        EditorGUI.indentLevel = (indentLevel + 1);
                        EditorGUILayout.BeginHorizontal();
                        GUI.color = Color.grey;
                        if (GUILayout.Button("Delete", GUILayout.Width(buttonSizeDelete)))
                        {
                            m_assetMarkedForDeletion = aInfo;
                        }
                        GUI.color = initialColor;
                        if (GUILayout.Button("Select", GUILayout.Width(buttonSizeSelect)))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath(aInfo.m_Path, aInfo.m_Type.SystemType);
                        }

                        EditorGUILayout.LabelField(aInfo.m_Name + " (" + aInfo.m_FileSizeString + ")", GUILayout.MaxWidth(600));
                        EditorGUILayout.EndHorizontal();
                    }

                    foreach (int childFolder in assetFolder.ChildFolderIndexers)
                    {
                        drawAssetFolderInfoRecursively(m_ProjectFolderList[childFolder], (indentLevel + 1));
                    }
                }
            }
        }

        private void loadEditorLog()
        {
            m_newBuildReady = false;

            m_ProjectFolderList.Clear();
            m_BuildLog = AssetHunterHelper.AnalyzeBuildLog();

            if (m_BuildLog.IsEmpty())
            {
                m_BuildLogLoaded = false;
                return;
            }
            else
            {
                m_BuildLogLoaded = true;
            }


            List<string> usedPrefabsInScenes = AssetHunterReader.GetPrefabsFromSceneFiles(AssetHunterHelper.GetEnabledSceneNamesInBuild(), out m_assetSceneDependencies);

            m_BuildLog.AddPrefabs(usedPrefabsInScenes);
            m_BuildLog.AddPlatformSpecificAssets();
            m_BuildLog.SortUsed();

            AssetHunterHelper.PopulateUnusedList(m_BuildLog, m_unusedTypeDict);

            refreshUnusedAssets();
        }

        private void refreshUnusedAssets()
        {
            updateUnusedTypeList();
            updateLogTypeList();

            m_ProjectFolderList[0].RecalcChildAssets(m_unusedTypeDict);
        }

        private void updateLogTypeList()
        {
            m_usedTypeDict.Clear();
            foreach (BuildReportAsset asset in m_BuildLog.m_BuildSizeList)
            {
                if (!m_usedTypeDict.ContainsKey(asset.Type))
                {
                    bool shouldBeListed = (asset.Type.SystemType == typeof(UnityEditor.MonoScript) ||
                        asset.Type.SystemType == typeof(UnityEngine.GameObject) ||
                        asset.Type.SystemType == typeof(UnityEngine.Object))
                        ? false : true;
                    m_usedTypeDict.Add(asset.Type, shouldBeListed);
                }
            }
            m_usedTypeDict.OrderBy(val => val.Key);
        }

        private void updateUnusedTypeList()
        {
            m_unusedTypeDict.Clear();

            getTypesRecursively(m_ProjectFolderList[0]);

            m_unusedTypeDict.OrderBy(val => val.Key);
        }

        private void getTypesRecursively(AssetHunterProjectFolderInfo afi)
        {
            foreach (AssetObjectInfo ai in afi.AssetList)
            {
                if (!m_unusedTypeDict.ContainsKey(ai.m_Type))
                {
                    m_unusedTypeDict.Add(ai.m_Type, true);
                }
            }
            foreach (int indexer in afi.ChildFolderIndexers)
            {
                getTypesRecursively(m_ProjectFolderList[indexer]);
            }
        }

        public static AssetHunterMainWindow Instance
        {
            get
            {
                if (m_window != null)
                    return m_window;
                else
                {
                    return initializeWindow();
                }
                ;
            }
        }

        internal void ReCalcUnusedAssetsFromIndex(int m_ParentListIndex)
        {
            m_ProjectFolderList[m_ParentListIndex].RecalcChildAssets(m_unusedTypeDict);
        }

        internal List<AssetHunterProjectFolderInfo> GetFolderList()
        {
            return m_ProjectFolderList;
        }

        internal void AddProjectFolderInfo(AssetHunterProjectFolderInfo afInfo)
        {
            m_ProjectFolderList.Add(afInfo);
        }

        internal int GetIndexOf(AssetHunterProjectFolderInfo projectFolderInfo)
        {
            return m_ProjectFolderList.IndexOf(projectFolderInfo);
        }

        internal AssetHunterProjectFolderInfo GetFolderInfo(string path)
        {
            return m_ProjectFolderList.Find(val => val.DirectoryName == path);
        }
    }

    [System.Serializable]
    public class StringListWrapper
    {
        public List<string> list;

        public StringListWrapper(List<string> list)
        {
            this.list = list;
        }
    }
}