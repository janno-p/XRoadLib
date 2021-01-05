using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Configurable part of operation.
    /// </summary>
    public class OperationDefinition : Definition
    {
        /// <summary>
        /// Runtime interface method which represents current operation.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// X-Road service version of the operation.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public uint Version { get; set; }

        /// <summary>
        /// Should this operation define binding part in service description?
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool IsAbstract { get; set; }

        /// <summary>
        /// Override input message name.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string InputMessageName { get; set; }

        /// <summary>
        /// Binary content serialization format for input messages.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public BinaryMode InputBinaryMode { get; set; }

        /// <summary>
        /// Does the operation return input message as part of output message.
        /// </summary>
        public bool CopyRequestPartToResponse { get; set; }

        /// <summary>
        /// Override output message name.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string OutputMessageName { get; set; }

        /// <summary>
        /// Binary content serialization format for output messages.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public BinaryMode OutputBinaryMode { get; set; }

        /// <summary>
        /// Serialization functionality of current operation.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public Type ServiceMapType { get; set; }

        /// <summary>
        /// Customized export options defined by extension.
        /// </summary>
        public ISchemaExporter ExtensionSchemaExporter { get; }

        /// <summary>
        /// Customized SOAPAction header value for this operation.
        /// </summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string SoapAction { get; set; }

        /// <summary>
        /// Initializes new definition object using default settings.
        /// </summary>
        public OperationDefinition(XName qualifiedName, uint? version, MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;

            var attribute = methodInfo.GetServices().SingleOrDefault(x => x.Name == qualifiedName.LocalName);

            Name = qualifiedName;
            IsAbstract = (attribute?.IsAbstract).GetValueOrDefault();
            InputBinaryMode = (attribute?.InputBinaryMode).GetValueOrDefault(BinaryMode.Xml);
            OutputBinaryMode = (attribute?.OutputBinaryMode).GetValueOrDefault(BinaryMode.Xml);
            State = (attribute?.IsHidden).GetValueOrDefault() ? DefinitionState.Hidden : DefinitionState.Default;
            Version = version.GetValueOrDefault(attribute?.AddedInVersion ?? 0u);
            CopyRequestPartToResponse = true;
            InputMessageName = qualifiedName.LocalName;
            OutputMessageName = $"{qualifiedName.LocalName}Response";
            Documentation = new DocumentationDefinition(methodInfo);
            ServiceMapType = attribute?.ServiceMapType ?? typeof(ServiceMap);
            ExtensionSchemaExporter = attribute?.SchemaExporter;
            SoapAction = attribute?.SoapAction ?? string.Empty;
        }
    }
}