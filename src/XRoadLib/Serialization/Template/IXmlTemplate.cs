using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Serialization.Template
{
    public interface IXmlTemplate
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        IDictionary<string, Type> ParameterTypes { get; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        IEnumerable<IXmlTemplateNode> ParameterNodes { get; }

        IXmlTemplateNode RequestNode { get; }

        IXmlTemplateNode ResponseNode { get; }
    }
}