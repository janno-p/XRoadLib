using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Serialization.Template
{
    public interface IXmlTemplateNode
    {
        IXmlTemplateNode this[string childNodeName, uint version] { get; }

        bool IsRequired { get; }

        string Name { get; }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        string Namespace { get; }

        IEnumerable<string> ChildNames { get; }

        int CountRequiredNodes(uint version);
    }
}