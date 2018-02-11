using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Google.Protobuf;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Extensions.ProtoBuf.Serialization.Mapping
{
    /// <summary>
    /// Provides protocol buffers serialization/deserialization interface for X-Road operations.
    /// </summary>
    public class ProtoBufServiceMap : ServiceMap
    {
        private delegate object ReadValueMethod(Stream stream);
        private delegate void WriteValueMethod(Stream stream, object value);

        private readonly ReadValueMethod readRequestMethod;
        private readonly ReadValueMethod readResponseMethod;
        private readonly WriteValueMethod writeRequestMethod;
        private readonly WriteValueMethod writeResponseMethod;

        public ProtoBufServiceMap(ISerializerCache serializerCache, OperationDefinition operationDefinition, RequestDefinition requestDefinition, ResponseDefinition responseDefinition, ITypeMap inputTypeMap, ITypeMap outputTypeMap)
            : base(serializerCache, operationDefinition, requestDefinition, responseDefinition, inputTypeMap, outputTypeMap)
        {
            var requestType = RequestDefinition.ParameterInfo?.ParameterType;
            var responseType = ResponseDefinition.ParameterInfo?.ParameterType;

            readRequestMethod = BuildReadValueMethod(requestType);
            readResponseMethod = BuildReadValueMethod(responseType);
            writeRequestMethod = BuildWriteValueMethod(requestType);
            writeResponseMethod = BuildWriteValueMethod(responseType);
        }

        protected override object PrepareRequestValue(object value) => PrepareValue(value, writeRequestMethod);
        protected override object PrepareResponseValue(object value) => PrepareValue(value, writeResponseMethod);
        protected override object ProcessRequestValue(object value) => ProcessValue(value, readRequestMethod);
        protected override object ProcessResponseValue(object value) => ProcessValue(value, readResponseMethod);

        private static object PrepareValue(object value, WriteValueMethod writeValueMethod)
        {
            if (value == null)
                return null;

            var stream = new MemoryStream();
            writeValueMethod(stream, value);

            return stream;
        }

        private static object ProcessValue(object value, ReadValueMethod readValueMethod)
        {
            if (!(value is Stream stream))
                return value;

            stream.Position = 0;

            return readValueMethod(stream);
        }

        private static ReadValueMethod BuildReadValueMethod(Type definitionType)
        {
            if (definitionType == null)
                return o => null;

            var parserProperty = definitionType.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
            var parserType = parserProperty.PropertyType;
            var parseMethod = parserType.GetMethod("ParseFrom", new[] { typeof(Stream) });

            var dynamicRead = new DynamicMethod("DynamicRead", typeof(object), new[] { typeof(Stream) }, definitionType, true);
            var generator = dynamicRead.GetILGenerator();

            generator.Emit(OpCodes.Call, parserProperty.GetGetMethod());
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, parseMethod);
            generator.Emit(OpCodes.Ret);

            return (ReadValueMethod)dynamicRead.CreateDelegate(typeof(ReadValueMethod));
        }

        private static WriteValueMethod BuildWriteValueMethod(Type definitionType)
        {
            if (definitionType == null)
                return (o, v) => { };

            var method = typeof(MessageExtensions).GetMethod("WriteTo", BindingFlags.Public | BindingFlags.Static);

            var dynamicWrite = new DynamicMethod("DynamicWrite", typeof(void), new[] { typeof(Stream), typeof(object) }, definitionType, true);
            var generator = dynamicWrite.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Castclass, typeof(IMessage));

            generator.Emit(OpCodes.Ldarg_0);

            generator.Emit(OpCodes.Call, method);
            generator.Emit(OpCodes.Ret);

            return (WriteValueMethod)dynamicWrite.CreateDelegate(typeof(WriteValueMethod));
        }
    }
}