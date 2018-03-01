﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using XRoadLib.Events;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNet
{
    /// <inheritdoc />
    public abstract class ServiceRequestHandlerBase : ServiceHandlerBase
    {
        private readonly ICollection<IServiceManager> serviceManagers;

        /// <summary>
        /// Temporary file storage location.
        /// </summary>
        public string StoragePath { get; set; }

        /// <summary>
        /// Serialization overrrides.
        /// </summary>
        public ICustomSerialization CustomSerialization { get; set; }

        /// <summary>
        /// Initialize new service request handler with protocols it can handle.
        /// </summary>
        protected ServiceRequestHandlerBase(IEnumerable<IServiceManager> serviceManagers)
        {
            if (serviceManagers == null)
                throw new ArgumentNullException(nameof(serviceManagers));
            this.serviceManagers = serviceManagers.ToList();
        }

        /// <summary>
        /// Handle X-Road message protocol meta-service request.
        /// </summary>
        protected abstract object InvokeMetaService(XRoadContext context);

        /// <summary>
        /// Get main service object which implements the functionality of
        /// the operation.
        /// </summary>
        protected abstract object GetServiceObject(XRoadContext context);

        /// <summary>
        /// Intercept X-Road service request after request message is loaded.
        /// </summary>
        protected virtual void OnRequestLoaded(XRoadContext context)
        { }

        /// <summary>
        /// Handle exception that occured on service method invokation.
        /// </summary>
        protected virtual void OnInvocationError(XRoadContext context)
        { }

        /// <summary>
        /// Customize XML reader settings before deserialization of the X-Road message.
        /// </summary>
        protected virtual void OnBeforeDeserialization(XRoadContext context, BeforeDeserializationEventArgs args)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after deserialization of the message.
        /// </summary>
        protected virtual void OnAfterDeserialization(XRoadContext context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling before serialization of the response message.
        /// </summary>
        protected virtual void OnBeforeSerialization(XRoadContext context)
        { }

        /// <summary>
        /// Intercept X-Road service request handling after serialization of the response message.
        /// </summary>
        protected virtual void OnAfterSerialization(XRoadContext context)
        { }

        /// <inheritdoc />
        protected override void HandleRequest(XRoadContext context)
        {
            if (context.HttpContext.Request.InputStream.Length == 0)
                throw XRoadException.InvalidQuery("Empty request content");

            context.Request.LoadRequest(context.HttpContext, StoragePath.GetValueOrDefault(Path.GetTempPath()), serviceManagers);
            if (context.Request.ServiceManager == null && context.Request.MetaServiceMap == null)
            {
                var supportedProtocolsString = string.Join(", ", serviceManagers.Select(x => $@"""{x.Name}"""));
                throw XRoadException.InvalidQuery($"Could not detect X-Road message protocol version from request message. Adapter supports following protocol versions: {supportedProtocolsString}.");
            }

            context.Response.Copy(context.Request);
            context.ServiceMap = context.Request.MetaServiceMap;

            OnRequestLoaded(context);
            InvokeServiceMethod(context);
            SerializeXRoadResponse(context);
        }

        /// <summary>
        /// Calls service method which implements the X-Road operation.
        /// </summary>
        protected virtual void InvokeServiceMethod(XRoadContext context)
        {
            if (context.ServiceMap != null)
            {
                context.Result = InvokeMetaService(context);
                return;
            }

            context.ServiceMap = context.Request.GetSerializer().GetServiceMap(context.Request.RootElementName);
            context.Response.BinaryMode = context.ServiceMap.OperationDefinition.OutputBinaryMode;

            var serviceObject = GetServiceObject(context);
            DeserializeMethodInput(context);

            try
            {
                var parameters = context.ServiceMap.RequestDefinition.ParameterInfo != null ? new[] { context.Parameters } : new object[0];
                context.Result = context.ServiceMap.OperationDefinition.MethodInfo.Invoke(serviceObject, parameters);
            }
            catch (Exception exception)
            {
                context.Exception = exception;
                OnInvocationError(context);

                if (context.Result == null)
                    throw;
            }
        }

        /// <summary>
        /// Deserializes X-Road request from SOAP message payload.
        /// </summary>
        protected virtual void DeserializeMethodInput(XRoadContext context)
        {
            var args = new BeforeDeserializationEventArgs();
            OnBeforeDeserialization(context, args);

            context.Request.ContentStream.Position = 0;
            var reader = XmlReader.Create(context.Request.ContentStream, args.XmlReaderSettings);

            reader.MoveToPayload(context.Request.RootElementName);

            context.Parameters = context.ServiceMap.DeserializeRequest(reader, context.Request);

            OnAfterDeserialization(context);
        }

        /// <summary>
        /// Serializes service result to a X-Road message response.
        /// </summary>
        protected virtual void SerializeXRoadResponse(XRoadContext context)
        {
            OnBeforeSerialization(context);

            context.Request.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(context.Request.ContentStream, new XmlReaderSettings { CloseInput = false }))
            {
                var writer = new XmlTextWriter(context.Response.ContentStream, context.Response.ContentEncoding);

                writer.WriteStartDocument();

                if (reader.MoveToElement(0) && reader.IsCurrentElement(0, "Envelope", NamespaceConstants.SOAP_ENV))
                {
                    writer.WriteStartElement(reader.Prefix, "Envelope", NamespaceConstants.SOAP_ENV);
                    writer.WriteAttributes(reader, true);
                }
                else
                {
                    writer.WriteStartElement(PrefixConstants.SOAP_ENV, "Envelope", NamespaceConstants.SOAP_ENV);
                    writer.WriteAttributeString("xmlns", PrefixConstants.SOAP_ENV, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENV);
                }

                if (reader.MoveToElement(1) && reader.IsCurrentElement(1, "Header", NamespaceConstants.SOAP_ENV))
                    writer.WriteNode(reader, true);

                writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);
                if (reader.IsCurrentElement(1, "Body", NamespaceConstants.SOAP_ENV) || reader.MoveToElement(1, "Body", NamespaceConstants.SOAP_ENV))
                    writer.WriteAttributes(reader, true);

                context.ServiceMap.SerializeResponse(writer, context.Result, context.Response, reader, CustomSerialization);

                writer.WriteEndDocument();
                writer.Flush();
            }

            context.Response.SaveTo(context.HttpContext);

            OnAfterSerialization(context);
        }
    }
}