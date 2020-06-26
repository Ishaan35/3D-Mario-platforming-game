using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace HeurekaGames.AssetHunter
{
    [System.Serializable]
    public class AssetHunterSettings : ScriptableObject
    {
        [SerializeField]
        public List<UnityEngine.Object> m_DirectoryExcludes = new List<UnityEngine.Object>();

        [SerializeField]
        public List<AssetHunterSerializableSystemType> m_AssetTypeExcludes = new List<AssetHunterSerializableSystemType>();

        [SerializeField]
        public List<string> m_AssetSubstringExcludes = new List<string>();

        [SerializeField]
        public List<string> m_AssetGUIDExcludes = new List<string>();

        [SerializeField]
        public bool m_MemoryCleanupActive = false;

        internal static string GetAssetPath()
        {
            return AssetHunterSettingsCreator.GetAssetPath();
        }

        internal bool ValidateDirectory(UnityEngine.Object newDir)
        {
            return !m_DirectoryExcludes.Contains(newDir) && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(newDir));
        }

        internal bool ValidateType(AssetHunterSerializableSystemType newtype)
        {
            return !m_AssetTypeExcludes.Contains(newtype);
        }

        internal bool ValidateSubstring(string newSubstring)
        {
            return !m_AssetSubstringExcludes.Contains(newSubstring);
        }

        internal bool ValidateAssetGUID(UnityEngine.Object activeObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(activeObject);

            //Return false if folder
            if (AssetDatabase.IsValidFolder(assetPath))
                return false;

            string assetID = AssetDatabase.AssetPathToGUID(assetPath);

            return !m_AssetGUIDExcludes.Contains(assetID);
        }

        public static void ExcludeIndividualAssetWithObject(UnityEngine.Object asset)
        {
            string AssetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            ExcludeIndividualAssetWithID(AssetID);
        }

        public static void ExcludeIndividualAssetWithpath(string path)
        {
            AssetHunterSettings settings = LoadSettings();
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!settings.m_AssetGUIDExcludes.Contains(guid))
            {
                if(!string.IsNullOrEmpty(guid))
                { 
                    settings.m_AssetGUIDExcludes.Add(guid);
                    setDirty(settings);
                }
            }
        }

        public static void ExcludeIndividualAssetWithID(string guid)
        {
            AssetHunterSettings settings = LoadSettings();
            if (!settings.m_AssetGUIDExcludes.Contains(guid))
            {
                settings.m_AssetGUIDExcludes.Add(guid);
                setDirty(settings);
            }
        }

        public static void ExcludeDirectory(UnityEngine.Object newDir)
        {
            AssetHunterSettings settings = LoadSettings();
            if (!settings.m_DirectoryExcludes.Contains(newDir))
            {
                settings.m_DirectoryExcludes.Add(newDir);
                setDirty(settings);
            }
        }

        public static void ExcludeType(AssetHunterSerializableSystemType newtype)
        {
            AssetHunterSettings settings = LoadSettings();
            if (!settings.m_AssetTypeExcludes.Contains(newtype))
            {
                settings.m_AssetTypeExcludes.Add(newtype);
                setDirty(settings);
            }
        }

        public static void ExcludeSubstring(string newSubstring)
        {
            AssetHunterSettings settings = LoadSettings();
            if (!settings.m_AssetSubstringExcludes.Contains(newSubstring))
            {
                settings.m_AssetSubstringExcludes.Add(newSubstring);
                setDirty(settings);
            }
        }

        public static AssetHunterSettings LoadSettings()
        {
            string path = AssetHunterSettingsCreator.GetAssetPath();
            return AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)) as AssetHunterSettings;
        }

        private static void setDirty(AssetHunterSettings settings)
        {
            EditorUtility.SetDirty(settings);
        }

        internal void RemoveAssetIDAtIndex(int indexer)
        {
            m_AssetGUIDExcludes.RemoveAt(indexer);
            setDirty(this);
        }

        internal void RemoveDirectoryAtIndex(int indexer)
        {
            m_DirectoryExcludes.RemoveAt(indexer);
            setDirty(this);
        }

        internal void RemoveTypeAtIndex(int indexer)
        {
            m_AssetTypeExcludes.RemoveAt(indexer);
            setDirty(this);
        }

        internal void RemoveSubstringAtIndex(int indexer)
        {
            m_AssetSubstringExcludes.RemoveAt(indexer);
            setDirty(this);
        }

        internal bool HasExcludes()
        {
            return m_DirectoryExcludes.Count >= 1 || m_AssetTypeExcludes.Count >= 1;
        }

        internal bool HasDirectoryExcludes()
        {
            return m_DirectoryExcludes.Count >= 1;
        }

        internal bool HasTypeExcludes()
        {
            return m_AssetTypeExcludes.Count >= 1;
        }

        internal bool HasSubStringExcludes()
        {
            return m_AssetSubstringExcludes.Count >= 1;
        }

        internal void CleanExcludedAssets()
        {
            for (int i = m_AssetGUIDExcludes.Count - 1; i > 0; i--)
            {
                string path = AssetDatabase.GUIDToAssetPath(m_AssetGUIDExcludes[i]);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

                if (obj == null)
                    m_AssetGUIDExcludes.RemoveAt(i);
            }
        }
    }
}
