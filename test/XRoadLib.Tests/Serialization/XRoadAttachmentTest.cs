using System;
using System.IO;
using System.Text;
using XRoadLib.Serialization;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadAttachmentTest
    {
        [Fact]
        public void CanWriteBase64()
        {
            var output = new StringBuilder();

            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes("ABC")))
            using (var attachment = new XRoadAttachment(contentStream))
            using (var writer = new StringWriter(output))
            {
                attachment.WriteAsBase64(writer);
                writer.Flush();
            }

            Assert.Equal("QUJD\r\n", output.ToString());
        }

        [Fact]
        public void CanWriteMultilineBase64()
        {
            const string text = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            var output = new StringBuilder();

            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            using (var attachment = new XRoadAttachment(contentStream))
            using (var writer = new StringWriter(output))
            {
                attachment.WriteAsBase64(writer);
                writer.Flush();
            }

            var encodedOutput = output.ToString();

            var lines = encodedOutput.Split(new [] { "\r\n" }, StringSplitOptions.None);
            Assert.Equal(9, lines.Length);
            Assert.Equal("TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwg", lines[0]);
            Assert.Equal("c2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0IGRvbG9yZSBtYWdu", lines[1]);
            Assert.Equal("YSBhbGlxdWEuIFV0IGVuaW0gYWQgbWluaW0gdmVuaWFtLCBxdWlzIG5vc3RydWQgZXhlcmNpdGF0", lines[2]);
            Assert.Equal("aW9uIHVsbGFtY28gbGFib3JpcyBuaXNpIHV0IGFsaXF1aXAgZXggZWEgY29tbW9kbyBjb25zZXF1", lines[3]);
            Assert.Equal("YXQuIER1aXMgYXV0ZSBpcnVyZSBkb2xvciBpbiByZXByZWhlbmRlcml0IGluIHZvbHVwdGF0ZSB2", lines[4]);
            Assert.Equal("ZWxpdCBlc3NlIGNpbGx1bSBkb2xvcmUgZXUgZnVnaWF0IG51bGxhIHBhcmlhdHVyLiBFeGNlcHRl", lines[5]);
            Assert.Equal("dXIgc2ludCBvY2NhZWNhdCBjdXBpZGF0YXQgbm9uIHByb2lkZW50LCBzdW50IGluIGN1bHBhIHF1", lines[6]);
            Assert.Equal("aSBvZmZpY2lhIGRlc2VydW50IG1vbGxpdCBhbmltIGlkIGVzdCBsYWJvcnVtLg==", lines[7]);
            Assert.Equal("", lines[8]);

            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

            Assert.NotEqual(encoded, encodedOutput);
            Assert.Equal(encoded, encodedOutput.Replace("\r\n", ""));
        }

        [Fact]
        public void CanWriteEmptyBase64()
        {
            var output = new StringBuilder();

            using (var contentStream = new MemoryStream())
            using (var attachment = new XRoadAttachment(contentStream))
            using (var writer = new StringWriter(output))
            {
                attachment.WriteAsBase64(writer);
                writer.Flush();
            }

            Assert.Equal("\r\n", output.ToString());
        }
    }
}