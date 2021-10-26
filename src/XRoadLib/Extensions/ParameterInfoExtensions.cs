using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace XRoadLib.Extensions
{
    public delegate Task<object> ConvertTaskMethod(Task task);

    public static class ParameterInfoExtensions
    {
        internal static ConvertTaskMethod CreateConvertTaskMethod(this ParameterInfo parameterInfo)
        {
            var type = parameterInfo?.ParameterType;
            if (type == null)
                return _ => null;

            var dynamicSet = new DynamicMethod("DynamicConvertTask", typeof(Task<object>), new[] { typeof(Task) });
            var generator = dynamicSet.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var gmi = typeof(ParameterInfoExtensions).GetMethod(nameof(ConvertTaskOfT), BindingFlags.Static | BindingFlags.NonPublic);
                var mi = gmi.MakeGenericMethod(type.GetGenericArguments().Single());
                generator.Emit(OpCodes.Call, mi);
                generator.Emit(OpCodes.Ret);
            }
            else if (type == typeof(Task))
            {
                var mi = typeof(ParameterInfoExtensions).GetMethod(nameof(ConvertTask), BindingFlags.Static | BindingFlags.NonPublic);
                generator.Emit(OpCodes.Call, mi);
                generator.Emit(OpCodes.Ret);
            }

            return (ConvertTaskMethod)dynamicSet.CreateDelegate(typeof(ConvertTaskMethod));
        }

        private static async Task<object> ConvertTask(Task task)
        {
            await task;
            return Task.FromResult((object)null).ConfigureAwait(false);
        }

        private static async Task<object> ConvertTaskOfT<T>(Task task)
        {
            var genericTask = (Task<T>)task;
            var result = await genericTask.ConfigureAwait(false);
            return result;
        }
    }
}