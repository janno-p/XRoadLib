using System;
using System.Collections.Generic;

namespace XRoadLib.Serialization.Template
{
    public interface IXmlTemplate
    {
        IDictionary<string, Type> ParameterTypes { get; }

        IEnumerable<IXmlTemplateNode> ParameterNodes { get; }

        IXmlTemplateNode ResponseNode { get; }

        IXmlTemplateNode GetParameterNode(string parameterName);
    }
}