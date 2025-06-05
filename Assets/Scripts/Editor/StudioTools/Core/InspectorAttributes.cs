using System;

namespace Lineage.Ancestral.Legacies.Editor.StudioTools.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorReadOnlyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorTooltipAttribute : Attribute
    {
        public string Tooltip { get; }
        public InspectorTooltipAttribute(string tooltip) => Tooltip = tooltip;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorHeaderAttribute : Attribute
    {
        public string Header { get; }
        public InspectorHeaderAttribute(string header) => Header = header;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorTextAreaAttribute : Attribute
    {
        public int Lines { get; }
        public InspectorTextAreaAttribute(int lines = 3) => Lines = lines;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorHideAttribute : Attribute { }
}
