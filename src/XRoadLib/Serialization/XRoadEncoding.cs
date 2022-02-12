using System.Text;

namespace XRoadLib.Serialization;

public static class XRoadEncoding
{
    public static Encoding Utf8 { get; } = new UTF8Encoding();
}