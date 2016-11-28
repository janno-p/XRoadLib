using System;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Attributes
{
    /// <summary>
    /// Defines operation method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class XRoadServiceAttribute : Attribute
    {
        private static readonly Type serviceMapType = typeof(ServiceMap);

        internal uint? addedInVersion;
        internal uint? removedInVersion;

        /// <summary>
        /// Operation name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// ServiceMap type which implements operation definition.
        /// </summary>
        public virtual Type ServiceMapType => serviceMapType;

        /// <summary>
        /// Abstract operations do not define binding details.
        /// </summary>
        public virtual bool IsAbstract { get; set; }

        /// <summary>
        /// Hidden operations are not included in service description.
        /// </summary>
        public virtual bool IsHidden { get; set; }

        /// <summary>
        /// X-Road service version which first defined given operation.
        /// </summary>
        public virtual uint AddedInVersion { get { return addedInVersion.GetValueOrDefault(1u); } set { addedInVersion = value; } }

        /// <summary>
        /// X-Road service version which removed given operation.
        /// </summary>
        public virtual uint RemovedInVersion { get { return removedInVersion.GetValueOrDefault(uint.MaxValue); } set { removedInVersion = value; } }

        /// <summary>
        /// Does ServiceMap use TypeMaps to serialize service request and response elements.
        /// </summary>
        public virtual bool UseTypeMaps { get; } = true;

        /// <summary>
        /// Specifies if service extension wants to override default operation definition.
        /// </summary>
        public virtual void CustomizeOperationDefinition(OperationDefinition definition) { }

        /// <summary>
        /// Specifies if service extension wants to override default request value definition.
        /// </summary>
        public virtual void CustomizeRequestValueDefinition(RequestValueDefinition definition) { }

        /// <summary>
        /// Specifies if service extension wants to override default response value definition.
        /// </summary>
        public virtual void CustomizeResponseValueDefinition(ResponseValueDefinition definition) { }

        /// <summary>
        /// Specifies if service extension wants to override default schema location.
        /// </summary>
        public virtual string CustomizeSchemaLocation(string namespaceName)
        {
            return null;
        }

        /// <summary>
        /// Initializes new operation definition.
        /// </summary>
        public XRoadServiceAttribute(string name)
        {
            Name = name;
        }
    }
}