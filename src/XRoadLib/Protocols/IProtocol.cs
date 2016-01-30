using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Protocols
{
    public interface IProtocol
    {
        string Name { get; }

        ISet<XName> MandatoryHeaders { get; }

        string RequestPartNameInRequest { get; }

        string RequestPartNameInResponse { get; }

        string ResponsePartNameInResponse { get; }

        string ProducerNamespace { get; }

        Style Style { get; }

        bool IsHeaderNamespace(string ns);

        bool IsDefinedByEnvelope(XmlReader reader);

        void ExportParameter(ParameterDefinition parameter);

        void ExportProperty(PropertyDefinition parameter);

        void ExportType(TypeDefinition type);

        void ExportOperation(OperationDefinition operation);

        void ExportServiceDescription(ServiceDescription serviceDescription);

        IXRoadHeader CreateHeader();

        void WriteServiceDescription(Stream outputStream, uint? version = null);

        XmlElement CreateOperationVersionElement(OperationDefinition operationDefinition);

        XmlElement CreateTitleElement(string languageCode, string value);

        void SetContractAssembly(Assembly assembly, params uint[] supportedVersions);

        ISerializerCache GetSerializerCache(uint? version = null);

        Assembly ContractAssembly { get; }

        IEnumerable<uint> SupportedVersions { get; }
    }

    public interface IProtocol<THeader> : IProtocol where THeader : IXRoadHeader
    {
        void AddMandatoryHeaderElement<T>(Expression<Func<THeader, T>> expression);
    }
}