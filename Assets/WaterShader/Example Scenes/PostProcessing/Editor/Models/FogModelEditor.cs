using GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.Attributes;
using GTASP.WaterShader.Example_Scenes.PostProcessing.Runtime.Models;
using UnityEditor;

namespace GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.Models
{
    using Settings = FogModel.Settings;

    [PostProcessingModelEditor(typeof(FogModel))]
    public class FogModelEditor : PostProcessingModelEditor
    {
        SerializedProperty m_ExcludeSkybox;

        public override void OnEnable()
        {
            m_ExcludeSkybox = FindSetting((Settings x) => x.excludeSkybox);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This effect adds fog compatibility to the deferred rendering path; enabling it with the forward rendering path won't have any effect. Actual fog settings should be set in the Lighting panel.", MessageType.Info);
            EditorGUILayout.PropertyField(m_ExcludeSkybox);
            EditorGUI.indentLevel--;
        }
    }
}
