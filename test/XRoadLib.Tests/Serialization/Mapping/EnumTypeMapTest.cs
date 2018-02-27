﻿using System;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Tests.Contract;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class EnumTypeMapTest : TypeMapTestBase
    {
        private static readonly ITypeMap typeMap = new EnumTypeMap(schemaDefinitionProvider.GetTypeDefinition(typeof(Gender)));
        private readonly Func<string, object> deserializeValue = x => DeserializeValue(typeMap, x);

        [Fact]
        public void CanDeserializeEnumValue()
        {
            var value = deserializeValue("Female");
            Assert.Equal(Gender.Female, value);
        }

        [Fact]
        public void CannotDeserializeUnknownValue()
        {
            var ex = Assert.Throws<MissingFieldException>(() => deserializeValue("Random"));
            Assert.Equal("Unexpected value `Random` for enumeration type `{http://producers.test-producer.xtee.riik.ee/producer/test-producer}Gender`.", ex.Message);
        }

        [Fact]
        public void CannotDeserializeEmptyValue()
        {
            var ex = Assert.Throws<MissingFieldException>(() => deserializeValue(""));
            Assert.Equal("Unexpected value `` for enumeration type `{http://producers.test-producer.xtee.riik.ee/producer/test-producer}Gender`.", ex.Message);
        }

        [Fact]
        public void CannotDeserializeSelfClosingEmptyValue()
        {
            var ex = Assert.Throws<MissingFieldException>(() => DeserializeEmptyValue(typeMap));
            Assert.Equal("Unexpected value `` for enumeration type `{http://producers.test-producer.xtee.riik.ee/producer/test-producer}Gender`.", ex.Message);
        }
    }
}