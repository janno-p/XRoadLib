using System.Reflection;
using XRoadLib.Attributes;
using XRoadLib.Extensions;

namespace XRoadLib.Schema;

public class DocumentationDefinition
{
    public List<XRoadTitleAttribute> Titles { get; } = new();
    public List<XRoadNotesAttribute> Notes { get; } = new();
    public List<XRoadTechNotesAttribute> TechNotes { get; } = new();

    public bool IsEmpty => !Titles.Any() && !Notes.Any() && !TechNotes.Any();

    public DocumentationDefinition()
    { }

    public DocumentationDefinition(ICustomAttributeProvider provider)
    {
        Titles.AddRange(provider.GetXRoadTitles());
        Notes.AddRange(provider.GetXRoadNotes());
        TechNotes.AddRange(provider.GetXRoadTechNotes());
    }
}