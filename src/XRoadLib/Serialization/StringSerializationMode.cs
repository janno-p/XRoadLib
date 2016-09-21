namespace XRoadLib.Serialization
{
    /// <summary>
    /// Specifies how string values with special characters should be handled.
    /// </summary>
    public enum StringSerializationMode
    {
        /// <summary>
        /// Encodes special characters using HTML encoding.
        /// </summary>
        HtmlEncoded,

        /// <summary>
        /// Wrap strings containing special characters inside CDATA.
        /// </summary>
        WrappedInCData
    }
}