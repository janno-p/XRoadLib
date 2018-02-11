using System;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public interface IContentDefinition
    {
        ParticleDefinition Particle { get; }

        bool IgnoreExplicitType { get; }

        bool MergeContent { get; }

        bool UseXop { get; }

        Type RuntimeType { get; }

        XName TypeName { get; }

        ArrayItemDefinition ArrayItemDefinition { get; }
    }
}
