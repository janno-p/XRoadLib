using System.Reflection;
using System.Xml.Schema;
using XRoadLib.Configuration;
using XRoadLib.Extensions;

namespace XRoadLib.Description
{
    internal class SchemaBuilder
    {
        private readonly XRoadSchemaBuilder xRoadSchema;

        internal SchemaBuilder(XRoadSchemaBuilder xRoadSchema)
        {
            this.xRoadSchema = xRoadSchema;
        }

        internal XmlSchemaElement CreateSchemaElement(PropertyInfo propertyInfo, ITypeConfiguration configuration)
        {
            var propertyName = propertyInfo.GetPropertyName(configuration);

            return CreateSchemaElement(propertyInfo, propertyName);
        }

        internal XmlSchemaElement CreateSchemaElement(ParameterInfo parameterInfo, IOperationConfiguration configuration)
        {
            var parameterName = parameterInfo.GetParameterName(configuration);

            return CreateSchemaElement(parameterInfo, parameterName);
        }

        private XmlSchemaElement CreateSchemaElement<T>(T source, string name)
            where T : ICustomAttributeProvider
        {
            var schemaElement = new XmlSchemaElement { Name = name, Annotation = xRoadSchema.CreateAnnotationFor(source) };

            if (!source.IsRequiredElement())
                schemaElement.MinOccurs = 0;

            return schemaElement;
        }
    }
}