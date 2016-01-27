using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Serialization.Template;
using XRoadLib.Soap;

namespace XRoadLib.Serialization.Mapping
{
    public class ServiceMap : IServiceMap
    {
        private readonly XName qualifiedName;
        private readonly IList<IParameterMap> parameters;
        private readonly IParameterMap result;
        private readonly XRoadContentLayoutMode contentLayout;

        public MethodInfo MethodInfo { get; }
        public bool HasMultipartRequest { get; }
        public bool HasMultipartResponse { get; }

        public ServiceMap(XName qualifiedName, MethodInfo methodInfo, IList<IParameterMap> parameters, IParameterMap result, XRoadContentLayoutMode contentLayout, bool hasMultipartRequest, bool hasMultipartResponse)
        {
            this.qualifiedName = qualifiedName;
            this.parameters = parameters;
            this.result = result;
            this.contentLayout = contentLayout;

            MethodInfo = methodInfo;
            HasMultipartRequest = hasMultipartRequest;
            HasMultipartResponse = hasMultipartResponse;
        }

        public IDictionary<string, object> DeserializeRequest(XmlReader reader, SerializationContext context)
        {
            var elementName = GetRequestElementName(context);

            if (!reader.MoveToElement(3, elementName))
                throw XRoadException.InvalidQuery($"Päringus puudub X-tee `{elementName}` element.");

            return contentLayout == XRoadContentLayoutMode.Strict ? DeserializeParametersStrict(reader, context) : DeserializeParametersNonStrict(reader, context);
        }

        private IDictionary<string, object> DeserializeParametersStrict(XmlReader reader, SerializationContext context)
        {
            var parameterValues = new List<Tuple<string, object>>();
            var parameterNodes = context.XmlTemplate?.ParameterNodes.ToList();

            var parameterEnumerator = parameters.GetEnumerator();
            var templateNodeEnumerator = context.XmlTemplate?.ParameterNodes.GetEnumerator();

            while (parameterEnumerator.MoveNext() && (templateNodeEnumerator == null || templateNodeEnumerator.MoveNext()))
            {
                var parameterMap = parameterEnumerator.Current;
                var parameterNode = templateNodeEnumerator != null ? templateNodeEnumerator.Current : XRoadXmlTemplate.EmptyNode;

                object value;
                if (parameters.Count < 2)
                    parameterValues.Add(Tuple.Create(parameterMap.ParameterInfo.Name, parameterMap.DeserializeRoot(reader, parameterNode, context)));
                else if (parameterMap.TryDeserialize(reader, parameterNode, context, out value))
                    parameterValues.Add(Tuple.Create(parameterMap.ParameterInfo.Name, value));
                else
                {
                    parameterValues.Add(Tuple.Create(parameterMap.ParameterInfo.Name, (object)null));
                    continue;
                }

                reader.Read();
            }

            if (parameterNodes != null)
                for (var i = 0; i < parameterNodes.Count; i++)
                    if (parameterNodes[i].IsRequired && (parameterValues.Count <= i || parameterValues[i].Item2 == null))
                        throw XRoadException.TeenuseKohustuslikParameeterPuudub(parameterValues[i].Item1);

            return parameterValues.ToDictionary(p => p.Item1, p => p.Item2);
        }

        private IDictionary<string, object> DeserializeParametersNonStrict(XmlReader reader, SerializationContext context)
        {
            var parameterValues = new Dictionary<string, object>();

            while (reader.Read() && reader.MoveToElement(4))
            {
                var parameter = parameters.SingleOrDefault(x => x.Name == reader.LocalName);
                if (parameter == null)
                    throw XRoadException.InvalidQuery("Unexpected parameter `{0}` in operation `{1}` request.", reader.LocalName, qualifiedName.LocalName);

                var parameterNode = context.XmlTemplate != null ? context.XmlTemplate.GetParameterNode(parameter.ParameterInfo.Name) : XRoadXmlTemplate.EmptyNode;
                parameterValues.Add(parameter.ParameterInfo.Name, parameter.DeserializeRoot(reader, parameterNode, context));
            }

            if (context.XmlTemplate != null)
                foreach (var parameterNode in context.XmlTemplate.ParameterNodes.Where(n => n.IsRequired && (!parameterValues.ContainsKey(n.Name) || parameterValues[n.Name] == null)))
                    throw XRoadException.TeenuseKohustuslikParameeterPuudub(parameterNode.Name);

            return parameterValues;
        }

        public object DeserializeResponse(XmlReader reader, SerializationContext context)
        {
            var elementName = context.Protocol.GetResponseElementName();
            var parameterNode = context.XmlTemplate != null ? context.XmlTemplate.ResponseNode : XRoadXmlTemplate.EmptyNode;

            if (!reader.MoveToElement(2))
                throw XRoadException.InvalidQuery("No payload element in SOAP message.");

            if (reader.NamespaceURI == NamespaceConstants.SOAP_ENV && reader.LocalName == "Fault")
                return SoapMessageHelper.DeserializeSoapFault(reader);

            if (!reader.MoveToElement(3, elementName.LocalName))
                throw XRoadException.InvalidQuery($"Expected payload element `{elementName}` was not found in SOAP message.");

            return result.DeserializeRoot(reader, parameterNode, context);
        }

        public void SerializeRequest(XmlWriter writer, IDictionary<string, object> values, SerializationContext context)
        {
            writer.WriteStartElement(qualifiedName.LocalName, qualifiedName.NamespaceName);
            writer.WriteStartElement(GetRequestElementName(context));

            foreach (var value in values)
            {
                var parameterMap = parameters.Single(x => x.Name == value.Key);
                var parameterNode = context.XmlTemplate != null ? context.XmlTemplate.GetParameterNode(parameterMap.Name) : XRoadXmlTemplate.EmptyNode;
                parameterMap.Serialize(writer, parameterNode, value.Value, context);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void SerializeResponse(XmlWriter writer, object value, SerializationContext context, XmlReader requestReader, ICustomSerialization customSerialization)
        {
            var containsRequest = requestReader.MoveToElement(2, qualifiedName.LocalName, qualifiedName.NamespaceName);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{qualifiedName.LocalName}Response", qualifiedName.NamespaceName);
            else writer.WriteStartElement($"{qualifiedName.LocalName}Response", qualifiedName.NamespaceName);

            var namespaceInContext = requestReader.NamespaceURI;
            if (containsRequest)
                CopyRequestToResponse(writer, requestReader, context, out namespaceInContext);

            if (result != null)
            {
                if (Equals(namespaceInContext, ""))
                    writer.WriteStartElement(context.Protocol.GetResponseElementName().LocalName);
                else writer.WriteStartElement(context.Protocol.GetResponseElementName().LocalName, "");

                var fault = value as IXRoadFault;
                if (fault == null)
                {
                    var responseNode = context.XmlTemplate != null ? context.XmlTemplate.ResponseNode : XRoadXmlTemplate.EmptyNode;
                    result.Serialize(writer, responseNode, value, context);
                }
                else SerializeFault(writer, fault, context.Protocol);

                writer.WriteEndElement();
            }

            customSerialization?.OnContentComplete(writer);

            writer.WriteEndElement();
        }

        private static void SerializeFault(XmlWriter writer, IXRoadFault fault, IProtocol protocol)
        {
            writer.WriteStartElement("faultCode");
            protocol.Style.WriteExplicitType(writer, XName.Get("string", NamespaceConstants.XSD));
            writer.WriteValue(fault.FaultCode);
            writer.WriteEndElement();

            writer.WriteStartElement("faultString");
            protocol.Style.WriteExplicitType(writer, XName.Get("string", NamespaceConstants.XSD));
            writer.WriteValue(fault.FaultString);
            writer.WriteEndElement();
        }

        private static void CopyRequestToResponse(XmlWriter writer, XmlReader reader, SerializationContext context, out string namespaceInContext)
        {
            namespaceInContext = reader.NamespaceURI;

            writer.WriteAttributes(reader, true);

            if (!reader.MoveToElement(3) || !reader.IsCurrentElement(3, GetRequestElementName(context)))
                return;

            if (context.Protocol == XRoadProtocol.Version20)
            {
                writer.WriteStartElement("paring");
                writer.WriteAttributes(reader, true);

                while (reader.MoveToElement(4))
                    writer.WriteNode(reader, true);

                writer.WriteEndElement();
            }
            else writer.WriteNode(reader, true);
        }

        private static string GetRequestElementName(SerializationContext context)
        {
            return context.Protocol == XRoadProtocol.Version20 ? "keha" : "request";
        }
    }
}