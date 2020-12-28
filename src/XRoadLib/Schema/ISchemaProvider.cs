using System;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public interface ISchemaProvider
    {
        string XRoadPrefix { get; }

        string XRoadNamespace { get; }

        TypeDefinition GetTypeDefinition(Type type, string typeName = null);

        TypeDefinition GetSimpleTypeDefinition<T>(string typeName);

        CollectionDefinition GetCollectionDefinition(TypeDefinition typeDefinition);

        PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition);

        OperationDefinition GetOperationDefinition(Type operationType, XName qualifiedName, uint? version);

        RequestDefinition GetRequestDefinition(OperationDefinition operationDefinition);

        ResponseDefinition GetResponseDefinition(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation = null);

        IHeaderDefinition GetXRoadHeaderDefinition();

        ProtocolDefinition GetProtocolDefinition();

        FaultDefinition GetFaultDefinition();

        bool IsQualifiedElementDefault(string namespaceName);

        string GetSchemaLocation(string namespaceName);
    }
}