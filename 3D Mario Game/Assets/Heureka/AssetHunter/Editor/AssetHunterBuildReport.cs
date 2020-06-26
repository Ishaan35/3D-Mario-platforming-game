using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace HeurekaGames.AssetHunter
{
    [System.Serializable]
    public class AssetHunterBuildReport
    {
        [SerializeField]
        private List<string> m_includedDependencies = new List<string>();
        [SerializeField]
        public List<BuildReportAsset> m_BuildSizeList = new List<BuildReportAsset>();

        internal void AddDependency(string assemblyName)
        {
            m_includedDependencies.Add(assemblyName);
        }

        internal bool IsEmpty()
        {
            return m_BuildSizeList.Count == 0 && m_includedDependencies.Count == 0;
        }

        public List<string> IncludedDependencies
        {
            get { return m_includedDependencies; }
            set { m_includedDependencies = value; }
        }

        internal void AddAsset(BuildReportAsset asset)
        {
            m_BuildSizeList.Add(asset);
        }

        public int AssetCount
        {
            get { return m_BuildSizeList.Count; }
        }

        internal BuildReportAsset GetAssetAtIndex(int i)
        {
            return m_BuildSizeList[i];
        }

        internal void AddPrefabs(List<string> usedPrefabsInScenes)
        {
            foreach (string path in usedPrefabsInScenes)
            {
                EditorUtility.DisplayProgressBar(
                                "Adding prefabs",
                                "Analyzing scenes to get prefabs",
                                (float)usedPrefabsInScenes.IndexOf(path) / (float)usedPrefabsInScenes.Count);
                //Early out
                if (m_BuildSizeList.Exists(val => val.Path == path))
                    continue;

                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

                if (obj != null)
                {
                    BuildReportAsset newAsset = new BuildReportAsset();

                    newAsset.SetAssetInfo(obj, path);

                    //newAsset.SetSize(0.0f, "--");
                    m_BuildSizeList.Add(newAsset);
                }
                else
                {
                    Debug.LogWarning(path + " is not a valid asset");
                }
            }

            EditorUtility.ClearProgressBar();
        }

        internal void AddPlatformSpecificAssets()
        {
            int counter = 0;
            int countTo = Enum.GetValues(typeof(BuildTargetGroup)).Length;

            //TODO Get all the different splash screens and config files somehow
            List<UnityEngine.Object> splash = new List<UnityEngine.Object>();

#if UNITY_5_5_OR_NEWER
            //Dont add splashscreen in this version
#else
    splash.Add(UnityEditor.PlayerSettings.xboxSplashScreen);
#endif
            //Loop the entries
            foreach (UnityEngine.Object obj in splash)
            {
                //Early out if it already exist
                if (obj == null || m_BuildSizeList.Exists(val => val.Path == AssetDatabase.GetAssetPath(obj)))
                    continue;

                BuildReportAsset newAsset = new BuildReportAsset();

                newAsset.SetAssetInfo(obj, AssetDatabase.GetAssetPath(obj));
                //newAsset.SetSize(0.0f, "--");
                m_BuildSizeList.Add(newAsset);
            }

            //Loop icons in buildtargetgroups
            foreach (BuildTargetGroup btg in (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup)))
            {
                EditorUtility.DisplayProgressBar(
                                "Add Target Specifics for " + btg.ToString(),
                                "Looking at icons and splash screens for targetgroups",
                                (float)counter / (float)countTo);

                Texture2D[] buildTargetGroupTextures = UnityEditor.PlayerSettings.GetIconsForTargetGroup(btg);

                foreach (Texture2D curIcon in buildTargetGroupTextures)
                {
                    //Early out if it already exist
                    if (curIcon == null || m_BuildSizeList.Exists(val => val.Path == AssetDatabase.GetAssetPath(curIcon)))
                        continue;

                    BuildReportAsset newAsset = new BuildReportAsset();

                    newAsset.SetAssetInfo(curIcon, AssetDatabase.GetAssetPath(curIcon));
                    //newAsset.SetSize(0.0f, "--");
                    m_BuildSizeList.Add(newAsset);
                }
                AssetHunterHelper.UnloadUnused();
            }

            EditorUtility.ClearProgressBar();
        }

        internal void SortUsed()
        {
            m_BuildSizeList.Sort((a, b) => b.FileSize.CompareTo(a.FileSize));
        }
    }


    [System.Serializable]
    public class BuildReportAsset : IEquatable<BuildReportAsset>
    {
        [SerializeField]
        private string m_name;
        [SerializeField]
        private string m_path;
        [SerializeField]
        private string m_assetGUID;
        [SerializeField]
        private AssetHunterSerializableSystemType m_type;
        [SerializeField]
        private long m_assetActualSize;
        [SerializeField]
        private string m_assetSizeString;
        [SerializeField]
        private bool m_bShowSceneDependency;
        [SerializeField]
        private string[] m_sceneDependenciesGUID;
        [SerializeField]
        private bool m_bFoldOut;

        static readonly string[] SizeSuffixes =
          { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        internal void SetAssetInfo(UnityEngine.Object obj, string path)
        {
            long fileSize = -1;

            fileSize = getImportedFileSize(path);

            m_assetActualSize = fileSize;
            //SetSize();
            m_assetSizeString = (m_assetActualSize != -1) ? SizeSuffix(m_assetActualSize, 1) : "NA";

            this.m_path = path;
            //string[] parts = path.Split('/');
            this.m_name = obj.name;// parts[parts.Length - 1];

            m_assetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            m_type = new AssetHunterSerializableSystemType(obj.GetType());

            AssetHunterHelper.UnloadUnused();
        }

        private long getImportedFileSize(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (guid.Length < 2)
                {
                    //Debug.Log(assetPath + " has no guid? value is \"" + guid + "\"");
                    return -1;
                }

                string assetImportedPath = System.IO.Path.GetFullPath(Application.dataPath + "../../Library/cache/" + guid.Substring(0, 2) + "/" + guid);

                if (File.Exists(assetImportedPath))
                {
                    return getActualFileSize(assetImportedPath);
                }
                else
                {
                    assetImportedPath = System.IO.Path.GetFullPath(Application.dataPath + "../../Library/metadata/" + guid.Substring(0, 2) + "/" + guid);
                    string addedResourcePath = System.IO.Path.GetFullPath(Application.dataPath + "../../Library/metadata/" + guid.Substring(0, 2) + "/" + guid + ".resource");

                    //The additional resource file (i.e. audioclips have a file + a resource file)
                    long resourceSize = File.Exists(assetImportedPath) ? getActualFileSize(addedResourcePath) : 0;

                    if (File.Exists(assetImportedPath))
                    {
                        return getActualFileSize(assetImportedPath) + resourceSize;
                    }
                }
            }

            //Fallback return -1;
            return -1;
        }

        private long getActualFileSize(string assetImportedPath)
        {

            if (string.IsNullOrEmpty(assetImportedPath) || !File.Exists(assetImportedPath))
            {
                return 0;
            }

            FileInfo fi = new FileInfo(assetImportedPath);
            return fi.Length;
        }

        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }

        public bool Equals(BuildReportAsset other)
        {
            if (other != null)
                return this.m_assetGUID == other.m_assetGUID;/* &&
                   this.m_name == other.m_name &&
                   this.m_path == other.m_path;*/

            Debug.LogWarning("Something when wrong in parsing, compare object is null");
            return false;
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public AssetHunterSerializableSystemType Type
        {
            get { return m_type; }
        }

        public string GUID
        {
            get { return m_assetGUID; }
        }

        public string Path
        {
            get { return m_path; }
        }

        public long FileSize
        {
            get { return m_assetActualSize; }
        }

        public string SizeString
        {
            get { return m_assetSizeString; }
        }


        internal void ToggleShowSceneDependency()
        {
            m_bShowSceneDependency = !m_bShowSceneDependency;
            m_bFoldOut = m_bShowSceneDependency;

            if (m_bShowSceneDependency == false)
                m_sceneDependenciesGUID = null;
        }

        internal void SetSceneDependencies(UnityEngine.Object[] scenes)
        {
            string[] sceneGuids = new string[scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                sceneGuids[i] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scenes[i]));
            }

            m_sceneDependenciesGUID = sceneGuids;
        }

        public bool ShouldShowDependencies
        {
            get { return m_bShowSceneDependency; }
        }

        public bool FoldOut
        {
            get { return m_bFoldOut; }
            set { m_bFoldOut = value; }
        }

        internal string[] GetSceneDependencies()
        {
            return m_sceneDependenciesGUID;
        }
    }
}
