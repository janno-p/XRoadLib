namespace XRoadLib.Schema;

public class PropertyComparer : IComparer<PropertyDefinition>
{
    public static PropertyComparer Instance { get; } = new();

    private PropertyComparer()
    { }

    public int Compare(PropertyDefinition x, PropertyDefinition y)
    {
        return CompareContent(x?.Content, y?.Content);
    }

    private static int CompareContent(ContentDefinition x, ContentDefinition y)
    {
        var orderValue = x.Order.CompareTo(y.Order);
        if (orderValue != 0)
            return orderValue;

        var xName = x.SerializedName;
        var yName = y.SerializedName;

        var namespaceValue = string.Compare(xName?.NamespaceName ?? "", yName?.NamespaceName ?? "", StringComparison.OrdinalIgnoreCase);

        return namespaceValue == 0
            ? string.Compare(xName?.LocalName ?? "", yName?.LocalName ?? "", StringComparison.OrdinalIgnoreCase)
            : namespaceValue;
    }
}