using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Editor.StudioTools.Core
{
    /// <summary>
    /// Utility class for drawing object fields using reflection.
    /// Supports basic field types and simple lists.
    /// </summary>
    public static class GenericEditorUIDrawer
    {
        public static void DrawObjectFields(object target)
        {
            if (target == null) return;
            var type = target.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                DrawField(target, field);
            }
        }

        private static void DrawField(object target, FieldInfo field)
        {
            var value = field.GetValue(target);
            object newValue = value;
            Type fieldType = field.FieldType;

            if (fieldType == typeof(int))
            {
                newValue = EditorGUILayout.IntField(ObjectNames.NicifyVariableName(field.Name), (int)value);
            }
            else if (fieldType == typeof(float))
            {
                newValue = EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(field.Name), (float)value);
            }
            else if (fieldType == typeof(string))
            {
                newValue = EditorGUILayout.TextField(ObjectNames.NicifyVariableName(field.Name), (string)value);
            }
            else if (fieldType == typeof(bool))
            {
                newValue = EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(field.Name), (bool)value);
            }
            else if (fieldType.IsEnum)
            {
                newValue = EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(field.Name), (Enum)value);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                newValue = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(field.Name), (UnityEngine.Object)value, fieldType, false);
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                IList list = value as IList;
                if (list == null)
                {
                    list = (IList)Activator.CreateInstance(fieldType);
                }

                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(field.Name));
                EditorGUI.indentLevel++;
                for (int i = 0; i < list.Count; i++)
                {
                    object element = list[i];
                    list[i] = DrawElement(element, fieldType.GetGenericArguments()[0]);
                }
                if (GUILayout.Button("Add"))
                {
                    list.Add(Activator.CreateInstance(fieldType.GetGenericArguments()[0]));
                }
                EditorGUI.indentLevel--;
                newValue = list;
            }

            if (!Equals(newValue, value))
            {
                field.SetValue(target, newValue);
            }
        }

        private static object DrawElement(object element, Type elementType)
        {
            object newValue = element;
            if (elementType == typeof(int))
            {
                newValue = EditorGUILayout.IntField((int)(element ?? 0));
            }
            else if (elementType == typeof(float))
            {
                newValue = EditorGUILayout.FloatField((float)(element ?? 0f));
            }
            else if (elementType == typeof(string))
            {
                newValue = EditorGUILayout.TextField((string)(element ?? ""));
            }
            else if (elementType.IsEnum)
            {
                newValue = EditorGUILayout.EnumPopup((Enum)(element ?? Activator.CreateInstance(elementType)));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(elementType))
            {
                newValue = EditorGUILayout.ObjectField((UnityEngine.Object)element, elementType, false);
            }
            return newValue;
        }
    }
}
