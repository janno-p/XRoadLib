using System;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Tests.Contract;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class EnumTypeMapTest : TypeMapTestBase
    {
        private static readonly ITypeMap TypeMap = new EnumTypeMap(SchemaDefinitionProvider.GetTypeDefinition(typeof(Gender)));

        private readonly Func<string, object> _deserializeValue = x => DeserializeValue(TypeMap, x);

        [Fact]
        public void CanDeserializeEnumValue()
        {
            var value = _deserializeValue("Female");
            Assert.Equal(Gender.Female, value);
        }

        [Fact]
        public void CannotDeserializeUnknownValue()
        {
            var ex = Assert.Throws<UnexpectedValueException>(() => _deserializeValue("Random"));
            Assert.Equal("Unexpected value `Random` for enumeration type `{http://producers.test-producer.xtee.riik.ee/producer/test-producer}Gender`.", ex.Message);
            Assert.Same(ex.TypeDefinition.Type, typeof(Gender));
            Assert.True(ex.Value.Equals("Random"));
        }

        [Fact]
        public void CannotDeserializeEmptyValue()
        {
            var ex = Assert.Throws<UnexpectedValueException>(() => _deserializeValue(""));
            Assert.Equal("Unexpected value `` for enumeration type `{http://producers.test-producer.xtee.riik.ee/producer/test-producer}Gender`.", ex.Message);
            Assert.Same(ex.TypeDefinition.Type, typeof(Gender));
            Assert.True(ex.Value.Equals(""));
        }

        [Fact]
        public void CannotDeserializeSelfClosingEmptyValue()
        {
            var ex = Assert.Throws<UnexpectedValueException>(() => DeserializeEmptyValue(TypeMap));
            Assert.Equal("Unexpected value `` for enumeration type `{http://producers.test-producer.xtee.riik.ee/producer/test-producer}Gender`.", ex.Message);
            Assert.Same(ex.TypeDefinition.Type, typeof(Gender));
            Assert.True(ex.Value.Equals(""));
        }
    }
}