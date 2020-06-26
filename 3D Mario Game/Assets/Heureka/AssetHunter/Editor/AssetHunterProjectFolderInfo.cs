using System;
using System.Collections.Generic;
using System.Linq;

namespace HeurekaGames.AssetHunter
{
    [System.Serializable]
    public class AssetHunterProjectFolderInfo
    {
        [UnityEngine.SerializeField]
        private List<AssetObjectInfo> m_assetList = new List<AssetObjectInfo>();

        [UnityEngine.SerializeField]
        private List<int> m_childFolderIndexers = new List<int>();

        [UnityEngine.SerializeField]
        string m_directoryName;

        [UnityEngine.SerializeField]
        private int m_ParentListIndex;

        [UnityEngine.SerializeField]
        int m_assetsInChildren = 0;

        [UnityEngine.SerializeField]
        long m_fileSize;

        [UnityEngine.SerializeField]
        long m_fileSizeAccumulated;

        [UnityEngine.SerializeField]
        string m_fileSizeString;

        [UnityEngine.SerializeField]
        string m_fileSizeAccumulatedString;

        public bool FoldOut = false;

        internal void AddAsset(AssetObjectInfo assetInfo)
        {
            if (AssetList == null)
                AssetList = new List<AssetObjectInfo>();

            assetInfo.SetParent(this);
            AssetList.Add(assetInfo);
        }

        public List<AssetObjectInfo> AssetList
        {
            get { return m_assetList; }
            set { m_assetList = value; }
        }
        public List<int> ChildFolderIndexers
        {
            get { return m_childFolderIndexers; }
            set { m_childFolderIndexers = value; }
        }
        public string DirectoryName
        {
            get { return m_directoryName; }
            set { m_directoryName = value; }
        }
        public int ParentIndex
        {
            get { return m_ParentListIndex; }
            set { m_ParentListIndex = value; }
        }
        public string FileSizeString
        {
            get { return m_fileSizeString; }
            set { m_fileSizeString = value; }
        }
        public string FileSizeAccumulatedString
        {
            get { return m_fileSizeAccumulatedString; }
            set { m_fileSizeAccumulatedString = value; }
        }

        internal bool ShouldBeListed(SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList)
        {
            return m_assetsInChildren >= 1 && hasValidAssetInChildren(this, validTypeList);
        }

        public void RecalcChildAssets(SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList)
        {
            CountChildren(validTypeList);

            if (m_ParentListIndex != -1)
                AssetHunterMainWindow.Instance.ReCalcUnusedAssetsFromIndex(m_ParentListIndex);
        }

        private bool hasValidAssetInChildren(AssetHunterProjectFolderInfo assetFolderInfo, SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList)
        {
            foreach (AssetObjectInfo aInfo in assetFolderInfo.AssetList)
            {
                if ((validTypeList.ContainsKey(aInfo.m_Type) && validTypeList[aInfo.m_Type] == true))
                    return true;
            }

            bool foundValidAsset = false;
            foreach (int indexer in assetFolderInfo.m_childFolderIndexers)
            {
                foundValidAsset = hasValidAssetInChildren(AssetHunterMainWindow.Instance.GetFolderList()[indexer], validTypeList);

                if (foundValidAsset)
                    return true;
            }

            return false;
        }

        private int calcAssetsInChildren(AssetHunterProjectFolderInfo assetFolderInfo, SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList, out long folderFileSize)
        {
            assetFolderInfo.m_fileSize = assetFolderInfo.GetUnusedAssetSize();

            long childrenSizeAccumulated = 0;

            int value = 0;
            foreach (int indexer in assetFolderInfo.m_childFolderIndexers)
            {
                long childSize = 0;
                value += AssetHunterMainWindow.Instance.GetFolderList()[indexer].m_assetsInChildren = calcAssetsInChildren(AssetHunterMainWindow.Instance.GetFolderList()[indexer], validTypeList, out childSize);

                childrenSizeAccumulated += childSize;
            }

            List<AssetObjectInfo> assetInfoList = (assetFolderInfo.AssetList.Where(val => (validTypeList.ContainsKey(val.m_Type) && validTypeList[val.m_Type]) == true)).ToList<AssetObjectInfo>();

            assetFolderInfo.m_fileSizeString = AssetHunterHelper.BytesToString(assetFolderInfo.m_fileSize);
            assetFolderInfo.m_fileSizeAccumulated = assetFolderInfo.m_fileSize + childrenSizeAccumulated;

            assetFolderInfo.m_fileSizeAccumulatedString = AssetHunterHelper.BytesToString(assetFolderInfo.m_fileSizeAccumulated);

            folderFileSize = assetFolderInfo.m_fileSizeAccumulated;

            return (value + assetInfoList.Count());
        }

        private long GetUnusedAssetSize()
        {
            long size = 0;
            foreach (AssetObjectInfo assetInfo in m_assetList)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(assetInfo.m_Path);
                size += fileInfo.Length;
            }
            return size;
        }

        internal void AddChildFolder(AssetHunterProjectFolderInfo afInfo)
        {
            if (m_childFolderIndexers == null)
                m_childFolderIndexers = new List<int>();

            AssetHunterMainWindow.Instance.AddProjectFolderInfo(afInfo);

            m_childFolderIndexers.Add(AssetHunterMainWindow.Instance.GetFolderList().IndexOf(afInfo));
        }

        internal void CountChildren(SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList)
        {
            long accumulatedSize = 0;
            m_assetsInChildren = calcAssetsInChildren(this, validTypeList, out accumulatedSize);

            //m_fileSize = accumulatedSize;
            //m_fileSizeString = AssetHunterHelper.BytesToString(m_fileSize);
        }

        internal int GetAssetCountInChildren()
        {
            return m_assetsInChildren;
        }

        internal void RemoveAsset(AssetObjectInfo assetObjectInfo)
        {
            m_assetList.Remove(assetObjectInfo);
        }

        internal string GetTopFolderName()
        {
            return System.IO.Path.GetFileName(DirectoryName.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        }
    }

    [System.Serializable]
    public class AssetObjectInfo
    {
        public string m_Path;

        [UnityEngine.SerializeField]
        public AssetHunterSerializableSystemType m_Type;

        public string m_Name;
        public string m_ParentPath;

        public long m_FileSize;
        public string m_FileSizeString;

        public AssetObjectInfo(string path, AssetHunterSerializableSystemType type)
        {
            this.m_Path = path;
            string[] parts = path.Split('/');
            this.m_Name = parts[parts.Length - 1];
            this.m_Type = type;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            this.m_FileSize = fileInfo.Length;
            this.m_FileSizeString = AssetHunterHelper.BytesToString(m_FileSize);
        }

        internal void Delete(SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList)
        {
            AssetHunterProjectFolderInfo parentFolder = AssetHunterMainWindow.Instance.GetFolderInfo(m_ParentPath);
            parentFolder.RemoveAsset(this);
            UnityEditor.AssetDatabase.DeleteAsset(m_Path);
            //UnityEditor.AssetDatabase.MoveAssetToTrash(m_Path);
        }

        internal void SetParent(AssetHunterProjectFolderInfo projectFolderInfo)
        {
            m_ParentPath = projectFolderInfo.DirectoryName;
        }
    }
}
