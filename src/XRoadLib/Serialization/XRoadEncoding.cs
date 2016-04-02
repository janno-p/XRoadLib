using System.Text;

namespace XRoadLib.Serialization
{
    public static class XRoadEncoding
    {
        private static Encoding utf8WithoutBOM = new UTF8Encoding();

        public static Encoding UTF8 => utf8WithoutBOM;
    }
}
