﻿using System;
using System.Net;
using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;

namespace XRoadLib
{
    /// <summary>
    /// Handles service oriented tasks based on specific schema definition.
    /// </summary>
    public interface IServiceManager
    {
        /// <summary>
        /// User defined name for service manager in case of multiple definitions.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Operation binding style of Xml messages.
        /// </summary>
        Style Style { get; }

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        bool IsHeaderNamespace(string namespaceName);

        /// <summary>
        /// Check if envelope defines given protocol schema.
        /// </summary>
        bool IsDefinedByEnvelope(XmlReader reader);

        /// <summary>
        /// Main namespace which defines current producer operations and types.
        /// </summary>
        string ProducerNamespace { get; }

        /// <summary>
        /// Header definition of the protocol.
        /// </summary>
        HeaderDefinition HeaderDefinition { get; }

        /// <summary>
        /// Protocol specification.
        /// </summary>
        ProtocolDefinition ProtocolDefinition { get; }

        /// <summary>
        /// Get serializer instance of schema definition runtime serialization.
        /// </summary>
        /// <param name="version">Global DTO version of wanted serialzier.</param>
        /// <returns>Runtime type serializer.</returns>
        ISerializer GetSerializer(uint? version = null);

        /// <summary>
        /// Initializes header instance based on current definition.
        /// </summary>
        /// <returns>Initialized header instance.</returns>
        IXRoadHeader CreateHeader();

        /// <summary>
        /// Converts common header description to specific header.
        /// </summary>
        /// <returns>Initialized header instance.</returns>
        IXRoadHeader ConvertHeader(XRoadCommonHeader commonHeader);

        /// <summary>
        /// Generates service description based on current schema definition.
        /// </summary>
        /// <param name="operationFilter">Allows to filter out unwanted operations which should not appear in service description.</param>
        /// <param name="version">Global DTO version of wanted service description.</param>
        /// <returns>Service description instance of current schema definition.</returns>
        ServiceDescription CreateServiceDescription(Func<OperationDefinition, bool> operationFilter = null, uint? version = null);

        /// <summary>
        /// Executes X-Road operation on endpoint specified by WebRequest parameter.
        /// </summary>
        /// <param name="webRequest">WebRequest used to transfer X-Road messages.</param>
        /// <param name="body">Soap body part of outgoing serialized X-Road message.</param>
        /// <param name="header">Soap header part of outgoing serialized X-Road message.</param>
        /// <param name="options">Additional options to configure service call execution.</param>
        /// <typeparam name="TResult">Expected result type of the operation.</typeparam>
        /// <returns>Deserialized value of X-Road response message Soap body.</returns>
        TResult Execute<TResult>(WebRequest webRequest, object body, XRoadCommonHeader header, ServiceExecutionOptions options = null);
    }
}