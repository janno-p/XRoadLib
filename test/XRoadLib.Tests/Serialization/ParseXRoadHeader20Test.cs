using XRoadLib.Headers;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class ParseXRoadHeader20Test
    {
        [Fact]
        public void CanParseAsutusValue()
        {
            var xhr2 = CreateHeader("asutus", "12345");
            var xhr = (IXRoadHeader)xhr2;
            Assert.NotNull(xhr.Client);
            Assert.Equal("12345", xhr.Client.MemberCode);
            Assert.Equal("12345", xhr2.Asutus);
        }

        [Fact]
        public void CanParseAndmekoguValue()
        {
            var xhr2 = CreateHeader("andmekogu", "andmekogu");
            var xhr = (IXRoadHeader)xhr2;
            Assert.NotNull(xhr.Service);
            Assert.Equal("andmekogu", xhr.Service.SubsystemCode);
            Assert.Equal("andmekogu", xhr2.Andmekogu);
        }

        [Fact]
        public void CanParseIsikukoodValue()
        {
            var xhr2 = CreateHeader("isikukood", "Kasutaja");
            var xhr = (IXRoadHeader)xhr2;
            Assert.Equal("Kasutaja", xhr.UserId);
            Assert.Equal("Kasutaja", xhr2.Isikukood);
        }

        [Fact]
        public void CanParseAmetnikValue()
        {
            var xhr2 = CreateHeader("ametnik", "Kasutaja");
            Assert.Equal("Kasutaja", xhr2.Ametnik);
        }

        [Fact]
        public void CanParseIdValue()
        {
            var xhr2 = CreateHeader("id", "hash");
            var xhr = (IXRoadHeader)xhr2;
            Assert.Equal("hash", xhr2.Id);
            Assert.Equal("hash", xhr.Id);
        }

        [Fact]
        public void CanParseNimiValue()
        {
            var xhr2 = CreateHeader("nimi", "producer.testService.v5");
            var xhr = (IXRoadHeader)xhr2;
            Assert.NotNull(xhr.Service);
            Assert.Null(xhr.Service.SubsystemCode);
            Assert.Equal("testService", xhr.Service.ServiceCode);
            Assert.Equal("v5", xhr.Service.ServiceVersion);
            Assert.Equal(5u, xhr.Service.Version);
            Assert.Equal("testService.v5", xhr2.Nimi);
        }

        [Fact]
        public void CanParseToimikValue()
        {
            var xhr2 = CreateHeader("toimik", "toimik");
            var xhr = (IXRoadHeader)xhr2;
            Assert.Equal("toimik", xhr.Issue);
            Assert.Equal("toimik", xhr2.Toimik);
        }

        [Fact]
        public void CanParseAllasutusValue()
        {
            var xhr2 = CreateHeader("allasutus", "yksus");
            Assert.Equal("yksus", xhr2.Allasutus);
        }

        [Fact]
        public void CanParseAmetValue()
        {
            var xhr2 = CreateHeader("amet", "ametikoht");
            Assert.Equal("ametikoht", xhr2.Amet);
        }

        [Fact]
        public void CanParseAmetniknimiValue()
        {
            var xhr2 = CreateHeader("ametniknimi", "Kuldar");
            Assert.Equal("Kuldar", xhr2.AmetnikNimi);
        }

        [Fact]
        public void CanParseAsynkroonne1Value()
        {
            var xhr2 = CreateHeader("asynkroonne", "1");
            Assert.True(xhr2.Asünkroonne);
        }

        [Fact]
        public void CanParseAsynkroonneTrueValue()
        {
            var xhr2 = CreateHeader("asynkroonne", "true");
            Assert.True(xhr2.Asünkroonne);
        }

        [Fact]
        public void CanParseAsynkroonneFalseValue()
        {
            var xhr2 = CreateHeader("asynkroonne", "false");
            Assert.False(xhr2.Asünkroonne);
        }

        [Fact]
        public void CanParseAsynkroonne0Value()
        {
            var xhr2 = CreateHeader("asynkroonne", "0");
            Assert.False(xhr2.Asünkroonne);
        }

        [Fact]
        public void CanParseAsynkroonneEmptyValue()
        {
            var xhr2 = CreateHeader("asynkroonne", "");
            Assert.False(xhr2.Asünkroonne);
        }

        [Fact]
        public void CanParseAutentijaValue()
        {
            var xhr2 = CreateHeader("autentija", "Juss");
            Assert.Equal("Juss", xhr2.Autentija);
        }

        [Fact]
        public void CanParseMakstudValue()
        {
            var xhr2 = CreateHeader("makstud", "just");
            Assert.Equal("just", xhr2.Makstud);
        }

        [Fact]
        public void CanParseSalastadaValue()
        {
            var xhr2 = CreateHeader("salastada", "sha1");
            Assert.Equal("sha1", xhr2.Salastada);
        }

        [Fact]
        public void CanParseSalastadaSertifikaadigaValue()
        {
            var xhr2 = CreateHeader("salastada_sertifikaadiga", "bibopp");
            Assert.Equal("bibopp", xhr2.SalastadaSertifikaadiga);
        }

        [Fact]
        public void CanParseSalastatudValue()
        {
            var xhr2 = CreateHeader("salastatud", "sha1");
            Assert.Equal("sha1", xhr2.Salastatud);
        }

        [Fact]
        public void CanParseSalastatudSertifikaadigaValue()
        {
            var xhr2 = CreateHeader("salastatud_sertifikaadiga", "bibopp");
            Assert.Equal("bibopp", xhr2.SalastatudSertifikaadiga);
        }

        public IXRoadHeader20 CreateHeader(string name, string value)
        {
            var tuple = ParseXRoadHeaderHelper.ParseHeader($"<xrd:{name}>{value}</xrd:{name}>", NamespaceConstants.Xtee);
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader20>(tuple.Item1);
            Assert.Same(Globals.ServiceManager20, tuple.Item3);
            return (IXRoadHeader20)tuple.Item1;
        }
    }
}