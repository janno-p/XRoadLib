using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XRoadLib.Soap;

namespace XRoadLib
{
    public class XRoadException : Exception
    {
        public FaultCode FaultCode { get; }

        private XRoadException(FaultCode faultCode, string message)
            : base(message)
        {
            FaultCode = faultCode;
        }

        public static XRoadException UnknownType(string typeName)
        {
            return new XRoadException(new ClientFaultCode("UnknownType"), $"Unknown type `{typeName}`.");
        }

        public static XRoadException UnknownOperation(XName typeName)
        {
            return new XRoadException(new ClientFaultCode("UnknownOperation"), $"Unknown operation `{typeName}`.");
        }

        public static XRoadException UnknownProperty(string propertyName, XName typeName)
        {
            return new XRoadException(new ClientFaultCode("UnknownProperty"), $"Type '{typeName}' does not define property '{propertyName}' (property names are case-sensitive).");
        }

        public static XRoadException ParameterUndefinedInTemplate(string parameterName)
        {
            return new XRoadException(ServerFaultCode.InternalError, $"Service template does not define parameter named '{parameterName}'.");
        }

        public static XRoadException AndmetüübileVastavNimeruumPuudub(string andmetüübiNimi)
        {
            return new XRoadException(ServerFaultCode.InternalError, $"Ei tuvastatud Xml nimeruumi süsteemsele andmetüübile '{andmetüübiNimi}'.");
        }

        public static XRoadException MissingRequiredPropertyValues(IEnumerable<string> propertyNames)
        {
            var missingParameters = string.Join(", ", propertyNames.Select(x => $"`{x}`"));
            return new XRoadException(new ClientFaultCode("ParameterRequired"), $"Service input is missing required parameters: {missingParameters}.");
        }

        public static XRoadException TypeAttributeRequired(string typeName)
        {
            return new XRoadException(new ClientFaultCode("TypeAttributeRequired"), $"The type '{typeName}' is abstract, type attribute is required to specify target type.");
        }

        public static XRoadException PäringSisaldabVarasematKuupäeva(DateTime kuupäev)
        {
            return new XRoadException(new ClientFaultCode("UnsupportedDateValue"), $"Päring sisaldab kuupäeva, mis on varasem kui '{kuupäev}'.");
        }

        public static XRoadException NoDefaultConstructorForType(XName qualifiedName)
        {
            return new XRoadException(ServerFaultCode.InternalError, $"The type '{qualifiedName}' does not have default constructor.");
        }

        public static XRoadException PäringusPuudubAttachment(string attachmentContentID)
        {
            return new XRoadException(new ClientFaultCode("MultipartAttachmentMissing"), $"MIME multipart message does not contain content with ID `{attachmentContentID}`.");
        }

        public static XRoadException MultipartManusegaSõnumiOotamatuLõpp()
        {
            return new XRoadException(new ClientFaultCode("MultipartStreamEndMarkerMissing"), "MIME multipart sõnum lõppes enne lõppu tähistava märgise tuvastamist.");
        }

        public static XRoadException ToetamataKodeering(string kodeering)
        {
            return new XRoadException(new ClientFaultCode("UnsupportedContentTransferEncoding"), $"Kodeering `{kodeering}` ei ole rakenduse poolt toetatud.");
        }

        public static XRoadException InvalidQuery(string message)
        {
            return new XRoadException(new ClientFaultCode("InvalidQuery"), message);
        }

        public static XRoadException UndefinedContract(string operationName)
        {
            throw new XRoadException(new ServerFaultCode("UndefinedContract"), $"Operation `{operationName}` does not implement any known service contract.");
        }

        public static XRoadException AmbiguousMatch(string operationName)
        {
            throw new XRoadException(new ServerFaultCode("AmbiguousMatch"), $"Unable to detect unique service contract for operation `{operationName}` (method implements multiple service contracts).");
        }
    }
}