using JetBrains.Annotations;

namespace XRoadLib.Tests.Contract;

[UsedImplicitly]
public class WsdlChangesTestDto : XRoadSerializable, IWsdlChangesTestDto
{
    [UsedImplicitly]
    public long? StaticProperty { get; set; }

    [UsedImplicitly]
    [XRoadRemoveContract(Version = 2, Converter = typeof(WsdlChangesTestDtoRenamedFromPropertyToRenamedToProperty))]
    public long? RenamedFromProperty { get; set; }

    [UsedImplicitly]
    [XRoadRemoveContract(Version = 2)]
    public long? RemovedProperty { get; set; }

    [UsedImplicitly]
    [XRoadAddContract(Version = 2)]
    public long? AddedProperty { get; set; }

    [XRoadRemoveContract(Version = 2, Converter = typeof(WsdlChangesTestDtoChangedTypePropertyStringToLong))]
    string? IWsdlChangesTestDto.ChangedTypeProperty { get; set; }

    [XRoadAddContract(Version = 2)]
    public long? ChangedTypeProperty { get; set; }

    [XRoadAddContract(Version = 2)]
    public long? RenamedToProperty { get; set; }

    [UsedImplicitly]
    [XRoadRemoveContract(Version = 2, Converter = typeof(WsdlChangesTestDtoSinglePropertyToMultipleProperty))]
    public long? SingleProperty { get; set; }

    [XRoadAddContract(Version = 2)]
    public long[]? MultipleProperty { get; set; }
}

[UsedImplicitly]
public interface IWsdlChangesTestDto
{
    [UsedImplicitly]
    string? ChangedTypeProperty { get; set; }
}

[UsedImplicitly]
public static class WsdlChangesTestDtoRenamedFromPropertyToRenamedToProperty
{
    [UsedImplicitly]
    public static void Convert(WsdlChangesTestDto entity, long? value)
    {
        entity.RenamedToProperty = value;
    }

    [UsedImplicitly]
    public static long? ConvertBack(WsdlChangesTestDto entity)
    {
        return entity.RenamedToProperty;
    }
}

[UsedImplicitly]
public static class WsdlChangesTestDtoChangedTypePropertyStringToLong
{
    [UsedImplicitly]
    public static void Convert(WsdlChangesTestDto entity, string value)
    {
        entity.ChangedTypeProperty = System.Convert.ToInt64(value);
    }

    [UsedImplicitly]
    public static string? ConvertBack(WsdlChangesTestDto entity)
    {
        return entity.ChangedTypeProperty?.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}

[UsedImplicitly]
public static class WsdlChangesTestDtoSinglePropertyToMultipleProperty
{
    [UsedImplicitly]
    public static void Convert(WsdlChangesTestDto entity, long? value)
    {
        entity.MultipleProperty = value.HasValue ? new[] { value.Value } : null;
    }

    [UsedImplicitly]
    public static long? ConvertBack(WsdlChangesTestDto entity)
    {
        return entity.MultipleProperty is { Length: > 0 } ? entity.MultipleProperty[0] : null;
    }
}