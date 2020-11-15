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
        public object DeserializeRequest(XmlReader reader, XRoadMessage message)
        {
            var requestName = RequestDefinition.Content.Name;

            if (!RequestDefinition.Content.MergeContent && !reader.MoveToElement(3, requestName))
                throw new InvalidQueryException($"Request wrapper element `{requestName}` was not found in incoming SOAP message.");

            return RequestDefinition.ParameterInfo != null
                ? ProcessRequestValue(DeserializeValue(reader, _inputTypeMap, message.RequestNode, RequestDefinition, message))
                : null;
        }

        /// <inheritdoc />
        public object DeserializeResponse(XmlReader reader, XRoadMessage message)
        {
            var requestName = ResponseDefinition.RequestContentName;
            var responseName = ResponseDefinition.Content.Name;

            if (OperationDefinition.CopyRequestPartToResponse)
            {
                if (!reader.MoveToElement(3, requestName))
                    throw new InvalidQueryException($"Expected payload element `{requestName}` was not found in SOAP message.");
                reader.Read();
            }

            var hasResponseElement = reader.MoveToElement(3);

            if (hasResponseElement && !ResponseDefinition.ContainsNonTechnicalFault && reader.GetXName() == ResponseDefinition.FaultName)
                return reader.ReadXRoadFault(4);

            if (!hasResponseElement || reader.GetXName() != responseName)
                throw new InvalidQueryException($"Expected payload element `{responseName}` in SOAP message, but `{reader.GetXName()}` was found instead.");

            var hasWrapperElement = HasWrapperResultElement;
            if (hasWrapperElement && !reader.MoveToElement(4, ResponseDefinition.ResultElementName))
                throw new InvalidQueryException($"Expected result wrapper element `{ResponseDefinition.ResultElementName}` was not found in SOAP message.");

            return ProcessResponseValue(DeserializeValue(reader, _outputTypeMap, message.ResponseNode, ResponseDefinition, message));
        }

        private object DeserializeValue(XmlReader reader, ITypeMap typeMap, IXmlTemplateNode templateNode, ParticleDefinition particleDefinition, XRoadMessage message)
        {
            if (reader.IsNilElement())
            {
                reader.ReadToEndElement();
                return null;
            }

            XName typeAttribute;
            if (typeMap.Definition.IsAnonymous && !(typeMap is IArrayTypeMap) && (typeAttribute = reader.GetTypeAttributeValue()) != null)
                throw new UnknownTypeException($"Expected anonymous type, but `{typeAttribute}` was given.", particleDefinition, typeMap.Definition, typeAttribute);

            var concreteTypeMap = typeMap;
            if (!particleDefinition.Content.IgnoreExplicitType)
                concreteTypeMap = (typeMap.Definition.IsInheritable ? _serializer.GetTypeMapFromXsiType(reader, particleDefinition) : null) ?? typeMap;

            return concreteTypeMap.Deserialize(reader, templateNode, particleDefinition.Content, message);
        }

        /// <inheritdoc />
        public void SerializeRequest(XmlWriter writer, object value, XRoadMessage message, string requestNamespace = null)
        {
            var requestWrapperElementName = RequestDefinition.WrapperElementName;
            var ns = string.IsNullOrEmpty(requestNamespace) ? requestWrapperElementName.NamespaceName : requestNamespace;
            var addPrefix = writer.LookupPrefix(ns) == null;

            if (addPrefix) writer.WriteStartElement(PrefixConstants.Target, requestWrapperElementName.LocalName, ns);
            else writer.WriteStartElement(requestWrapperElementName.LocalName, ns);

            if (!RequestDefinition.Content.MergeContent)
                writer.WriteStartElement(RequestDefinition.Content.Name);

            if (RequestDefinition.ParameterInfo != null)
                SerializeValue(writer, PrepareRequestValue(value), _inputTypeMap, message.RequestNode, message, RequestDefinition.Content);

            if (!RequestDefinition.Content.MergeContent)
                writer.WriteEndElement();

            writer.WriteEndElement();
        }

        /// <summary>
        /// Serializes X-Road message protocol responses according to operation definitions.
        /// </summary>
        public void SerializeResponse(XmlWriter writer, object value, XRoadMessage message, XmlReader requestReader, ICustomSerialization customSerialization)
        {
            var containsRequest = requestReader.MoveToElement(2, RequestDefinition.WrapperElementName);

            writer.WriteStartElement(ResponseDefinition.WrapperElementName);

            if (containsRequest && OperationDefinition.CopyRequestPartToResponse)
                CopyRequestToResponse(writer, requestReader);

            var fault = value as IXRoadFault;

            if (!ResponseDefinition.ContainsNonTechnicalFault && fault != null)
            {
                writer.WriteStartElement(ResponseDefinition.FaultName);
                message.Style.SerializeFault(writer, fault);
                writer.WriteEndElement();
            }
            else if (_outputTypeMap != null)
            {
                writer.WriteStartElement(ResponseDefinition.Content.Name);

                if (fault != null)
                    message.Style.SerializeFault(writer, fault);
                else if (_outputTypeMap != null)
                {
                    var addWrapperElement = HasWrapperResultElement;
                    if (addWrapperElement)
                        writer.WriteStartElement(ResponseDefinition.ResultElementName);

                    SerializeValue(writer, PrepareResponseValue(value), _outputTypeMap, message.ResponseNode, message, ResponseDefinition.Content);

                    if (addWrapperElement)
                        writer.WriteEndElement();
                }

                writer.WriteEndElement();

                customSerialization?.OnContentComplete(writer);
            }

            writer.WriteEndElement();
        }

        protected virtual object PrepareRequestValue(object value) => value;
        protected virtual object PrepareResponseValue(object value) => value;
        protected virtual object ProcessRequestValue(object value) => value;
        protected virtual object ProcessResponseValue(object value) => value;

        private void SerializeValue(XmlWriter writer, object value, ITypeMap typeMap, IXmlTemplateNode templateNode, XRoadMessage message, ContentDefinition content)
        {
            if (value == null)
            {
                writer.WriteNilAttribute();
                return;
            }

            var concreteTypeMap = typeMap.Definition.IsInheritable ? _serializer.GetTypeMap(value.GetType()) : typeMap;

            concreteTypeMap.Serialize(writer, templateNode, value, content, message);
        }

        private void CopyRequestToResponse(XmlWriter writer, XmlReader reader)
        {
            writer.WriteAttributes(reader, true);

            if (!reader.MoveToElement(3) || !reader.IsCurrentElement(3, RequestDefinition.Content.Name))
                return;

            if (RequestDefinition.Content.Name != ResponseDefinition.RequestContentName)
            {
                writer.WriteStartElement(ResponseDefinition.RequestContentName);
                writer.WriteAttributes(reader, true);

                while (reader.MoveToElement(4))
                    writer.WriteNode(reader, true);

                writer.WriteEndElement();
            }
            else writer.WriteNode(reader, true);
        }
    }
}