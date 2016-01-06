using System;

namespace XRoadLib.Attributes
{
    public enum MessagePartDirection { Input, Output }

    [AttributeUsage(AttributeTargets.Method)]
    public class XRoadMessagePartAttribute : Attribute
    {
        public MessagePartDirection Direction { get; }
        public string PartName { get; }
        public string TypeName { get; }

        public XRoadMessagePartAttribute(MessagePartDirection direction, string partName, string typeName)
        {
            Direction = direction;
            PartName = partName;
            TypeName = typeName;
        }
    }
}