﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static LightFramework.Editor.Resource.ProfileAccessor;
namespace LightFramework.Editor.Resource
{
    public class BuildApp
    {
        private static readonly string appName = "RLFrame";
        public static readonly string AppPathAndroid = Application.dataPath + "/../Build/Android/";
        public static readonly string AppPathIOS = Application.dataPath + "/../Build/IOS/";
        public static readonly string AppPathWindows = Application.dataPath + "/../Build/Windows/";

        [MenuItem("LightFramework/Build/Build")]
        public static void Build()
        {

            BundleEditor.Build();

            string targetPath = null;
            //todo editor
            string assetBundlePath = GetFrameProfile().BundleTargetPath;
            //todo clean
            CleanDirectory(GetFrameProfile().BundleBuildInPath);
            Copy(assetBundlePath, Application.streamingAssetsPath);
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    targetPath = AppPathAndroid + appName + "." + EditorUserBuildSettings.activeBuildTarget +
                                 $".{DateTime.Now:yyyy-M-d_hh.mm}.apk";
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    targetPath = AppPathWindows + appName + "." + EditorUserBuildSettings.activeBuildTarget +
                                 $".{DateTime.Now:yyyy-M-d_hh.mm}/{appName}.exe";
                    break;
                case BuildTarget.iOS:
                    targetPath = AppPathIOS + appName + "." + EditorUserBuildSettings.activeBuildTarget +
                                 $".{DateTime.Now:yyyy-M-d_hh.mm}";
                    break;
            }
            BuildPipeline.BuildPlayer(FindScenesInBuild(), targetPath, EditorUserBuildSettings.activeBuildTarget,
                BuildOptions.None);
        }

        private static string[] FindScenesInBuild()
        {
            List<string> scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                scenes.Add(scene.path);
            }

            return scenes.ToArray();
        }

        private static void Copy(string srcPath, string desPath)
        {
            try
            {
                if (Directory.Exists(srcPath))
                {
                    DirectoryInfo directoryInfo=new DirectoryInfo(srcPath);
                    foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFileSystemInfos())
                    {
                        string destName = Path.Combine(desPath, fileSystemInfo.Name);
                        if (fileSystemInfo is FileInfo)
                        {
                            File.Copy(fileSystemInfo.FullName,destName,true);
                        }
                        else
                        {
                            if (!Directory.Exists(destName))
                            {
                                Directory.CreateDirectory(destName);
                            }
                            Copy(fileSystemInfo.FullName,destName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Copy Error :{srcPath}->{desPath} : {e}");
                throw;
            }
        }

        private static void CleanDirectory(string path)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
                foreach (var fileSystemInfo in fileSystemInfos)
                {
                    if (fileSystemInfo is DirectoryInfo)
                    {
                        DirectoryInfo subDirectoryInfo = new DirectoryInfo(fileSystemInfo.FullName);
                        subDirectoryInfo.Delete(true);
                    }
                    else
                    {
                        File.Delete(fileSystemInfo.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"CleanDirectory Error :{path} : {e}");
                throw;
            }
        }
    }
}