using GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.Attributes;
using GTASP.WaterShader.Example_Scenes.PostProcessing.Runtime.Models;
using UnityEditor;

namespace GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.Models
{
    [PostProcessingModelEditor(typeof(DitheringModel))]
    public class DitheringModelEditor : PostProcessingModelEditor
    {
        public override void OnInspectorGUI()
        {
            if (profile.grain.enabled && target.enabled)
                EditorGUILayout.HelpBox("Grain is enabled, you probably don't need dithering !", MessageType.Warning);
            else
                EditorGUILayout.HelpBox("Nothing to configure !", MessageType.Info);
        }
    }
}
