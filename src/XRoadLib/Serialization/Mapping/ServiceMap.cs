using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Serialization.Mapping
{
    public class ServiceMap : IServiceMap
    {
        private readonly XmlQualifiedName qualifiedName;
        private readonly IList<IParameterMap> parameters;
        private readonly IParameterMap result;

        public bool HasMultipartRequest { get; }
        public bool HasMultipartResponse { get; }

        public ServiceMap(XmlQualifiedName qualifiedName, IList<IParameterMap> parameters, IParameterMap result, bool hasMultipartRequest, bool hasMultipartResponse)
        {
            this.qualifiedName = qualifiedName;
            this.parameters = parameters;
            this.result = result;

            HasMultipartRequest = hasMultipartRequest;
            HasMultipartResponse = hasMultipartResponse;
        }

        public IDictionary<string, object> DeserializeRequest(XmlReader reader, SerializationContext context)
        {
            var elementName = GetRequestElementName(context);

            if (!reader.MoveToElement(3, elementName))
                throw XRoadException.ViganePäring($"Päringus puudub X-tee `{elementName}` element.");

            if (context.Protocol != XRoadProtocol.Version20)
            {
                var parameterValues = new List<Tuple<string, object>>();
                var parameterNodes = context.XmlTemplate.ParameterNodes.ToList();

                for (var i = 0; i < Math.Min(parameters.Count, parameterNodes.Count); i++)
                {
                    var parameterNode = parameterNodes[i];
                    parameterValues.Add(Tuple.Create(parameterNode.Name, parameters.Count < 2 ? parameters[i].DeserializeRoot(reader, parameterNode, context) : parameters[i].Deserialize(reader, parameterNode, context)));
                    reader.Read();
                }

                for (var i = 0; i < parameterNodes.Count; i++)
                    if (parameterNodes[i].IsRequired && (parameterValues.Count <= i || parameterValues[i].Item2 == null))
                        throw XRoadException.TeenuseKohustuslikParameeterPuudub(parameterValues[i].Item1);

                return parameterValues.ToDictionary(p => p.Item1, p => p.Item2);
            }
            else
            {
                var parameterValues = new Dictionary<string, object>();

                while (reader.Read() && reader.MoveToElement(4))
                {
                    var parameter = parameters.Single(x => x.Name == reader.LocalName);
                    parameterValues.Add(reader.LocalName, parameter.DeserializeRoot(reader, context.XmlTemplate.GetParameterNode(parameter.Name), context));
                }

                foreach (var parameterNode in context.XmlTemplate.ParameterNodes.Where(n => n.IsRequired && (!parameterValues.ContainsKey(n.Name) || parameterValues[n.Name] == null)))
                    throw XRoadException.TeenuseKohustuslikParameeterPuudub(parameterNode.Name);

                return parameterValues;
            }
        }

        public object DeserializeResponse(XmlReader reader, SerializationContext context)
        {
            var elementName = GetResponseElementName(context);

            if (!reader.MoveToElement(3, elementName))
                throw XRoadException.ViganePäring($"Päringus puudub X-tee `{elementName}` element.");

            return result.Deserialize(reader, context.XmlTemplate?.ResponseNode, context);
        }

        public void SerializeRequest(XmlWriter writer, IDictionary<string, object> values, SerializationContext context)
        {
            writer.WriteStartElement(qualifiedName.Name, qualifiedName.Namespace);
            writer.WriteStartElement(GetRequestElementName(context));

            foreach (var value in values)
            {
                var parameterMap = parameters.Single(x => x.Name == value.Key);
                parameterMap.Serialize(writer, context.XmlTemplate.GetParameterNode(parameterMap.Name), value.Value, context);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void SerializeResponse(XmlWriter writer, object value, SerializationContext context, XmlReader requestReader, Action afterSerialize = null)
        {
            var containsRequest = requestReader.MoveToElement(2, qualifiedName.Name, qualifiedName.Namespace);

            if (containsRequest)
                writer.WriteStartElement(requestReader.Prefix, $"{qualifiedName.Name}Response", qualifiedName.Namespace);
            else writer.WriteStartElement($"{qualifiedName.Name}Response", qualifiedName.Namespace);

            var namespaceInContext = requestReader.NamespaceURI;
            if (containsRequest)
                CopyRequestToResponse(writer, requestReader, context, out namespaceInContext);

            if (result != null)
            {
                if (Equals(namespaceInContext, ""))
                    writer.WriteStartElement(GetResponseElementName(context));
                else writer.WriteStartElement(GetResponseElementName(context), "");

                var fault = value as IXRoadFault;
                if (fault == null)
                    result.Serialize(writer, context.XmlTemplate?.ResponseNode, value, context);
                else SerializeFault(writer, fault, context.Protocol);

                writer.WriteEndElement();
            }

            afterSerialize?.Invoke();

            writer.WriteEndElement();
        }

        private static void SerializeFault(XmlWriter writer, IXRoadFault fault, XRoadProtocol protocol)
        {
            writer.WriteStartElement("faultCode");
            if (protocol == XRoadProtocol.Version20)
                writer.WriteTypeAttribute("string", NamespaceHelper.XSD);
            writer.WriteValue(fault.FaultCode);
            writer.WriteEndElement();

            writer.WriteStartElement("faultString");
            if (protocol == XRoadProtocol.Version20)
                writer.WriteTypeAttribute("string", NamespaceHelper.XSD);
            writer.WriteValue(fault.FaultString);
            writer.WriteEndElement();
        }

        private void CopyRequestToResponse(XmlWriter writer, XmlReader reader, SerializationContext context, out string namespaceInContext)
        {
            namespaceInContext = reader.NamespaceURI;

            writer.WriteAttributes(reader, true);

            if (!reader.MoveToElement(3) || !reader.IsCurrentElement(3, GetRequestElementName(context)))
                return;

            if (context.Protocol != XRoadProtocol.Version31)
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

        private static string GetResponseElementName(SerializationContext context)
        {
            return context.Protocol == XRoadProtocol.Version20 ? "keha" : "response";
        }
    }
}