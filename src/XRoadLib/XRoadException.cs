using System;
using XRoadLib.Extensions;
using XRoadLib.Soap;

namespace XRoadLib
{
    public class XRoadException : Exception
    {
        public FaultCode FaultCode { get; }

        private XRoadException(FaultCode faultCode, string message, params object[] args)
            : base(string.Format(message, args))
        {
            FaultCode = faultCode;
        }

        public static XRoadException UnknownType(string typeName)
        {
            return new XRoadException(new ClientFaultCode("UnknownType"), "Unknown type `{0}`.", typeName);
        }

        public static XRoadException UnknownProperty(Type type, string propertyName)
        {
            return new XRoadException(new ClientFaultCode("UnknownProperty"), "Type '{0}' does not define property '{1}' (property names are case-sensitive).", type.GetTypeName(), propertyName);
        }

        public static XRoadException ParameterUndefinedInTemplate(string parameterName)
        {
            return new XRoadException(ServerFaultCode.InternalError, "Service template does not define parameter named '{0}'.", parameterName);
        }

        public static XRoadException AndmetüübileVastavNimeruumPuudub(string andmetüübiNimi)
        {
            return new XRoadException(ServerFaultCode.InternalError, "Ei tuvastatud Xml nimeruumi süsteemsele andmetüübile '{0}'.", andmetüübiNimi);
        }

        public static XRoadException TeenuseKohustuslikParameeterPuudub(string parameetriNimi)
        {
            return new XRoadException(new ClientFaultCode("ParameterRequired"), "Teenuse sisendist puudub kohustuslik parameeter '{0}'.", parameetriNimi);
        }

        public static XRoadException UnknownParameter(string parameterName)
        {
            return new XRoadException(new ClientFaultCode("UnknownParameter"), "Service does not define parameter named '{0}'.", parameterName);
        }

        public static XRoadException TypeAttributeRequired(string typeName)
        {
            return new XRoadException(new ClientFaultCode("TypeAttributeRequired"), "The type '{0}' is abstract, type attribute is required to specify target type.", typeName);
        }

        public static XRoadException PäringSisaldabVarasematKuupäeva(DateTime kuupäev)
        {
            return new XRoadException(new ClientFaultCode("UnsupportedDateValue"), "Päring sisaldab kuupäeva, mis on varasem kui '{0}'.", kuupäev);
        }

        public static XRoadException NoDefaultConstructorForType(string typeName)
        {
            return new XRoadException(ServerFaultCode.InternalError, "The type '{0}' does not have default constructor.", typeName);
        }

        public static XRoadException PäringusPuudubAttachment(string attachmentContentID)
        {
            return new XRoadException(new ClientFaultCode("MultipartAttachmentMissing"), "MIME multipart message does not contain content with ID `{0}`.", attachmentContentID);
        }

        public static XRoadException MultipartManusegaSõnumiOotamatuLõpp()
        {
            return new XRoadException(new ClientFaultCode("MultipartStreamEndMarkerMissing"), "MIME multipart sõnum lõppes enne lõppu tähistava märgise tuvastamist.");
        }

        public static XRoadException TundmatuNimeruum(string nimeruum)
        {
            return new XRoadException(new ClientFaultCode("UndefinedNamespace"), "X-tee sõnumis kasutatud andmetüübi nimeruum `{0}` on rakenduses defineerimata.", nimeruum);
        }

        public static XRoadException SamaAndmekoguNimiKorduvaltKasutuses(string andmekoguNimi)
        {
            return new XRoadException(ServerFaultCode.InternalError, "Sama andmekogu nimi '{0}' on korduvalt kasutuses.", andmekoguNimi);
        }

        public static XRoadException ToetamataKodeering(string kodeering)
        {
            return new XRoadException(new ClientFaultCode("UnsupportedContentTransferEncoding"), "Kodeering `{0}` ei ole rakenduse poolt toetatud.", kodeering);
        }

        public static XRoadException InvalidQuery(string message, params object[] args)
        {
            return new XRoadException(new ClientFaultCode("InvalidQuery"), string.Format(message, args));
        }

        public static XRoadException UnsupportedQuery()
        {
            return new XRoadException(new ClientFaultCode("UnsupportedQuery"), "Received request does not meet any known operation.");
        }

        public static XRoadException UndefinedContract(string operationName)
        {
            throw new XRoadException(new ServerFaultCode("UndefinedContract"), "Operation `{0}` does not implement any known service contract.", operationName);
        }

        public static XRoadException AmbiguousMatch(string operationName)
        {
            throw new XRoadException(new ServerFaultCode("AmbiguousMatch"), "Unable to detect unique service contract for operation `{0}` (method implements multiple service contracts).", operationName);
        }
    }
}