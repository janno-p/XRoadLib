using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;

namespace XRoadLib.Protocols.Styles
{
    public abstract class Style
    {
        public virtual void WriteExplicitType(XmlWriter writer, XName qualifiedName)
        { }

        public virtual void WriteExplicitArrayType(XmlWriter writer, XName itemQualifiedName, int arraySize)
        { }

        public virtual void WriteType(XmlWriter writer, TypeDefinition typeDefinition, Type expectedType)
        {
            if (typeDefinition.IsAnonymous)
                return;

            if (typeDefinition.RuntimeInfo != expectedType)
            {
                writer.WriteTypeAttribute(typeDefinition.Name);
                return;
            }

            WriteExplicitType(writer, typeDefinition.Name);
        }
    }
}