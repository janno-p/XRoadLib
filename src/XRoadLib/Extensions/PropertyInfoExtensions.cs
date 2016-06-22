using System.Reflection;
using System.Reflection.Emit;
using XRoadLib.Attributes;

namespace XRoadLib.Extensions
{
    internal static class PropertyInfoExtensions
    {
        internal static GetValueMethod CreateGetValueMethod(this PropertyInfo propertyInfo)
        {
            var type = propertyInfo.DeclaringType;
            if (type == null)
                return o => null;

            var converterAttribute = propertyInfo.GetSingleAttribute<XRoadRemoveContractAttribute>();
            var converterType = converterAttribute?.Converter;

            var propertyType = propertyInfo.PropertyType;

            var dynamicSet = new DynamicMethod("DynamicGet", typeof(object), new[] { typeof(object) }, type, true);
            var generator = dynamicSet.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);

            if (converterType != null)
                generator.Emit(OpCodes.Call, converterType.GetTypeInfo().GetMethod("ConvertBack", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
            else
                generator.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod(true));

            if (propertyType.GetTypeInfo().IsValueType)
                generator.Emit(OpCodes.Box, propertyType);

            generator.Emit(OpCodes.Ret);

            return (GetValueMethod)dynamicSet.CreateDelegate(typeof(GetValueMethod));
        }

        internal static SetValueMethod CreateSetValueMethod(this PropertyInfo propertyInfo)
        {
            var type = propertyInfo.DeclaringType;
            if (type == null)
                return (o, v) => { };

            var converterAttribute = propertyInfo.GetSingleAttribute<XRoadRemoveContractAttribute>();
            var converterType = converterAttribute?.Converter;

            var propertyType = propertyInfo.PropertyType;

            var dynamicSet = new DynamicMethod("DynamicSet", typeof(void), new[] { typeof(object), typeof(object) }, type, true);
            var generator = dynamicSet.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);

            if (propertyType.GetTypeInfo().IsValueType)
                generator.Emit(OpCodes.Unbox_Any, propertyType);

            if (converterType != null)
                generator.Emit(OpCodes.Call, converterType.GetTypeInfo().GetMethod("Convert", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
            else
                generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod(true));

            generator.Emit(OpCodes.Ret);

            return (SetValueMethod)dynamicSet.CreateDelegate(typeof(SetValueMethod));
        }
    }
}