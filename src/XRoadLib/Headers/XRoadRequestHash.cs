namespace XRoadLib.Headers;

/// <summary>
/// For responses, this field contains a Base64 encoded hash of the request
/// SOAP message. This field is automatically filled in by the service
/// provider's security server.
/// </summary>
[XmlType(AnonymousType = true)]
public class XRoadRequestHash
{
    /// <summary>
    /// Identifies the hash algorithm that was used to calculate the value of
    /// the requestHash field. The  algorithms are specified as URIs listed
    /// in the XML-DSIG specification [DSIG].
    /// </summary>
    [XmlAttribute("algorithmId")]
    public string Algorithm { get; }

    /// <summary>
    /// Base64 encoded hash of the SOAP request message.
    /// </summary>
    [XmlText]
    public string Value { get; }

    /// <summary>
    /// Constructor for read-only request hash type.
    /// </summary>
    public XRoadRequestHash(string value, string algorithm)
    {
        Algorithm = algorithm;
        Value = value;
    }
}