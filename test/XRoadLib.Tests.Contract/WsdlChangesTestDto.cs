﻿using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Tests.Contract;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class WsdlChangesTestDto : XRoadSerializable, IWsdlChangesTestDto
{
    public long? StaticProperty { get; set; }

    [XRoadRemoveContract(Version = 2, Converter = typeof(WsdlChangesTestDtoRenamedFromPropertyToRenamedToProperty))]
    public long? RenamedFromProperty { get; set; }

    [XRoadRemoveContract(Version = 2)]
    public long? RemovedProperty { get; set; }

    [XRoadAddContract(Version = 2)]
    public long? AddedProperty { get; set; }

    [XRoadRemoveContract(Version = 2, Converter = typeof(WsdlChangesTestDtoChangedTypePropertyStringToLong))]
    string? IWsdlChangesTestDto.ChangedTypeProperty { get; set; }

    [XRoadAddContract(Version = 2)]
    public long? ChangedTypeProperty { get; set; }

    [XRoadAddContract(Version = 2)]
    public long? RenamedToProperty { get; set; }

    [XRoadRemoveContract(Version = 2, Converter = typeof(WsdlChangesTestDtoSinglePropertyToMultipleProperty))]
    public long? SingleProperty { get; set; }

    [XRoadAddContract(Version = 2)]
    public long[]? MultipleProperty { get; set; }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IWsdlChangesTestDto
{
    string? ChangedTypeProperty { get; set; }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class WsdlChangesTestDtoRenamedFromPropertyToRenamedToProperty
{
    public static void Convert(WsdlChangesTestDto entity, long? value)
    {
        entity.RenamedToProperty = value;
    }

    public static long? ConvertBack(WsdlChangesTestDto entity)
    {
        return entity.RenamedToProperty;
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class WsdlChangesTestDtoChangedTypePropertyStringToLong
{
    public static void Convert(WsdlChangesTestDto entity, string value)
    {
        entity.ChangedTypeProperty = System.Convert.ToInt64(value);
    }

    public static string? ConvertBack(WsdlChangesTestDto entity)
    {
        return entity.ChangedTypeProperty?.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class WsdlChangesTestDtoSinglePropertyToMultipleProperty
{
    public static void Convert(WsdlChangesTestDto entity, long? value)
    {
        entity.MultipleProperty = value.HasValue ? new[] { value.Value } : null;
    }

    public static long? ConvertBack(WsdlChangesTestDto entity)
    {
        return entity.MultipleProperty is { Length: > 0 } ? entity.MultipleProperty[0] : null;
    }
}