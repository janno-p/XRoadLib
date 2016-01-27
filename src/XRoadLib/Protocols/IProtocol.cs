using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Protocols
{
    public interface IProtocol
    {
        string Name { get; }

        string ProducerNamespace { get; }

        Style Style { get; }

        ISerializerCache SerializerCache { get; }

        bool IsHeaderNamespace(string ns);

        bool IsDefinedByEnvelope(XmlReader reader);

        void ExportType(TypeDefinition type);

        void ExportOperation(OperationDefinition operation);

        void ExportServiceDescription(ServiceDescription serviceDescription, Context context);

        IXRoadHeader CreateHeader();

        void WriteServiceDescription(Assembly contractAssembly, Stream outputStream);
    }

    public interface IProtocol<THeader> : IProtocol where THeader : IXRoadHeader
    {
        void AddMandatoryHeaderElement<T>(Expression<Func<THeader, T>> expression);
    }
}