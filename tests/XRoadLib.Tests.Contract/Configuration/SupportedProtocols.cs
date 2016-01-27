using XRoadLib.Protocols;

namespace XRoadLib.Tests.Contract.Configuration
{
    public static class SupportedProtocols
    {
        public static readonly IProtocol XRoadProtocol20 = new XRoad20Protocol("test-producer", "http://producers.test-producer.xtee.riik.ee/producer/test-producer")
        {
            Titles =
            {
                { "", "Ilma keeleta palun" },
                { "en", "XRoadLib test producer" },
                { "et", "XRoadLib test andmekogu" },
                { "pt", "Portugalikeelne loba ..." }
            }
        };

        public static readonly IProtocol XRoadProtocol31 = new XRoad31Protocol("test-producer", "http://test-producer.x-road.ee/producer/")
        {
            Titles =
            {
                { "", "Ilma keeleta palun" },
                { "en", "XRoadLib test producer" },
                { "et", "XRoadLib test andmekogu" },
                { "pt", "Portugalikeelne loba ..." }
            }
        };

        public static readonly IProtocol XRoadProtocol40 = new XRoad40Protocol("http://test-producer.x-road.eu/producer");
    }
}