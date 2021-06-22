/**
 * Copyright 2017 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Showreel
{
    // This will load the Authentication Scene whenever the Showreel has started in Unity Editor.

    [InitializeOnLoad]
    public class MainSceneLoader : EditorWindow
    {
        private const string cEditorPrefPreviousScene = "MainSceneLoader.PreviousScene";

        private const string SceneFolder = "Assets/Showreel/";
        private const string SceneExtension = ".unity";
        private const string MasterScene = "AuthenticationScene";

        private static string _previousScene;

        private static string PreviousScene
        {
            get { return EditorPrefs.GetString(cEditorPrefPreviousScene, _previousScene); }
            set
            {
                _previousScene = value;
                EditorPrefs.SetString(cEditorPrefPreviousScene, value);
            }
        }

        static MainSceneLoader()
        {
            EditorApplication.playmodeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged()
        {
            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PreviousScene = SceneManager.GetActiveScene().name;

                // User pressed play -- autoload master scene.
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(SceneFolder + MasterScene + SceneExtension, OpenSceneMode.Single);
                }
                else
                {
                    EditorApplication.isPlaying = false;
                }
            }
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed stop -- reload previous scene.
                if (PreviousScene != MasterScene)
                {
                    EditorApplication.update += ReloadLastScene;
                }
            }
        }

        private static void ReloadLastScene()
        {
            if (SceneManager.GetActiveScene().name != PreviousScene)
            {
                EditorSceneManager.OpenScene(SceneFolder + PreviousScene + SceneExtension, OpenSceneMode.Single);
            }
            EditorApplication.update -= ReloadLastScene;
        }
    }
}