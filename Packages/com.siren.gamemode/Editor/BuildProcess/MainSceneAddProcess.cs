﻿using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameMode.Editor.BuildProcess
{
    public class MainSceneAddProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = AppSettings.Instance;

            if (!Execute(settings))
            {
                //TODO: build exit
                throw new BuildFailedException("required master scene");
            }
        }

        // [AfterAssembliesLoaded] -> [BeforeSplashScreen] -> [BeforeSceneLoad]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void Initialize()
        {
            if (Execute(AppSettings.Instance)) return;

            EditorApplication.isPlaying = false;
            Debug.LogError("required master scene");
        }

        public static bool Execute(AppSettings settings)
        {
            if (settings.SkipInitialize)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(settings.MainScene))
            {
                // var path = AssetDatabase.GetAssetPath(settings.MainScene);
                var path = settings.MainScene;
                var scenes = EditorBuildSettings.scenes.ToList();

                if (scenes.Any() && scenes.Select(_ => _.path).Contains(path))
                {
                    if (scenes[0].path != path)
                    {
                        var masterSceneIndex = scenes.FindIndex(_ => _.path == path);
                        scenes.RemoveAt(masterSceneIndex);
                    }
                    else
                    {
                        return true;
                    }
                }

                scenes.Insert(0, new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                return true;
            }

            return false;
        }

        [MenuItem("GameMode/Test/MasterSceneGuid")]
        private static void MasterSceneGuid()
        {
            var app = AppSettings.Instance;

            if (app.MainScene != null)
            {
                // var path = AssetDatabase.GetAssetPath(app.MainScene);
                Debug.Log(app.MainScene);
            }
        }

        [MenuItem("GameMode/Test/AddMasterScene")]
        private static void AddMasterScene()
        {
            var settings = AppSettings.Instance;
            Execute(settings);
        }
    }
}