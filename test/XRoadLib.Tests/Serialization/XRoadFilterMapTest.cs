using System;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadFilterMapTest
    {
        private class CustomClass
        {
            public string Member1 { get; set; } = "";
            public int Member2 { get; set; }
            public CustomClass Member3 { get; set; } = new CustomClass();
        }

        private class NoCustomMap : XRoadFilterMap<CustomClass>
        {
            public NoCustomMap()
                : base("Group name")
            { }
        }

        private class CustomMap : XRoadFilterMap<CustomClass>
        {
            public CustomMap()
                : base("CustomMap")
            {
                Enable(x => x.Member1);
                Enable(x => x.Member3);
                Enable(x => x.Member1);
                Enable(x => x.Member1);
            }
        }

        private class BrokenMap : XRoadFilterMap<CustomClass>
        {
            public BrokenMap()
                : base("BrokenMap")
            {
                Enable(x => x.Member3.Member1);
            }
        }

        private class Broken2Map : XRoadFilterMap<CustomClass>
        {
            public Broken2Map()
                : base("Broken2Map")
            {
                Enable(x => true);
            }
        }

        [Fact]
        public void MissingMappings()
        {
            var map = (IXRoadFilterMap)new NoCustomMap();
            Assert.Equal("Group name", map.GroupName);
            Assert.NotNull(map.EnabledProperties);
            Assert.Equal(0, map.EnabledProperties.Count);
        }

        [Fact]
        public void IgnoreReEnabledMembers()
        {
            var map = (IXRoadFilterMap)new CustomMap();
            Assert.Equal("CustomMap", map.GroupName);
            Assert.NotNull(map.EnabledProperties);
            Assert.Equal(2, map.EnabledProperties.Count);
            Assert.True(map.EnabledProperties.Contains("Member1"));
            Assert.True(map.EnabledProperties.Contains("Member3"));
        }

        [Fact]
        public void CannotHandleNestedProperties()
        {
            var exception = Assert.Throws<SchemaDefinitionException>(() => new BrokenMap());
            Assert.Equal("Only parameter members should be used in mapping definition (BrokenMap).", exception.Message);
        }

        [Fact]
        public void CannotHandleNonMemberExpressions()
        {
            var exception = Assert.Throws<SchemaDefinitionException>(() => new Broken2Map());
            Assert.Equal("MemberExpression expected, but was ConstantExpression (Broken2Map).", exception.Message);
        }
    }
}