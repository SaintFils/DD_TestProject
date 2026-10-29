using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DDTest.Editor._src.Editor.Scripts
{
	[InitializeOnLoad]
	public class StartupSceneOpener
	{
		private const string HandledKey = "DDTest.StartupSceneOpener.Handled";


		static StartupSceneOpener()
		{
			if (Application.isBatchMode || SessionState.GetBool(HandledKey, false))
				return;

			EditorApplication.delayCall += TryOpen;
		}


		private static void TryOpen()
		{
			if (EditorApplication.isCompiling || EditorApplication.isUpdating)
			{
				EditorApplication.delayCall += TryOpen;
				return;
			}

			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			if (SceneManager.sceneCount > 1 || !string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
			{
				SessionState.SetBool(HandledKey, true);
				return;
			}

			EditorBuildSettingsScene startScene = Array.Find(EditorBuildSettings.scenes, scene => scene.enabled);

			if (startScene == null || AssetDatabase.LoadAssetAtPath<SceneAsset>(startScene.path) == null)
				return;

			EditorSceneManager.OpenScene(startScene.path);
			SessionState.SetBool(HandledKey, true);
		}
	}
}