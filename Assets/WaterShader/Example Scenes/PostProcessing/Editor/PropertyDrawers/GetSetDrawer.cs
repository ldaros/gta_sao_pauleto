using GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.Utils;
using GTASP.WaterShader.Example_Scenes.PostProcessing.Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace GTASP.WaterShader.Example_Scenes.PostProcessing.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(GetSetAttribute))]
    sealed class GetSetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = (GetSetAttribute)base.attribute;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);

            if (EditorGUI.EndChangeCheck())
            {
                attribute.dirty = true;
            }
            else if (attribute.dirty)
            {
                var parent = ReflectionUtils.GetParentObject(property.propertyPath, property.serializedObject.targetObject);

                var type = parent.GetType();
                var info = type.GetProperty(attribute.name);

                if (info == null)
                    Debug.LogError("Invalid property name \"" + attribute.name + "\"");
                else
                    info.SetValue(parent, fieldInfo.GetValue(parent), null);

                attribute.dirty = false;
            }
        }
    }
}
