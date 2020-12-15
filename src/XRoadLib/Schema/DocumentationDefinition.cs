using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XRoadLib.Attributes;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    public class DocumentationDefinition
    {
        public List<XRoadTitleAttribute> Titles { get; } = new();
        public List<XRoadNotesAttribute> Notes { get; } = new();
        public List<XRoadTechNotesAttribute> TechNotes { get; } = new();

        public bool IsEmpty => !Titles.Any() && !Notes.Any() && !TechNotes.Any();

        public DocumentationDefinition()
        { }

        public DocumentationDefinition(ICustomAttributeProvider provider, DocumentationTarget target = DocumentationTarget.Default)
        {
            Titles.AddRange(provider.GetXRoadTitles(target));
            Notes.AddRange(provider.GetXRoadNotes(target));
            TechNotes.AddRange(provider.GetXRoadTechNotes(target));
        }
    }
}