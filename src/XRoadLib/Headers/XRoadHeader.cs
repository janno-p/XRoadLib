using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;

namespace XRoadLib.Headers;

/// <summary>
/// Implements default X-Road message protocol version 4.0 SOAP header.
/// </summary>
[UsedImplicitly]
public class XRoadHeader : IXRoadHeader
{
    /// <summary>
    /// Client identity.
    /// </summary>
    public virtual XRoadClientIdentifier Client { get; set; }

    /// <summary>
    /// Service identity.
    /// </summary>
    [UsedImplicitly]
    public virtual XRoadServiceIdentifier Service { get; set; }

    /// <summary>
    /// User identity code.
    /// </summary>
    [UsedImplicitly]
    public virtual string UserId { get; set; }

    /// <summary>
    /// Received application, issue or document.
    /// </summary>
    [UsedImplicitly]
    public virtual string Issue { get; set; }

    /// <summary>
    /// Unique identity of the request.
    /// </summary>
    public virtual string Id { get; set; }

    /// <summary>
    /// X-Road message protocol version.
    /// </summary>
    public virtual string ProtocolVersion { get; set; }

    /// <summary>
    /// Central service identity.
    /// </summary>
    [UsedImplicitly]
    public virtual XRoadCentralServiceIdentifier CentralService { get; set; }

    /// <summary>
    /// Represented party details.
    /// </summary>
    [UsedImplicitly]
    public virtual XRoadRepresentedParty RepresentedParty { get; set; }

    /// <summary>
    /// Request hash of the X-Road message.
    /// </summary>
    [UsedImplicitly]
    public virtual XRoadRequestHash RequestHash { get; set; }

    /// <summary>
    /// Check for presence of mandatory parts.
    /// </summary>
    public virtual void Validate()
    {
        if (Client == null)
            throw new InvalidQueryException("X-Road header `client` element is mandatory.");

        if (Id == null)
            throw new InvalidQueryException("X-Road header `id` element is mandatory.");

        if (ProtocolVersion == null)
            throw new InvalidQueryException("X-Road header `protocolVersion` element is mandatory.");
    }

    /// <summary>
    /// Read next header value from the XML reader object.
    /// </summary>
    public virtual async Task ReadHeaderValueAsync(XmlReader reader)
    {
        switch (reader.NamespaceURI)
        {
            case NamespaceConstants.XRoadRepr when reader.LocalName == "representedParty":
                RepresentedParty = await ReadRepresentedPartyAsync(reader).ConfigureAwait(false);
                return;

            case NamespaceConstants.XRoad:
                switch (reader.LocalName)
                {
                    case "client":
                        Client = await ReadClientAsync(reader).ConfigureAwait(false);
                        return;

                    case "service":
                        Service = await ReadServiceAsync(reader).ConfigureAwait(false);
                        return;

                    case "centralService":
                        CentralService = await ReadCentralServiceAsync(reader).ConfigureAwait(false);
                        return;

                    case "id":
                        Id = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        return;

                    case "userId":
                        UserId = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        return;

                    case "issue":
                        Issue = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        return;

                    case "protocolVersion":
                        ProtocolVersion = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        return;

                    case "requestHash":
                        RequestHash = await ReadRequestHashAsync(reader).ConfigureAwait(false);
                        return;
                }

                break;
        }

        throw new InvalidQueryException($"Unexpected X-Road header element `{reader.GetXName()}`.");
    }

    private static async Task<XRoadRepresentedParty> ReadRepresentedPartyAsync(XmlReader reader)
    {
        var qualifiedName = reader.GetXName();

        if (reader.IsEmptyElement)
            throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

        var representedParty = new XRoadRepresentedParty();

        var depth = reader.Depth;
        var success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);

        if (success && reader.LocalName == "partyClass" && reader.NamespaceURI == NamespaceConstants.XRoadRepr)
        {
            representedParty.Class = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
            success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);
        }

        if (!success)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("partyCode", NamespaceConstants.XRoadRepr)}`.");

        if (reader.LocalName != "partyCode" || reader.NamespaceURI != NamespaceConstants.XRoadRepr)
            throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

        representedParty.Code = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false))
            return representedParty;

        throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");
    }

    private static async Task<XRoadClientIdentifier> ReadClientAsync(XmlReader reader)
    {
        var qualifiedName = reader.GetXName();

        if (reader.IsEmptyElement)
            throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

        var client = new XRoadClientIdentifier();

        var depth = reader.Depth;

        var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadId);
        if (string.IsNullOrWhiteSpace(objectType))
            throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadId)}` value.");
        client.ObjectType = GetObjectType(objectType);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadId)}`.");
        client.XRoadInstance = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XRoadId)}`.");
        client.MemberClass = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XRoadId)}`.");
        client.MemberCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        var success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);

        if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XRoadId)
        {
            client.SubsystemCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
            success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);
        }

        if (success)
            throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

        return client;
    }

    private static async Task<XRoadServiceIdentifier> ReadServiceAsync(XmlReader reader)
    {
        var qualifiedName = reader.GetXName();

        if (reader.IsEmptyElement)
            throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

        var service = new XRoadServiceIdentifier();

        var depth = reader.Depth;

        var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadId);
        if (string.IsNullOrWhiteSpace(objectType))
            throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadId)}` value.");
        service.ObjectType = GetObjectType(objectType);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadId)}`.");
        service.XRoadInstance = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XRoadId)}`.");
        service.MemberClass = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XRoadId)}`.");
        service.MemberCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        var success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);
        if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XRoadId)
        {
            service.SubsystemCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
            success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);
        }

        if (!success || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XRoadId)}`.");
        service.ServiceCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);
        if (success && reader.LocalName == "serviceVersion" && reader.NamespaceURI == NamespaceConstants.XRoadId)
        {
            service.ServiceVersion = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
            success = await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false);
        }

        if (success)
            throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

        return service;
    }

    private static async Task<XRoadCentralServiceIdentifier> ReadCentralServiceAsync(XmlReader reader)
    {
        var qualifiedName = reader.GetXName();

        if (reader.IsEmptyElement)
            throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

        var centralService = new XRoadCentralServiceIdentifier();

        var depth = reader.Depth;

        var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadId);
        if (string.IsNullOrWhiteSpace(objectType))
            throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadId)}` value.");
        centralService.ObjectType = GetObjectType(objectType);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadId)}`.");
        centralService.XRoadInstance = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (!await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false) || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
            throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XRoadId)}`.");
        centralService.ServiceCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        if (await reader.MoveToElementAsync(depth + 1).ConfigureAwait(false))
            throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

        return centralService;
    }

    private static async Task<XRoadRequestHash> ReadRequestHashAsync(XmlReader reader)
    {
        var algorithm = reader.GetAttribute("requestHash");
        var value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        return new XRoadRequestHash(value, algorithm);
    }

    private static XRoadObjectType GetObjectType(string value) => value.Trim() switch
    {
        "MEMBER" => XRoadObjectType.Member,
        "SUBSYSTEM" => XRoadObjectType.Subsystem,
        "SERVICE" => XRoadObjectType.Service,
        "CENTRALSERVICE" => XRoadObjectType.CentralService,
        _ => throw new InvalidQueryException($"Invalid `{XName.Get("objectType", NamespaceConstants.XRoadId)}` attribute value `{value}`.")
    };

    /// <summary>
    /// Serializes X-Road message SOAP headers to XML.
    /// </summary>
    public virtual async Task WriteToAsync(XmlWriter writer, Style style, HeaderDefinition headerDefinition)
    {
        if (writer.LookupPrefix(NamespaceConstants.XRoad) == null)
            await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.XRoad, NamespaceConstants.Xmlns, NamespaceConstants.XRoad).ConfigureAwait(false);

        if (writer.LookupPrefix(NamespaceConstants.XRoadId) == null)
            await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.Id, NamespaceConstants.Xmlns, NamespaceConstants.XRoadId).ConfigureAwait(false);

        if (headerDefinition.RequiredHeaders.Contains(XName.Get("client", NamespaceConstants.XRoad)) || Client != null)
        {
            await writer.WriteStartElementAsync(null, "client", NamespaceConstants.XRoad).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(null, "objectType", NamespaceConstants.XRoadId, string.IsNullOrWhiteSpace(Client.SubsystemCode) ? "MEMBER" : "SUBSYSTEM").ConfigureAwait(false);

            await writer.WriteElementStringAsync(null, "xRoadInstance", NamespaceConstants.XRoadId, Client.XRoadInstance).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "memberClass", NamespaceConstants.XRoadId, Client.MemberClass).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "memberCode", NamespaceConstants.XRoadId, Client.MemberCode).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(Client.SubsystemCode))
                await writer.WriteElementStringAsync(null, "subsystemCode", NamespaceConstants.XRoadId, Client.SubsystemCode).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        if (headerDefinition.RequiredHeaders.Contains(XName.Get("service", NamespaceConstants.XRoad)) || Service != null)
        {
            await writer.WriteStartElementAsync(null, "service", NamespaceConstants.XRoad).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(null, "objectType", NamespaceConstants.XRoadId, "SERVICE").ConfigureAwait(false);

            await writer.WriteElementStringAsync(null, "xRoadInstance", NamespaceConstants.XRoadId, Service.XRoadInstance).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "memberClass", NamespaceConstants.XRoadId, Service.MemberClass).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "memberCode", NamespaceConstants.XRoadId, Service.MemberCode).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(Service.SubsystemCode))
                await writer.WriteElementStringAsync(null, "subsystemCode", NamespaceConstants.XRoadId, Service.SubsystemCode).ConfigureAwait(false);

            await writer.WriteElementStringAsync(null, "serviceCode", NamespaceConstants.XRoadId, Service.ServiceCode).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(Service.ServiceVersion))
                await writer.WriteElementStringAsync(null, "serviceVersion", NamespaceConstants.XRoadId, Service.ServiceVersion).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        if (headerDefinition.RequiredHeaders.Contains(XName.Get("centralService", NamespaceConstants.XRoad)) || CentralService != null)
        {
            await writer.WriteStartElementAsync(null, "centralService", NamespaceConstants.XRoad).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(null, "objectType", NamespaceConstants.XRoadId, "CENTRALSERVICE").ConfigureAwait(false);

            await writer.WriteElementStringAsync(null, "xRoadInstance", NamespaceConstants.XRoadId, CentralService.XRoadInstance).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "serviceCode", NamespaceConstants.XRoadId, CentralService.ServiceCode).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        async Task WriteHeaderValueAsync(string elementName, object value, XName typeName)
        {
            var name = XName.Get(elementName, NamespaceConstants.XRoad);
            if (headerDefinition.RequiredHeaders.Contains(name) || value != null)
                await style.WriteHeaderElementAsync(writer, name, value, typeName).ConfigureAwait(false);
        }

        await WriteHeaderValueAsync("id", Id, XmlTypeConstants.String).ConfigureAwait(false);
        await WriteHeaderValueAsync("userId", UserId, XmlTypeConstants.String).ConfigureAwait(false);
        await WriteHeaderValueAsync("issue", Issue, XmlTypeConstants.String).ConfigureAwait(false);
        await WriteHeaderValueAsync("protocolVersion", ProtocolVersion, XmlTypeConstants.String).ConfigureAwait(false);
    }
}