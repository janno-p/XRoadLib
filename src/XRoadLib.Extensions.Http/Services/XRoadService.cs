using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions.Http.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.Http.Services
{
    public class XRoadService : IXRoadService
    {
        private readonly HttpClient _httpClient;

        protected IServiceManager ServiceManager { get; }

        public XRoadService(HttpClient httpClient, IServiceManager serviceManager)
        {
            _httpClient = httpClient;
            ServiceManager = serviceManager;
        }

        /// <inheritdoc />
        public virtual async Task<XRoadResponse<TResult>> RunOperationAsync<TRequest, TResult, THeader>(XRoadOperation<TRequest, TResult, THeader> operation, ServiceExecutionOptions options = null)
            where THeader : ISoapHeader
        {
            var messageFormatter = options?.MessageFormatter ?? new SoapMessageFormatter();

            var header = operation.Header;

            using var message = new XRoadMessage(ServiceManager, header) { BinaryMode = options?.BinaryMode ?? BinaryMode.Xml };

            IServiceMap operationServiceMap;
            using (var writer = XmlWriter.Create(message.ContentStream, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 }))
            {
                await writer.WriteStartDocumentAsync().ConfigureAwait(false);

                await writer.WriteSoapEnvelopeAsync(messageFormatter, ServiceManager.ProtocolDefinition).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(options?.RequestNamespace) && writer.LookupPrefix(options.RequestNamespace) == null)
                    await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, "req", NamespaceConstants.Xmlns, options.RequestNamespace).ConfigureAwait(false);

                await messageFormatter.WriteSoapHeaderAsync(writer, ServiceManager.Style, header, ServiceManager.HeaderDefinition).ConfigureAwait(false);
                await messageFormatter.WriteStartBodyAsync(writer).ConfigureAwait(false);

                var serviceCode = (header as IXRoadHeader)?.Service?.ServiceCode ?? string.Empty;

                var operationName = XName.Get(options?.OperationName ?? serviceCode, ServiceManager.ProducerNamespace);
                operationServiceMap = options?.ServiceMap ?? ServiceManager.GetSerializer(options?.Version ?? message.Version).GetServiceMap(operationName);
                await operationServiceMap.SerializeRequestAsync(writer, operation.Request, message, options?.RequestNamespace).ConfigureAwait(false);

                await writer.WriteEndElementAsync().ConfigureAwait(false);

                await writer.WriteEndElementAsync().ConfigureAwait(false);
                await writer.WriteEndDocumentAsync().ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            await OnBeforeRequestAsync(message).ConfigureAwait(false);

            using var httpResponseMessage = await _httpClient.SendXRoadMessageAsync(message, messageFormatter).ConfigureAwait(false);
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            using var seekableStream = new MemoryStream();
            using var responseMessage = new XRoadMessage();

            if (responseStream != null)
                await responseStream.CopyToAsync(seekableStream).ConfigureAwait(false);

            var storagePath = options?.StoragePath ?? new DirectoryInfo(Path.GetTempPath());

            await OnAfterResponseAsync(httpResponseMessage, seekableStream).ConfigureAwait(false);

            await responseMessage.LoadResponseAsync(
                seekableStream,
                messageFormatter,
                httpResponseMessage.Content.Headers.ContentType.ToString() ?? "text/xml; charset=UTF-8",
                storagePath,
                ServiceManager
            ).ConfigureAwait(false);

            var result = await responseMessage.DeserializeMessageContentAsync(operationServiceMap, messageFormatter).ConfigureAwait(false);
            
            var attachments = new List<XRoadAttachment>(responseMessage.AllAttachments);
            responseMessage.AllAttachments.Clear();

            return new XRoadResponse<TResult>((TResult)result, attachments);
        }

        protected virtual Task OnBeforeRequestAsync(XRoadMessage message) =>
            Task.CompletedTask;

        protected virtual Task OnAfterResponseAsync(HttpResponseMessage httpResponseMessage, Stream stream) =>
            Task.CompletedTask;
    }
}