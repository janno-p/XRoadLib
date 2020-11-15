using System;
using System.Collections.Generic;
using System.Xml.Linq;
using XRoadLib.Headers;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadHeader31Test
    {
        [Fact]
        public void CanParseEmptyElement()
        {
            var tuple = ParseHeader("<xrd:userId />");
            var header = tuple.Item1 as IXRoadHeader;
            Assert.NotNull(header);
            Assert.Equal("", header.UserId);
        }

        [Fact]
        public void CanParseConsumerValue()
        {
            var xhr3 = CreateHeader("consumer", "12345");
            var xhr = (IXRoadHeader)xhr3;
            Assert.NotNull(xhr.Client);
            Assert.Equal("12345", xhr.Client.MemberCode);
            Assert.Equal("12345", xhr3.Consumer);
        }

        [Fact]
        public void CanParseProducerValue()
        {
            var xhr3 = CreateHeader("producer", "andmekogu");
            var xhr = (IXRoadHeader)xhr3;
            Assert.NotNull(xhr.Service);
            Assert.Equal("andmekogu", xhr.Service.SubsystemCode);
            Assert.Equal("andmekogu", xhr3.Producer);
        }

        [Fact]
        public void CanParseUserIdValue()
        {
            var xhr = (IXRoadHeader)CreateHeader("userId", "Kasutaja");
            Assert.Equal("Kasutaja", xhr.UserId);
        }

        [Fact]
        public void CanParseIdValue()
        {
            var xhr = (IXRoadHeader)CreateHeader("id", "hash");
            Assert.Equal("hash", xhr.Id);
        }

        [Fact]
        public void CanParseServiceValue()
        {
            var xhr3 = CreateHeader("service", "producer.testService.v5");
            var xhr = (IXRoadHeader)xhr3;
            Assert.NotNull(xhr.Service);
            Assert.Null(xhr.Service.SubsystemCode);
            Assert.Equal("testService", xhr.Service.ServiceCode);
            Assert.Equal("v5", xhr.Service.ServiceVersion);
            Assert.Equal((uint?)5, xhr.Service.Version);
            Assert.Equal("testService.v5", xhr3.ServiceName);
        }

        [Fact]
        public void CanParseIssueValue()
        {
            var xhr = (IXRoadHeader)CreateHeader("issue", "toimik");
            Assert.Equal("toimik", xhr.Issue);
        }

        [Fact]
        public void CanParseUnitValue()
        {
            var xhr3 = CreateHeader("unit", "yksus");
            Assert.Equal("yksus", xhr3.Unit);
        }

        [Fact]
        public void CanParsePositionValue()
        {
            var xhr3 = CreateHeader("position", "ametikoht");
            Assert.Equal("ametikoht", xhr3.Position);
        }

        [Fact]
        public void CanParseUserNameValue()
        {
            var xhr3 = CreateHeader("userName", "Kuldar");
            Assert.Equal("Kuldar", xhr3.UserName);
        }

        [Fact]
        public void CanParseAsync1Value()
        {
            var xhr3 = CreateHeader("async", "1");
            Assert.True(xhr3.Async);
        }

        [Fact]
        public void CanParseAsyncTrueValue()
        {
            var xhr3 = CreateHeader("async", "true");
            Assert.True(xhr3.Async);
        }

        [Fact]
        public void CanParseAsyncFalseValue()
        {
            var xhr3 = CreateHeader("async", "false");
            Assert.False(xhr3.Async);
        }

        [Fact]
        public void CanParseAsync0Value()
        {
            var xhr3 = CreateHeader("async", "0");
            Assert.False(xhr3.Async);
        }

        [Fact]
        public void CanParseAsyncEmptyValue()
        {
            var xhr3 = CreateHeader("async", "");
            Assert.False(xhr3.Async);
        }

        [Fact]
        public void CanParseAuthenticatorValue()
        {
            var xhr3 = CreateHeader("authenticator", "Juss");
            Assert.Equal("Juss", xhr3.Authenticator);
        }

        [Fact]
        public void CanParsePaidValue()
        {
            var xhr3 = CreateHeader("paid", "just");
            Assert.Equal("just", xhr3.Paid);
        }

        [Fact]
        public void CanParseEncryptValue()
        {
            var xhr3 = CreateHeader("encrypt", "sha1");
            Assert.Equal("sha1", xhr3.Encrypt);
        }

        [Fact]
        public void CanParseEncryptCertValue()
        {
            var xhr3 = CreateHeader("encryptCert", "bibopp");
            Assert.Equal("bibopp", xhr3.EncryptCert);
        }

        [Fact]
        public void CanParseEncryptedValue()
        {
            var xhr3 = CreateHeader("encrypted", "sha1");
            Assert.Equal("sha1", xhr3.Encrypted);
        }

        [Fact]
        public void CanParseEncryptedCertValue()
        {
            var xhr3 = CreateHeader("encryptedCert", "bibopp");
            Assert.Equal("bibopp", xhr3.EncryptedCert);
        }

        public static Tuple<ISoapHeader, IList<XElement>, IServiceManager> ParseHeader(string xml)
        {
            return ParseXRoadHeaderHelper.ParseHeader(xml, NamespaceConstants.XRoad);
        }

        public static IXRoadHeader31 CreateHeader(string name, string value)
        {
            var tuple = ParseHeader($"<xrd:{name}>{value}</xrd:{name}>");
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader31>(tuple.Item1);
            Assert.Same(Globals.ServiceManager31, tuple.Item3);
            return (IXRoadHeader31)tuple.Item1;
        }
    }
}