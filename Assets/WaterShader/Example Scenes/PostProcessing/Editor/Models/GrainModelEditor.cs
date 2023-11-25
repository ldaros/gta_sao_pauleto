using GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.Attributes;
using GTASP.WaterShader.Example_Scenes.PostProcessing.Runtime.Models;
using UnityEditor;

namespace GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.Models
{
    using Settings = GrainModel.Settings;

    [PostProcessingModelEditor(typeof(GrainModel))]
    public class GrainModelEditor : PostProcessingModelEditor
    {
        SerializedProperty m_Colored;
        SerializedProperty m_Intensity;
        SerializedProperty m_Size;
        SerializedProperty m_LuminanceContribution;

        public override void OnEnable()
        {
            m_Colored = FindSetting((Settings x) => x.colored);
            m_Intensity = FindSetting((Settings x) => x.intensity);
            m_Size = FindSetting((Settings x) => x.size);
            m_LuminanceContribution = FindSetting((Settings x) => x.luminanceContribution);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_Intensity);
            EditorGUILayout.PropertyField(m_LuminanceContribution);
            EditorGUILayout.PropertyField(m_Size);
            EditorGUILayout.PropertyField(m_Colored);
        }
    }
}
