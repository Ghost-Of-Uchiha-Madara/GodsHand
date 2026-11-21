using UnityEditor;

namespace TheGodAbove.UI.Editor
{
    public static class HUDLayoutBuilder
    {
        [MenuItem("The God Above/Generate HUD Layout")]
        public static void GenerateHUD()
        {
            GameUISceneBuilder.BuildGameUIScene();
        }
    }
}
