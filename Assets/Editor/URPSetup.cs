using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AmishSimulator.Editor
{
    public static class URPSetup
    {
        [MenuItem("Amish Simulator/Fix URP Pipeline")]
        public static void FixURPPipeline()
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/Settings");

            // Create renderer data asset
            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, "Assets/Settings/UniversalRendererData.asset");

            // Create pipeline asset linked to renderer
            var pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/UniversalRenderPipelineAsset.asset");

            // Set as the project-wide default render pipeline
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "URP Fixed!",
                "Universal Render Pipeline pipeline asset created and assigned.\n\nNow re-run 'Amish Simulator \u2192 Create Homestead Scene' and hit Play.",
                "OK");
        }
    }
}
