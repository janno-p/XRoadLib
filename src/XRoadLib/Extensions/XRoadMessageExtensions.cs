using System.Xml.XPath;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions;

/// <summary>
/// Extension methods for <see>XRoadMessage</see> class.
/// </summary>
public static class XRoadMessageExtensions
{
    /// <summary>
    /// Deserializes X-Road message response or throws <see>XRoadFaultException</see> when
    /// X-Road fault is parsed from the message instead of expected result value.
    /// </summary>
    public static async Task<object> DeserializeMessageContentAsync(this XRoadMessage message, IServiceMap serviceMap, IMessageFormatter messageFormatter)
    {
        if (serviceMap.ResponseDefinition.ContainsNonTechnicalFault)
            await ThrowIfXRoadFaultAsync(message, serviceMap, messageFormatter).ConfigureAwait(false);

        message.ContentStream.Position = 0;
        using var reader = XmlReader.Create(message.ContentStream, new XmlReaderSettings { Async = true });

        await messageFormatter.MoveToBodyAsync(reader).ConfigureAwait(false);

        if (!await reader.MoveToElementAsync(2).ConfigureAwait(false))
            throw new InvalidQueryException("No payload element in SOAP message.");

        await messageFormatter.ThrowSoapFaultIfPresentAsync(reader).ConfigureAwait(false);

        var result = await serviceMap.DeserializeResponseAsync(reader, message).ConfigureAwait(false);

        return result is XRoadFault fault ? throw new XRoadFaultException(fault) : result;
    }

    private static async Task ThrowIfXRoadFaultAsync(XRoadMessage message, IServiceMap serviceMap, IMessageFormatter messageFormatter)
    {
        message.ContentStream.Position = 0;

        var wrapperElement = serviceMap.ResponseDefinition.WrapperElementName;
        var responseElement = serviceMap.ResponseDefinition.Content.Name;
        var pathRoot = $"/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='{wrapperElement.LocalName}' and namespace-uri()='{wrapperElement.NamespaceName}']/*[local-name()='{responseElement.LocalName}' and namespace-uri()='{responseElement.NamespaceName}']";

        var doc = new XPathDocument(XmlReader.Create(message.ContentStream, new XmlReaderSettings { Async = true }));
        var navigator = doc.CreateNavigator();

        if (navigator.SelectSingleNode($"{pathRoot}/faultCode | {pathRoot}/faultString") != null)
            throw new XRoadFaultException(await serviceMap.DeserializeXRoadFaultAsync(message, messageFormatter).ConfigureAwait(false));
    }

    public static T? HandleEmptyElementOfValueType<T>(this XRoadMessage message, ContentDefinition content, Func<T?> getStrictValue) where T : struct
    {
        return (content.EmptyTagHandlingMode ?? message.ServiceManager.ProtocolDefinition.EmptyTagHandlingMode) switch
        {
            EmptyTagHandlingMode.DefaultValue => default,
            EmptyTagHandlingMode.Null => null,
            _ => getStrictValue()
        };
    }
}