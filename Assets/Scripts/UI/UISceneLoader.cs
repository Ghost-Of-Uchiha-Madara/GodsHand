using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheGodAbove.UI
{
    /// <summary>
    /// Loads the dedicated UI scene additively alongside the 3D game world.
    /// Ensures clean separation between gameplay diorama and UI layer.
    /// </summary>
    public class UISceneLoader : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string uiSceneName = "GameUIScene";
        [SerializeField] private bool autoLoadInPlayMode = true;

        private void Awake()
        {
            if (autoLoadInPlayMode)
            {
                EnsureUISceneLoaded(uiSceneName);
            }
        }

        public static void EnsureUISceneLoaded(string sceneName = "GameUIScene")
        {
            if (!Application.isPlaying) return;

            bool isLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded)
                {
                    isLoaded = true;
                    break;
                }
            }

            if (!isLoaded)
            {
                Debug.Log($"[UISceneLoader] Additively loading dedicated UI scene: '{sceneName}'...");
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }
    }
}
