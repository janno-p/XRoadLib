using System.Linq.Expressions;
using XRoadLib.Headers;

namespace XRoadLib.Schema;

/// <summary>
/// Allows to fluently configure headers mandatory elements.
/// </summary>
public interface IHeaderDefinitionBuilder<THeader> where THeader : IXRoadHeader
{
    /// <summary>
    /// Specify mandatory element of the header object.
    /// </summary>
    IHeaderDefinitionBuilder<THeader> WithRequiredHeader<TValue>(Expression<Func<THeader, TValue>> expression);

    /// <summary>
    /// Define namespaces used to define SOAP header elements.
    /// </summary>
    IHeaderDefinitionBuilder<THeader> WithHeaderNamespace(string namespaceName);
}