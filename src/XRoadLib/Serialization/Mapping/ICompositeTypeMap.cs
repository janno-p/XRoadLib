using XRoadLib.Schema;

namespace XRoadLib.Serialization.Mapping;

public interface ICompositeTypeMap : ITypeMap
{
    void InitializeProperties(IEnumerable<Tuple<PropertyDefinition, ITypeMap>> propertyDefinitions, IEnumerable<string> availableFilters);
}