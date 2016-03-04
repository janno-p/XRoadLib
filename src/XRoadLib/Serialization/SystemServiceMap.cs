using System;
using System.Xml.Linq;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Serialization
{
    public static class SystemServiceMap
    {
        private static readonly Lazy<IServiceMap> testSystemEncoded = new Lazy<IServiceMap>(() => CreateTestSystem(NamespaceConstants.XTEE));
        private static readonly Lazy<IServiceMap> testSystemLiteral = new Lazy<IServiceMap>(() => CreateTestSystem(NamespaceConstants.XROAD));
        private static readonly Lazy<IServiceMap> listMethodsEncoded = new Lazy<IServiceMap>(() => CreateListMethods(NamespaceConstants.XTEE));
        private static readonly Lazy<IServiceMap> listMethodsLiteral = new Lazy<IServiceMap>(() => CreateListMethods(NamespaceConstants.XROAD));

        public static IServiceMap TestSystemEncoded => testSystemEncoded.Value;
        public static IServiceMap TestSystemLiteral => testSystemLiteral.Value;
        public static IServiceMap ListMethodsEncoded => listMethodsEncoded.Value;
        public static IServiceMap ListMethodsLiteral => listMethodsLiteral.Value;

        private static IServiceMap CreateTestSystem(string xroadNamespace)
        {
            var methodInfo = typeof(MockMethods).GetMethod("TestSystem");

            var operationDefinition = new OperationDefinition(XName.Get("testSystem", xroadNamespace), null, methodInfo);

            var requestValueDefinition = new RequestValueDefinition(null, operationDefinition) { MergeContent = true };
            var responseValueDefinition = new ResponseValueDefinition(operationDefinition) { XRoadFaultPresentation = XRoadFaultPresentation.Implicit };

            return new ServiceMap(null, operationDefinition, requestValueDefinition, responseValueDefinition, null, null);
        }

        private static IServiceMap CreateListMethods(string xroadNamespace)
        {
            var methodInfo = typeof(MockMethods).GetMethod("ListMethods");

            var operationDefinition = new OperationDefinition(XName.Get("listMethods", xroadNamespace), null, methodInfo);

            var typeDefinition = new TypeDefinition(typeof(string))
            {
                Name = XName.Get("string", NamespaceConstants.XSD),
                IsSimpleType = true
            };

            var typeMap = new StringTypeMap(typeDefinition);

            var collectionDefinition = new CollectionDefinition(typeDefinition.Type.MakeArrayType())
            {
                ItemDefinition = typeDefinition,
                CanHoldNullValues = true,
                IsAnonymous = true
            };

            var arrayTypeMap = new ArrayTypeMap<string>(null, collectionDefinition, typeMap);

            var requestValueDefinition = new RequestValueDefinition(null, operationDefinition) { MergeContent = true };
            var responseValueDefinition = new ResponseValueDefinition(operationDefinition) { XRoadFaultPresentation = XRoadFaultPresentation.Implicit };

            return new ServiceMap(null, operationDefinition, requestValueDefinition, responseValueDefinition, null, arrayTypeMap);
        }


        private class MockMethods
        {
            public void TestSystem()
            { }

            public string[] ListMethods()
            {
                return null;
            }
        }
    }
}