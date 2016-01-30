using System;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public interface IContentDefinition
    {
        XName TypeName { get; }

        bool UseXop { get; }

        Type RuntimeType { get; }
    }
}
