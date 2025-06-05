using System;
using UnityEngine;

namespace Lineage.Core.Editor.StudioTools
{
    /// <summary>
    /// Marks a field as read-only in the generated inspector UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorReadOnlyAttribute : Attribute { }

    /// <summary>
    /// Provides a tooltip for the generated inspector field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorTooltipAttribute : Attribute
    {
        public readonly string Tooltip;
        public InspectorTooltipAttribute(string tooltip) { Tooltip = tooltip; }
    }

    /// <summary>
    /// Draws a header label before the field in the generated inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorHeaderAttribute : Attribute
    {
        public readonly string Header;
        public InspectorHeaderAttribute(string header) { Header = header; }
    }

    /// <summary>
    /// Marks a string field to be drawn with a multi-line text area.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorTextAreaAttribute : Attribute
    {
        public readonly int Lines;
        public InspectorTextAreaAttribute(int lines = 3) { Lines = Mathf.Max(1, lines); }
    }

    /// <summary>
    /// Hides the field from the generated inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorHideAttribute : Attribute { }
}
