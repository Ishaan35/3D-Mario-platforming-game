using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace HeurekaGames.AssetHunter
{
    public class AssetHunterSettingsCreator : MonoBehaviour
    {
        public const string NAME = "AssetHunterSettingsData";

        internal static string GetAssetPath()
        {
            //SettingsData
            string[] scriptableAssetsFound = AssetDatabase.FindAssets("AssetHunterSettingsData t:ScriptableObject", null);
            if (scriptableAssetsFound.Length >= 1)
            {
                return AssetDatabase.GUIDToAssetPath(scriptableAssetsFound[0]);
            }
            //If the scriptableObject does not exist
            else
            {
                string[] allPaths = Directory.GetDirectories(Application.dataPath, "Settings", SearchOption.AllDirectories);
                foreach (string path in allPaths)
                {
                    if (path.Contains(string.Format("Heureka{0}AssetHunter{1}Editor{2}Settings", Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar)))
                    {
                        string relativepath = "";

                        if (path.StartsWith(Application.dataPath))
                        {
                            relativepath = "Assets" + path.Substring(Application.dataPath.Length) + Path.DirectorySeparatorChar;
                        }

                        AssetHunterSettings asset = ScriptableObject.CreateInstance<AssetHunterSettings>();
                        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(relativepath + NAME + ".asset");

                        AssetDatabase.CreateAsset(asset, assetPathAndName);

                        AssetDatabase.SaveAssets();

                        return assetPathAndName;
                    }
                }
            }
            return null;
        }
    }
}