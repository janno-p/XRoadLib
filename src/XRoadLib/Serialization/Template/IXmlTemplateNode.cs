using System.Collections.Generic;

namespace XRoadLib.Serialization.Template
{
    public interface IXmlTemplateNode
    {
        IXmlTemplateNode this[string childNodeName, uint version] { get; }

        bool IsRequired { get; }

        string Name { get; }

        string Namespace { get; }

        IEnumerable<string> ChildNames { get; }

        int CountRequiredNodes(uint version);
    }
}