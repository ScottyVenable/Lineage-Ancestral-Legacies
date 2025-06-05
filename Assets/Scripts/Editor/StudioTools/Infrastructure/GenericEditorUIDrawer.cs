using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lineage.Core.Editor.StudioTools
{
    /// <summary>
    /// Utility class for drawing inspector UI for arbitrary objects via reflection.
    /// Handles common primitive types, enums, UnityEngine.Object references and
    /// simple lists of those types.
    /// </summary>
    public static class GenericEditorUIDrawer
    {
        public static void DrawObjectFields(object target)
        {
            if (target == null) return;
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(InspectorHideAttribute)))
                    continue;

                DrawField(target, field);
            }
        }

        private static void DrawField(object obj, FieldInfo field)
        {
            var label = new GUIContent(ObjectNames.NicifyVariableName(field.Name));
            var tooltip = field.GetCustomAttribute<InspectorTooltipAttribute>();
            if (tooltip != null) label.tooltip = tooltip.Tooltip;

            var header = field.GetCustomAttribute<InspectorHeaderAttribute>();
            if (header != null)
                EditorGUILayout.LabelField(header.Header, EditorStyles.boldLabel);

            var value = field.GetValue(obj);
            bool readOnly = Attribute.IsDefined(field, typeof(InspectorReadOnlyAttribute));
            EditorGUI.BeginDisabledGroup(readOnly);
            object newValue = DrawValueField(label, field.FieldType, value, field.GetCustomAttribute<InspectorTextAreaAttribute>());
            EditorGUI.EndDisabledGroup();

            if (!Equals(value, newValue))
                field.SetValue(obj, newValue);
        }

        private static object DrawValueField(GUIContent label, Type type, object value, InspectorTextAreaAttribute textArea)
        {
            if (type == typeof(int))
                return EditorGUILayout.IntField(label, value != null ? (int)value : 0);
            if (type == typeof(float))
                return EditorGUILayout.FloatField(label, value != null ? (float)value : 0f);
            if (type == typeof(bool))
                return EditorGUILayout.Toggle(label, value != null && (bool)value);
            if (type == typeof(string))
            {
                if (textArea != null)
                    return EditorGUILayout.TextArea(value as string ?? string.Empty, GUILayout.Height(EditorGUIUtility.singleLineHeight * textArea.Lines));
                return EditorGUILayout.TextField(label, value as string ?? string.Empty);
            }
            if (type.IsEnum)
                return EditorGUILayout.EnumPopup(label, (Enum)value);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return EditorGUILayout.ObjectField(label, value as UnityEngine.Object, type, false);
            if (typeof(IList).IsAssignableFrom(type))
                return DrawListField(label, type, value as IList);

            EditorGUILayout.LabelField(label); // unsupported type
            return value;
        }

        private static object DrawListField(GUIContent label, Type listType, IList list)
        {
            if (list == null)
                list = (IList)Activator.CreateInstance(listType);

            var elementType = listType.IsArray ? listType.GetElementType() : listType.GetGenericArguments()[0];
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            int removeIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                object element = list[i];
                object newElement = DrawValueField(new GUIContent($"Element {i}"), elementType, element, null);
                if (!Equals(element, newElement))
                    list[i] = newElement;
                if (GUILayout.Button("-", GUILayout.Width(20)))
                    removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }
            if (removeIndex >= 0)
                list.RemoveAt(removeIndex);

            if (GUILayout.Button($"Add {elementType.Name}"))
            {
                object newElement = elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
                list.Add(newElement);
            }

            return list;
        }
    }
}
