using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    /// <summary>
    /// Provides implementation details about X-Road services defined by contract.
    /// </summary>
    public class ServiceMap : IServiceMap
    {
        private readonly ISerializer _serializer;
        private readonly ITypeMap _inputTypeMap;
        private readonly ITypeMap _outputTypeMap;

        /// <inheritdoc />
        public OperationDefinition OperationDefinition { get; }

        /// <inheritdoc />
        public RequestDefinition RequestDefinition { get; }

        /// <inheritdoc />
        public ResponseDefinition ResponseDefinition { get; }

        private bool HasWrapperResultElement =>
            !ResponseDefinition.Content.MergeContent
            && ResponseDefinition.XRoadFaultPresentation != XRoadFaultPresentation.Implicit
            && ResponseDefinition.ContainsNonTechnicalFault;

        /// <summary>
        /// Initializes new ServiceMap entity using settings specified in operationDefinition.
        /// <param name="serializer">Provides TypeMap lookup.</param>
        /// <param name="operationDefinition">Operation which this ServiceMap represents.</param>
        /// <param name="requestDefinition">Defines operation request message.</param>
        /// <param name="responseDefinition">Defines operation response message.</param>
        /// <param name="inputTypeMap">Default TypeMap of the operation request root element.</param>
        /// <param name="outputTypeMap"> Default TypeMap of the operation response root element.</param>
        /// </summary>
        public ServiceMap(ISerializer serializer, OperationDefinition operationDefinition, RequestDefinition requestDefinition, ResponseDefinition responseDefinition, ITypeMap inputTypeMap, ITypeMap outputTypeMap)
        {
            _serializer = serializer;

            RequestDefinition = requestDefinition;
            ResponseDefinition = responseDefinition;
            OperationDefinition = operationDefinition;

            _inputTypeMap = inputTypeMap is IContentTypeMap inputContentTypeMap && requestDefinition.Content.UseXop
                ? inputContentTypeMap.GetOptimizedContentTypeMap()
                : inputTypeMap;

            _outputTypeMap = outputTypeMap is IContentTypeMap outputContentTypeMap && responseDefinition.Content.UseXop
                ? outputContentTypeMap.GetOptimizedContentTypeMap()
                : outputTypeMap;
        }

        /// <inheritdoc />
        public async Task<object> DeserializeRequestAsync(XmlReader reader, XRoadMessage message)
        {
            var requestName = RequestDefinition.Content.Name;

            if (!RequestDefinition.Content.MergeContent && !await reader.MoveToElementAsync(3, requestName).ConfigureAwait(false))
                throw new InvalidQueryException($"Request wrapper element `{requestName}` was not found in incoming SOAP message.");

            return RequestDefinition.ParameterInfo != null
                ? ProcessRequestValue(await DeserializeValueAsync(reader, _inputTypeMap, message.RequestNode, RequestDefinition, message).ConfigureAwait(false))
                : null;
        }

        /// <inheritdoc />
        public async Task<object> DeserializeResponseAsync(XmlReader reader, XRoadMessage message)
        {
            var requestName = ResponseDefinition.RequestContentName;
            var responseName = ResponseDefinition.Content.Name;

            if (OperationDefinition.CopyRequestPartToResponse)
            {
                if (!await reader.MoveToElementAsync(3, requestName).ConfigureAwait(false))
                    throw new InvalidQueryException($"Expected payload element `{requestName}` was not found in SOAP message.");
                await reader.ReadAsync().ConfigureAwait(false);
            }

            var hasResponseElement = await reader.MoveToElementAsync(3).ConfigureAwait(false);

            if (hasResponseElement && !ResponseDefinition.ContainsNonTechnicalFault && reader.GetXName() == ResponseDefinition.FaultName)
                return await reader.ReadXRoadFaultAsync(4).ConfigureAwait(false);

            if (!hasResponseElement || reader.GetXName() != responseName)
                throw new InvalidQueryException($"Expected payload element `{responseName}` in SOAP message, but `{reader.GetXName()}` was found instead.");

            var hasWrapperElement = HasWrapperResultElement;
            if (hasWrapperElement && !await reader.MoveToElementAsync(4, ResponseDefinition.ResultElementName).ConfigureAwait(false))
                throw new InvalidQueryException($"Expected result wrapper element `{ResponseDefinition.ResultElementName}` was not found in SOAP message.");

            return ProcessResponseValue(await DeserializeValueAsync(reader, _outputTypeMap, message.ResponseNode, ResponseDefinition, message).ConfigureAwait(false));
        }

        private async Task<object> DeserializeValueAsync(XmlReader reader, ITypeMap typeMap, IXmlTemplateNode templateNode, ParticleDefinition particleDefinition, XRoadMessage message)
        {
            if (reader.IsNilElement())
            {
                await reader.ReadToEndElementAsync().ConfigureAwait(false);
                return null;
            }

            XName typeAttribute;
            if (typeMap.Definition.IsAnonymous && !(typeMap is IArrayTypeMap) && (typeAttribute = reader.GetTypeAttributeValue()) != null)
                throw new UnknownTypeException($"Expected anonymous type, but `{typeAttribute}` was given.", particleDefinition, typeMap.Definition, typeAttribute);

            var concreteTypeMap = typeMap;
            if (!particleDefinition.Content.IgnoreExplicitType)
                concreteTypeMap = (typeMap.Definition.IsInheritable ? _serializer.GetTypeMapFromXsiType(reader, particleDefinition) : null) ?? typeMap;

            return await concreteTypeMap.DeserializeAsync(reader, templateNode, particleDefinition.Content, message).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task SerializeRequestAsync(XmlWriter writer, object value, XRoadMessage message, string requestNamespace = null)
        {
            var requestWrapperElementName = RequestDefinition.WrapperElementName;
            var ns = string.IsNullOrEmpty(requestNamespace) ? requestWrapperElementName.NamespaceName : requestNamespace;
            var addPrefix = writer.LookupPrefix(ns) == null;

            if (addPrefix) await writer.WriteStartElementAsync(PrefixConstants.Target, requestWrapperElementName.LocalName, ns).ConfigureAwait(false);
            else await writer.WriteStartElementAsync(null, requestWrapperElementName.LocalName, ns).ConfigureAwait(false);

            if (!RequestDefinition.Content.MergeContent)
                await writer.WriteStartElementAsync(RequestDefinition.Content.Name).ConfigureAwait(false);

            if (RequestDefinition.ParameterInfo != null)
                await SerializeValueAsync(writer, PrepareRequestValue(value), _inputTypeMap, message.RequestNode, message, RequestDefinition.Content).ConfigureAwait(false);

            if (!RequestDefinition.Content.MergeContent)
                await writer.WriteEndElementAsync().ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Serializes X-Road message protocol responses according to operation definitions.
        /// </summary>
        public async Task SerializeResponseAsync(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization)
        {
            var containsRequest = await requestReader.MoveToElementAsync(2, RequestDefinition.WrapperElementName).ConfigureAwait(false);

            await writer.WriteStartElementAsync(ResponseDefinition.WrapperElementName).ConfigureAwait(false);

            if (containsRequest && OperationDefinition.CopyRequestPartToResponse)
                await CopyRequestToResponseAsync(writer, requestReader).ConfigureAwait(false);

            var fault = value as IXRoadFault;

            if (!ResponseDefinition.ContainsNonTechnicalFault && fault != null)
            {
                await writer.WriteStartElementAsync(ResponseDefinition.FaultName).ConfigureAwait(false);
                await message.Style.SerializeFaultAsync(writer, fault).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }
            else if (_outputTypeMap != null)
            {
                await writer.WriteStartElementAsync(ResponseDefinition.Content.Name).ConfigureAwait(false);

                if (fault != null)
                    await message.Style.SerializeFaultAsync(writer, fault).ConfigureAwait(false);
                else if (_outputTypeMap != null)
                {
                    var addWrapperElement = HasWrapperResultElement;
                    if (addWrapperElement)
                        await writer.WriteStartElementAsync(ResponseDefinition.ResultElementName).ConfigureAwait(false);

                    await SerializeValueAsync(writer, PrepareResponseValue(value), _outputTypeMap, message.ResponseNode, message, ResponseDefinition.Content).ConfigureAwait(false);

                    if (addWrapperElement)
                        await writer.WriteEndElementAsync().ConfigureAwait(false);
                }

                await writer.WriteEndElementAsync().ConfigureAwait(false);

                if (customSerialization != null)
                    await customSerialization.OnContentCompleteAsync(writer).ConfigureAwait(false);
            }

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        protected virtual object PrepareRequestValue(object value) => value;
        protected virtual object PrepareResponseValue(object value) => value;
        protected virtual object ProcessRequestValue(object value) => value;
        protected virtual object ProcessResponseValue(object value) => value;

        private async Task SerializeValueAsync(XmlWriter writer, object value, ITypeMap typeMap, IXmlTemplateNode templateNode, XRoadMessage message, ContentDefinition content)
        {
            if (value == null)
            {
                await writer.WriteNilAttributeAsync().ConfigureAwait(false);
                return;
            }

            var concreteTypeMap = typeMap.Definition.IsInheritable ? _serializer.GetTypeMap(value.GetType()) : typeMap;

            await concreteTypeMap.SerializeAsync(writer, templateNode, value, content, message).ConfigureAwait(false);
        }

        private async Task CopyRequestToResponseAsync(XmlWriter writer, XmlReader reader)
        {
            await writer.WriteAttributesAsync(reader, true).ConfigureAwait(false);

            if (!await reader.MoveToElementAsync(3).ConfigureAwait(false) || !reader.IsCurrentElement(3, RequestDefinition.Content.Name))
                return;

            if (RequestDefinition.Content.Name != ResponseDefinition.RequestContentName)
            {
                await writer.WriteStartElementAsync(ResponseDefinition.RequestContentName).ConfigureAwait(false);
                await writer.WriteAttributesAsync(reader, true).ConfigureAwait(false);

                while (await reader.MoveToElementAsync(4).ConfigureAwait(false))
                    await writer.WriteNodeAsync(reader, true).ConfigureAwait(false);

                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }
            else await writer.WriteNodeAsync(reader, true).ConfigureAwait(false);
        }
    }
}