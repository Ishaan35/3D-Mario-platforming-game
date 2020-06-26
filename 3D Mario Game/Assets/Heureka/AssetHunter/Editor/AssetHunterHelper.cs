using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HeurekaGames.AssetHunter
{
    internal class AssetHunterHelper
    {
        /*
        Blue
        91,168,220

        Grey,
        117,119,97

        Yellow1
        244,231,110

        Yellow2
        247,254,114

        Green
        143,247,167

        Red
        200,70,48
        */
        public static Color AH_BLUE = new Color(91f / 255f, 168f / 255f, 220f / 255f);
        public static Color AH_GREY = new Color(117f / 255f, 119f / 255f, 97f / 255f);
        public static Color AH_YELLOW1 = new Color(244f / 255f, 231f / 255f, 110f / 255f);
        public static Color AH_YELLOW2 = new Color(247f / 255f, 254f / 255f, 114f / 255f);
        public static Color AH_GREEN = new Color(143f / 255f, 247f / 255f, 167f / 255f);
        public static Color AH_RED = new Color(200f / 255f, 70f / 255f, 48f / 255f);

        private static int m_NumberOfDirectories;

        internal static bool HasBuildLogAvaliable()
        {
            string UnityEditorLogfile = GetLogFolderPath();
            string line = string.Empty;

            try
            {
                // Have to use FileStream to get around sharing violations!
                FileStream FS = new FileStream(UnityEditorLogfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader SR = new StreamReader(FS);

                while (!SR.EndOfStream && !(line = SR.ReadLine()).Contains("Mono dependencies included in the build")) ;
                while (!SR.EndOfStream && (line = SR.ReadLine()) != "")
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError(line + ": " + e);
                return false;
            }
        }

        public static string GetLogFolderPath()
        {
            string LocalAppData;
            string UnityEditorLogfile = string.Empty;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                LocalAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                UnityEditorLogfile = LocalAppData + "\\Unity\\Editor\\Editor.log";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                LocalAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                UnityEditorLogfile = LocalAppData + "/Library/Logs/Unity/Editor.log";
            }
            else
                Debug.LogError("RuntimePlatform not known");

            return UnityEditorLogfile;
        }

        internal static AssetHunterBuildReport AnalyzeBuildLog()
        {
            AssetHunterBuildReport buildReport = new AssetHunterBuildReport();
            string UnityEditorLogfile = GetLogFolderPath();

            try
            {
                // Have to use FileStream to get around sharing violations!
                FileStream FS = new FileStream(UnityEditorLogfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader SR = new StreamReader(FS);

                string line;
                int linesRead = 0;
                int lineIndex = 0;

                while (!SR.EndOfStream)
                {
                    line = SR.ReadLine();
                    linesRead++;
                    if ((line).Contains("Mono dependencies included in the build"))
                    {
                        lineIndex = linesRead;
                    }
                }

                FS.Position = 0;
                SR.DiscardBufferedData();

                //Start reading from log at the right line
                for (int i = 0; i < lineIndex - 1; i++)
                {

                    SR.ReadLine();
                }

                while (!SR.EndOfStream && !(line = SR.ReadLine()).Contains("Mono dependencies included in the build")) ;
                while (!SR.EndOfStream && (line = SR.ReadLine()) != "")
                {
                    int stringLength = line.Length;
                    int startIndex = line.LastIndexOf(" ");
                    buildReport.AddDependency(line.Substring(startIndex, stringLength - startIndex));
                }
                while (!SR.EndOfStream && !(line = SR.ReadLine()).Contains("Used Assets")) ;
                bool assetAnalysisComplete = false;
                while (!SR.EndOfStream && !assetAnalysisComplete)
                {
                    string curLine = SR.ReadLine();

                    if (curLine == "" || curLine.Contains("System memory in use before") || !curLine.Contains("% "))
                    {
                        assetAnalysisComplete = true;
                    }
                    else
                    {
                        if (!curLine.Contains("Built-in"))
                        {
                            string str = curLine.Substring(curLine.IndexOf("% ") + 2);
                            if (str.StartsWith("assets/", true, null))
                            {
                                EditorUtility.DisplayProgressBar(
                                    "Parsing build log",
                                    "Parsing build log to retrieve info",
                                    (float)SR.BaseStream.Position / (float)SR.BaseStream.Length);

                                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(str, typeof(UnityEngine.Object));

                                if (obj != null)
                                {
                                    BuildReportAsset asset = new BuildReportAsset();
                                    asset.SetAssetInfo(obj, str);

                                    buildReport.AddAsset(asset);
                                }
                                else
                                {
                                    Debug.Log(str + " does not seem to be a valid asset (Maybe its a \"terrain folder\"");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Debug.LogError("Error: " + E);
            }
            EditorUtility.ClearProgressBar();

            return buildReport;
        }

        internal static void PopulateUnusedList(AssetHunterBuildReport buildLog, SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList)
        {
            //Count all directories in project for use in progress bar
            m_NumberOfDirectories = System.IO.Directory.GetDirectories(Application.dataPath, "*.*", System.IO.SearchOption.AllDirectories).Length;
            int directoriesTraversed = 0;

            //traverse directories
            traverseDirectory(-1, Application.dataPath, buildLog.m_BuildSizeList, 0, ref directoriesTraversed, validTypeList);
            EditorUtility.ClearProgressBar();
        }

        private static void traverseDirectory(int parentIndex, string path, List<BuildReportAsset> usedAssets, int heirarchyDepth, ref int directoriesTraversed, SortedDictionary<AssetHunterSerializableSystemType, bool> validTypeList)
        {
            directoriesTraversed++;

            EditorUtility.DisplayProgressBar(
                                "Traversing Directories",
                                "(" + directoriesTraversed + " of " + m_NumberOfDirectories + ") Analyzing " + path.Substring(path.IndexOf("/Assets") + 1),
                                (float)directoriesTraversed / (float)m_NumberOfDirectories);

            //Get the settings to exclude certain folders or suffixes
            foreach (UnityEngine.Object dir in AssetHunterMainWindow.Instance.settings.m_DirectoryExcludes)
            {
                //TODO Can this be done more elegantly
                int startingIndex = Application.dataPath.Length - 6;
                string relativePath = path.Substring(startingIndex, path.Length - startingIndex);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object));

                if (dir == obj)
                {
                    //This folder was exluded
                    return;
                }
            }

            //Exclude types and folders that should not be reviewed
            //TODO perhaps improve performance of this step (Also use String.Contains(excluder, StringComparison.OrdinalIgnoreCase)) might be better not to use LINQ
            string[] assetsInDirectory = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(name => !name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)
                    && (!name.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    && (!name.EndsWith("thumbs.db", StringComparison.OrdinalIgnoreCase))
                    && (!name.EndsWith(".orig", StringComparison.OrdinalIgnoreCase))
                    && (!name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    && (!name.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase))
                    && (!name.Contains(Path.DirectorySeparatorChar + "heureka" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    && (!name.Contains(Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    && (!name.Contains(Path.DirectorySeparatorChar + "streamingassets" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    && (!name.Contains(Path.DirectorySeparatorChar + "resources" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    && (!name.Contains(Path.DirectorySeparatorChar + "editor default resources" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    && (!name.Contains(Path.DirectorySeparatorChar + "editor" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    && (!name.EndsWith(@".ds_store", StringComparison.OrdinalIgnoreCase))
                    && (!name.EndsWith(@".workspace.mel", StringComparison.OrdinalIgnoreCase))
                    && (!name.EndsWith(@".mayaswatches", StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

            //TODO this could also be improved for performance
            for (int i = 0; i < assetsInDirectory.Length; i++)
            {
                assetsInDirectory[i] = assetsInDirectory[i].Substring(assetsInDirectory[i].IndexOf("/Assets") + 1);
                assetsInDirectory[i] = assetsInDirectory[i].Replace(@"\", "/");
            }

            //Remove the assets we ignore through settings && Find any assets that does not live in UsedAssets List
            //TODO for performance reasons, perhaps dont to this for each folder, but just once, when finished?
            //That would mean to do folder creation and populating unused assets lists after all folders are traversed
            var result = assetsInDirectory.Where(p => (!AssetHunterMainWindow.Instance.settings.m_AssetGUIDExcludes.Any(p2 => UnityEditor.AssetDatabase.GUIDToAssetPath(p2) == p)) 
                                                        && !usedAssets.Any(p2 => UnityEditor.AssetDatabase.GUIDToAssetPath(p2.GUID) == p));

            //Create new folder object
            AssetHunterProjectFolderInfo afInfo = new AssetHunterProjectFolderInfo();

            //TODO this could also be improved for performance
            afInfo.DirectoryName = path.Substring(path.IndexOf("/Assets") + 1).Replace(@"\", "/");
            afInfo.ParentIndex = parentIndex;

            if (heirarchyDepth == 0)
                afInfo.FoldOut = true;

            //Add to static list
            AssetHunterMainWindow.Instance.AddProjectFolderInfo(afInfo);

            if (parentIndex != -1)
            {
                AssetHunterMainWindow.Instance.GetFolderList()[parentIndex].AddChildFolder(afInfo);
            }

            UnityEngine.Object objToFind;
            foreach (string assetName in result)
            {
                bool bExclude = false;

                foreach (string excluder in AssetHunterMainWindow.Instance.settings.m_AssetSubstringExcludes)
                {
                    //Exlude Asset Exclude substrings from settings
                    //If we find an excluded asset just continue to next iteration in loop
                    if (assetName.Contains(excluder, StringComparison.OrdinalIgnoreCase))
                        bExclude = true;
                }
                if (bExclude)
                    continue;

                objToFind = AssetDatabase.LoadAssetAtPath(assetName, typeof(UnityEngine.Object));

                if (objToFind == null)
                {
                    Debug.LogWarning("Couldnt find " + assetName);
                    continue;
                }

                AssetHunterSerializableSystemType assetType = new AssetHunterSerializableSystemType(objToFind.GetType());

                if (assetType.SystemType != typeof(MonoScript) && (!AssetHunterMainWindow.Instance.settings.m_AssetTypeExcludes.Contains(assetType)))
                {
                    AssetObjectInfo newAssetInfo = new AssetObjectInfo(assetName, assetType);
                    afInfo.AddAsset(newAssetInfo);
                }

                objToFind = null;

                //Memory leak safeguard
                //This have heavy performance implications
                if (AssetHunterMainWindow.Instance.settings.m_MemoryCleanupActive)
                    UnloadUnused();
            }

            string[] nextLevelDirectories = System.IO.Directory.GetDirectories(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);

            //Memory leak safeguard per folder
            if (!AssetHunterMainWindow.Instance.settings.m_MemoryCleanupActive)
                UnloadUnused();

            foreach (string nld in nextLevelDirectories)
            {
                traverseDirectory(AssetHunterMainWindow.Instance.GetFolderList().IndexOf(afInfo), nld, usedAssets, (heirarchyDepth + 1), ref directoriesTraversed, validTypeList);
            }
        }

        public static void UnloadUnused()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        public static string GetScriptAssemblyPath()
        {
            DirectoryInfo rootDirInfo = Directory.GetParent(Application.dataPath);

            string[] assemblyDirectories = Directory.GetDirectories(@rootDirInfo.FullName, "ScriptAssemblies", SearchOption.AllDirectories);

            string scriptAssembly = string.Empty;

            foreach (string dir in assemblyDirectories)
            {
                scriptAssembly = (Directory.GetFiles(dir, "*.dll").FirstOrDefault(val => val.EndsWith("Assembly-CSharp.dll")));

                if (!string.IsNullOrEmpty(scriptAssembly))
                    return scriptAssembly;
            }

            Debug.LogError("Could not find script assembly");
            return string.Empty;
        }

        public static string[] GetResourcesDirectories()
        {
            List<string> result = new List<string>();
            Stack<string> stack = new Stack<string>();
            // Add the root directory to the stack
            stack.Push(Application.dataPath);
            // While we have directories to process...
            while (stack.Count > 0)
            {
                // Grab a directory off the stack
                string currentDir = stack.Pop();
                try
                {
                    foreach (string dir in Directory.GetDirectories(currentDir))
                    {
                        if (Path.GetFileName(dir).Equals("Resources"))
                        {
                            // foreach (string subDir in Directory.GetDirectories(dir))
                            //result.Add(subDir);

                            // If one of the found directories is a Resources dir, add it to the result
                            result.Add(dir);
                        }
                        // Add directories at the current level into the stack
                        stack.Push(dir);
                    }
                }
                catch
                {
                    Debug.LogError("Directory " + currentDir + " couldn't be read from.");
                }
            }

            for (int i = 0; i < result.Count; i++)
                if (result[i].StartsWith(Application.dataPath))
                {
                    result[i] = "Assets" + result[i].Substring(Application.dataPath.Length).Replace("\\", "/"); ;
                }

            return result.ToArray();
        }

        public static string[] GetEnabledSceneNamesInBuild()
        {
            return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        }

        public static string[] GetAllSceneNamesInBuild()
        {
            return (from scene in EditorBuildSettings.scenes select scene.path).ToArray();
        }

        public static string[] GetAllSceneNames()
        {
            return (from scene in AssetDatabase.GetAllAssetPaths() where scene.EndsWith(".unity") select scene).ToArray();
        }

        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        #region ScriptDetection

        /*public static List<Type> GetScriptLevelDependencies(EditorBuildSettingsScene[] ebsScenes)
        {
            List<Type> dependencyScripts = new List<Type>();
            //List<Type> analyzedScripts = new List<Type>();
            List<string> assetsPathsToTrackForDependencies = new List<string>();

            String[] levelPaths = new string[ebsScenes.Length];

            //Loop scenes
            for (int i = 0; i < ebsScenes.Length; i++)
                levelPaths[i] = ebsScenes[i].path;

            //TODO Loop resources as well to find scripts in those
            string[] resourcesFolderPaths = AssetHunterHelper.GetResourcesDirectories();
            string[] resourceGUIDs = AssetDatabase.FindAssets("", resourcesFolderPaths);
            string[] resourcePaths = new string[resourceGUIDs.Length];

            for (int i = 0; i < resourceGUIDs.Length; i++)
                resourcePaths[i] = AssetDatabase.GUIDToAssetPath(resourceGUIDs[i]);

            assetsPathsToTrackForDependencies.AddRange(AssetDatabase.GetDependencies(resourcePaths).ToList<string>());
            assetsPathsToTrackForDependencies.AddRange(AssetDatabase.GetDependencies(levelPaths).ToList<string>());
            

            //Loop all relevant scenes, resources and assets and find their dependencies
            foreach (string dep in AssetDatabase.GetDependencies(assetsPathsToTrackForDependencies.ToArray<string>()))
            {
                //Is it a script?
                MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>(dep);
                if (ms != null)
                    dependencyScripts.Add(ms.GetClass());
            }

            List<Type> allDependencies = new List<Type>();
            allDependencies.AddRange(dependencyScripts);

            //Traverse each script and analyze its instructions to see if it references other scripts
            //foreach (Type t in dependencyScripts)
            //{
            //    traverseScript(t, ref allDependencies, ref analyzedScripts);
            //}

            return allDependencies;
        }*/


        /*private static void traverseScript(Type newType, ref List<Type> allAddedTypes, ref List<Type> analyzedScripts)
        {
            if (analyzedScripts.Contains(newType))
                return;
            else
            {
                //Find all the types referenced in script, i.e. through "AddComponent"
                List<Type> typesReferencedFromType = getTypeRefs(newType);
                analyzedScripts.Add(newType);

                //Temporaryly hold list
                List<Type> tmpList = new List<Type>();
                tmpList.AddRange(tmpList);

                bool containsNewType = typesReferencedFromType.Exists(val => (!tmpList.Contains(val)));

                //Get new types
                if (containsNewType)
                {
                    List<Type> newTypes = typesReferencedFromType.Where(val => (!tmpList.Contains(val))).ToList<Type>();
                    allAddedTypes.AddRange(newTypes);
                }

                //loop recursively
                foreach (Type t in typesReferencedFromType)
                {
                    //Traverse New Type
                    traverseScript(t, ref allAddedTypes, ref analyzedScripts);
                }
            }
        }*/

        /*private static List<Type> getTypeRefs(Type newType)
        {
            //Get script assembly
            string scriptAssemblyPath = GetScriptAssemblyPath();

            if (string.IsNullOrEmpty(scriptAssemblyPath))
            {
                Debug.LogError("ScriptAssembly could not be found");
                return null;
            }

            //Get added scripts

            Mono.Cecil.AssemblyDefinition scriptAssemblyDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(scriptAssemblyPath);

            Mono.Cecil.ModuleDefinition scriptModuleDef = scriptAssemblyDef.MainModule;

            List<Type> referencedTypes = new List<Type>();

            //Loop types on scriptAssembly
            foreach (Mono.Cecil.TypeDefinition td in scriptModuleDef.Types)
            {
                //Get type
                Type t = Type.GetType(td.FullName + ", " + td.Module.Assembly.FullName);

                //If type found in scriptAssembly matches the argument type
                if (t == newType)
                {
                    //Loop method definitions to get "Addcomponent" refs
                    foreach (Mono.Cecil.MethodDefinition md in td.Methods) //Use linq to only get the methoddefinitions we need ((m => ...);.Where(instr => instr.Operand is MethodReference && ((MethodReference)instr.Operand).ReturnType is GenericParameter)) 
                    {
                        //TODO. Make sure the linq finds properties/fields as well as method arguments + attributes
                        if (md.HasBody)
                            foreach (Mono.Cecil.Cil.Instruction inst in md.Body.Instructions.Where(val =>
                                val.Operand is Mono.Cecil.GenericInstanceMethod
                                && val.Operand.ToString().Contains("AddComponent")))
                            {
                                //Todo Do this more elegant by comparing method definition with methodreference
                                //if (!inst.Operand.ToString().Contains("AddComponent"))

                                Mono.Cecil.GenericInstanceMethod genInstanceRef = (Mono.Cecil.GenericInstanceMethod)inst.Operand;

                                if (genInstanceRef.HasGenericArguments)
                                {
                                    foreach (Mono.Cecil.TypeReference typeRef in genInstanceRef.GenericArguments)
                                    {
                                        Type argumentType = Type.GetType(typeRef.FullName + ", " + typeRef.Module.Assembly.FullName);
                                        if (argumentType != null)
                                            referencedTypes.Add(argumentType);
                                    }
                                }
                            }
                    }
                }
            }


            //TODO ferhaps run through the mono.cecil.scriptassembly to get the medthoddefinitions. And compare the type to the type parameter in this method?

            return referencedTypes;
        }*/

        //Analyze which scripts are added
        /*public static List<Type> GetAddedComponents()
        {
            //string unityEngineAssemblyPath = GetUnityEngineAssemblyPath();
            string scriptAssemblyPath = GetScriptAssemblyPath();

            if (string.IsNullOrEmpty(scriptAssemblyPath))
            {
                Debug.LogError("ScriptAssembly could not be found");
                return null;
            }

            Mono.Cecil.AssemblyDefinition scriptAssemblyDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(scriptAssemblyPath);

            Mono.Cecil.ModuleDefinition scriptModuleDef = scriptAssemblyDef.MainModule;

            //Now parse all Classes (types) in your assembly, parse all their Methods, parse all their Instructions, parse all their Operands and see if it's referencing yourMethod.
            List<Type> referencedTypes = new List<Type>();

            //TODO PERHAPS SEARCH TYPEDEFINITIONS TO SEE IF WE ARE ADDING SCRIPT PROCEDURALLY
            //ALSO FIELDDEFINITIONS INSIDE TYPEDEFINITIONS
            //ALSO REMEMBER TO LOOK IN RESOURCES
            foreach (Mono.Cecil.TypeDefinition td in scriptModuleDef.Types)
            {
                Type newType = Type.GetType(td.FullName + ", " + td.Module.Assembly.FullName);
                if (newType.IsClass)
                    referencedTypes.Add(newType);
            }

            return referencedTypes;
        }*/

        /*Mono.Cecil.TypeDefinition GetParameterTypeDefinition(Mono.Cecil.ParameterDefinition definition)
        {
            foreach (Mono.Cecil.TypeDefinition td in definition.ParameterType.Module.Types)
            {
                if (td.FullName == definition.ParameterType.FullName) return td;
            }
            return null;
        }*/
        #endregion
    }
}