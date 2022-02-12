using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using XRoadLib.Events;
using XRoadLib.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore;

/// <summary>
/// Base class of service request handlers.
/// </summary>
[UsedImplicitly]
public class WebServiceRequestHandler : WebServiceHandler
{
    protected virtual IServiceProvider ServiceProvider { get; }

    public DirectoryInfo? StoragePath { get; set; }

    /// <summary>
    /// Initialize new service request handler with X-Road message protocols
    /// it should be able to handle and storage path of temporary files.
    /// </summary>
    public WebServiceRequestHandler(IServiceProvider serviceProvider, IServiceManager serviceManager)
        : base(serviceManager)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Handle incoming web request as X-Road service request.
    /// </summary>
    public override async Task HandleRequestAsync(WebServiceContext context)
    {
        if (context.HttpContext.Request.Body.CanSeek && context.HttpContext.Request.Body.Length == 0)
            throw new InvalidQueryException("Empty request content");

        var messageFormatter = context.MessageFormatter ?? DefaultMessageFormatter;

        await context.Request.LoadRequestAsync(context.HttpContext, messageFormatter, GetStorageOrTempPath().FullName, ServiceManager).ConfigureAwait(false);
        context.Response.Copy(context.Request);

        context.HttpContext.Response.ContentType = messageFormatter.ContentType;

        await OnRequestLoadedAsync(context).ConfigureAwait(false);
        await InvokeWebServiceAsync(context).ConfigureAwait(false);
        await SerializeXRoadResponseAsync(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Handle X-Road message protocol meta-service request.
    /// </summary>
    [UsedImplicitly]
    protected virtual Task<object?> InvokeMetaServiceAsync(WebServiceContext context) =>
        Task.FromResult<object?>(null);

    /// <summary>
    /// Get main service object which implements the functionality of
    /// the operation.
    /// </summary>
    protected virtual Task<object> GetServiceObjectAsync(WebServiceContext context)
    {
        if (context.ServiceMap is null)
            throw new InvalidOperationException("WebServiceContext does not define ServiceMap");

        var operationDefinition = context.ServiceMap.OperationDefinition;

        var service = operationDefinition.MethodInfo.DeclaringType != null
            ? ServiceProvider.GetRequiredService(operationDefinition.MethodInfo.DeclaringType)
            : null;

        if (service == null)
            throw new SchemaDefinitionException($"Operation {operationDefinition.Name} is not implemented by contract.");

        return Task.FromResult(service);
    }

    /// <summary>
    /// Intercept X-Road service request after request message is loaded.
    /// </summary>
    [UsedImplicitly]
    protected virtual Task OnRequestLoadedAsync(WebServiceContext context) =>
        Task.CompletedTask;

    /// <summary>
    /// Handle exception that occurred on service method invokation.
    /// </summary>
    [UsedImplicitly]
    protected virtual Task OnInvocationErrorAsync(WebServiceContext context) =>
        Task.CompletedTask;

    /// <summary>
    /// Customize XML reader settings before deserialization of the X-Road message.
    /// </summary>
    [UsedImplicitly]
    protected virtual Task OnBeforeDeserializationAsync(WebServiceContext context, BeforeDeserializationEventArgs args) =>
        Task.CompletedTask;

    /// <summary>
    /// Intercept X-Road service request handling after deserialization of the message.
    /// </summary>
    [UsedImplicitly]
    protected virtual Task OnAfterDeserializationAsync(WebServiceContext context) =>
        Task.CompletedTask;

    /// <summary>
    /// Intercept X-Road service request handling before serialization of the response message.
    /// </summary>
    [UsedImplicitly]
    protected virtual Task OnBeforeSerializationAsync(WebServiceContext context) =>
        Task.CompletedTask;

    /// <summary>
    /// Intercept X-Road service request handling after serialization of the response message.
    /// </summary>
    [UsedImplicitly]
    protected virtual Task OnAfterSerializationAsync(WebServiceContext context) =>
        Task.CompletedTask;

    protected virtual XName ResolveOperationName(WebServiceContext context)
    {
        if (context.Request.RootElementName == null)
            throw new UnknownOperationException("Could not resolve operation name from request.", context.Request.RootElementName);

        return context.Request.RootElementName;
    }

    /// <summary>
    /// Calls service method which implements the X-Road operation.
    /// </summary>
    protected virtual async Task InvokeWebServiceAsync(WebServiceContext context)
    {
        if (context.ServiceMap != null)
        {
            context.Result = await InvokeMetaServiceAsync(context).ConfigureAwait(false);
            return;
        }

        var operationName = ResolveOperationName(context);

        context.ServiceMap = context.Request.GetSerializer().GetServiceMap(operationName);
        context.Response.BinaryMode = context.ServiceMap.OperationDefinition.OutputBinaryMode;

        var serviceObject = await GetServiceObjectAsync(context).ConfigureAwait(false);
        await DeserializeMethodInputAsync(context).ConfigureAwait(false);

        try
        {
            var parameters = context.ServiceMap.RequestDefinition.ParameterInfo != null ? new[] { context.Parameters! } : Array.Empty<object>();
            context.Result = await InvokeRuntimeMethodAsync(context, serviceObject, parameters).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            context.Exception = exception;
            await OnInvocationErrorAsync(context).ConfigureAwait(false);

            if (context.Result == null)
                throw;
        }
    }

    protected virtual async Task<object> InvokeRuntimeMethodAsync(WebServiceContext context, object serviceObject, object[] parameters)
    {
        if (context.ServiceMap is null)
            throw new InvalidOperationException("ServiceMap is not define for current WebService request");

        if (!context.ServiceMap.ResponseDefinition.IsAsync)
            return context.ServiceMap.OperationDefinition.MethodInfo.Invoke(serviceObject, parameters);

        var task = (Task)context.ServiceMap.OperationDefinition.MethodInfo.Invoke(serviceObject, parameters);
        await task.ConfigureAwait(false);

        return await context.ServiceMap.ResponseDefinition.ConvertTaskMethod(task).ConfigureAwait(false);
    }

    /// <summary>
    /// Deserializes X-Road request from SOAP message payload.
    /// </summary>
    protected virtual async Task DeserializeMethodInputAsync(WebServiceContext context)
    {
        var args = new BeforeDeserializationEventArgs();
        await OnBeforeDeserializationAsync(context, args).ConfigureAwait(false);

        args.XmlReaderSettings ??= new XmlReaderSettings();
        args.XmlReaderSettings.Async = true;

        context.Request.ContentStream.Position = 0;
        var reader = XmlReader.Create(context.Request.ContentStream, args.XmlReaderSettings);

        await (context.MessageFormatter ?? DefaultMessageFormatter).MoveToPayloadAsync(reader, context.Request.RootElementName).ConfigureAwait(false);

        if (context.ServiceMap is null)
            throw new InvalidOperationException("ServiceMap is not define for current WebService request");

        context.Parameters = await context.ServiceMap.DeserializeRequestAsync(reader, context.Request).ConfigureAwait(false);

        await OnAfterDeserializationAsync(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Serializes service result to a X-Road message response.
    /// </summary>
    protected virtual async Task SerializeXRoadResponseAsync(WebServiceContext context)
    {
        await OnBeforeSerializationAsync(context).ConfigureAwait(false);

        var messageFormatter = context.MessageFormatter ?? DefaultMessageFormatter;

        context.Request.ContentStream.Position = 0;
        using (var reader = XmlReader.Create(context.Request.ContentStream, new XmlReaderSettings { Async = true, CloseInput = false }))
        using (var textWriter = new StreamWriter(context.Response.ContentStream, context.Response.ContentEncoding, 1024, true))
        using (var writer = XmlWriter.Create(textWriter, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 }))
        {
            await writer.WriteStartDocumentAsync().ConfigureAwait(false);

            if (await messageFormatter.TryMoveToEnvelopeAsync(reader).ConfigureAwait(false))
            {
                await messageFormatter.WriteStartEnvelopeAsync(writer, reader.Prefix).ConfigureAwait(false);
                await writer.WriteAttributesAsync(reader, true).ConfigureAwait(false);
                await writer.WriteMissingAttributesAsync(ServiceManager.ProtocolDefinition).ConfigureAwait(false);
            }
            else await writer.WriteSoapEnvelopeAsync(context.MessageFormatter, ServiceManager.ProtocolDefinition).ConfigureAwait(false);

            if (await messageFormatter.TryMoveToHeaderAsync(reader).ConfigureAwait(false))
                await writer.WriteNodeAsync(reader, true).ConfigureAwait(false);

            await messageFormatter.WriteStartBodyAsync(writer).ConfigureAwait(false);
            if (await messageFormatter.TryMoveToBodyAsync(reader).ConfigureAwait(false))
                await writer.WriteAttributesAsync(reader, true).ConfigureAwait(false);

            await SerializeServiceResultAsync(context, reader, writer).ConfigureAwait(false);

            await writer.WriteEndDocumentAsync().ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }

        await context.Response.SaveToAsync(context.HttpContext, messageFormatter).ConfigureAwait(false);

        await OnAfterSerializationAsync(context).ConfigureAwait(false);
    }

    protected virtual Task SerializeServiceResultAsync(WebServiceContext context, XmlReader requestReader, XmlWriter responseWriter)
    {
        if (context.ServiceMap is null)
            throw new InvalidOperationException("ServiceMap is not define for current WebService request");

        return context.ServiceMap.SerializeResponseAsync(responseWriter, context.Result, context.Response, requestReader);
    }

    private DirectoryInfo GetStorageOrTempPath()
    {
        return StoragePath ?? new DirectoryInfo(Path.GetTempPath());
    }
}