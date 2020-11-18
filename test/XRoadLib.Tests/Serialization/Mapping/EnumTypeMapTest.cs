using System;
using System.Threading.Tasks;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Tests.Contract;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class EnumTypeMapTest : TypeMapTestBase
    {
        private static readonly ITypeMap TypeMap = new EnumTypeMap(SchemaDefinitionProvider.GetTypeDefinition(typeof(Gender)));

        private readonly Func<string, Task<object>> _deserializeValueAsync = x => DeserializeValueAsync(TypeMap, x);

        [Fact]
        public async Task CanDeserializeEnumValue()
        {
            var value = await _deserializeValueAsync("Female");
            Assert.Equal(Gender.Female, value);
        }

        [Fact]
        public async Task CannotDeserializeUnknownValue()
        {
            var ex = await Assert.ThrowsAsync<UnexpectedValueException>(() => _deserializeValueAsync("Random"));
            Assert.Equal("Unexpected value `Random` for enumeration type `{urn:some-namespace}Gender`.", ex.Message);
            Assert.Same(ex.TypeDefinition.Type, typeof(Gender));
            Assert.True(ex.Value.Equals("Random"));
        }

        [Fact]
        public async Task CannotDeserializeEmptyValue()
        {
            var ex = await Assert.ThrowsAsync<UnexpectedValueException>(() => _deserializeValueAsync(""));
            Assert.Equal("Unexpected value `` for enumeration type `{urn:some-namespace}Gender`.", ex.Message);
            Assert.Same(ex.TypeDefinition.Type, typeof(Gender));
            Assert.True(ex.Value.Equals(""));
        }

        [Fact]
        public async Task CannotDeserializeSelfClosingEmptyValue()
        {
            var ex = await Assert.ThrowsAsync<UnexpectedValueException>(() => DeserializeEmptyValueAsync(TypeMap));
            Assert.Equal("Unexpected value `` for enumeration type `{urn:some-namespace}Gender`.", ex.Message);
            Assert.Same(ex.TypeDefinition.Type, typeof(Gender));
            Assert.True(ex.Value.Equals(""));
        }
    }
}