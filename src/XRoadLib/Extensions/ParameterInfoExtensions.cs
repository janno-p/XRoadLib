using System.Reflection;
using System.Reflection.Emit;

namespace XRoadLib.Extensions;

public static class ParameterInfoExtensions
{
    internal static ConvertTaskMethod CreateConvertTaskMethod(this ParameterInfo? parameterInfo)
    {
        var type = parameterInfo?.ParameterType;
        if (type == null)
            return _ => Task.FromResult<object?>(null);

        var dynamicSet = new DynamicMethod("DynamicConvertTask", typeof(Task<object>), new[] { typeof(Task) });
        var generator = dynamicSet.GetILGenerator();

        generator.Emit(OpCodes.Ldarg_0);

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var gmi = GetConvertTaskMethod(nameof(ConvertTaskOfT));
            var mi = gmi.MakeGenericMethod(type.GetGenericArguments().Single());
            generator.Emit(OpCodes.Call, mi);
            generator.Emit(OpCodes.Ret);
        }
        else if (type == typeof(Task))
        {
            var mi = GetConvertTaskMethod(nameof(ConvertTask));
            generator.Emit(OpCodes.Call, mi);
            generator.Emit(OpCodes.Ret);
        }

        return (ConvertTaskMethod)dynamicSet.CreateDelegate(typeof(ConvertTaskMethod));
    }

    private static MethodInfo GetConvertTaskMethod(string methodName)
    {
#pragma warning disable S3011
        return typeof(ParameterInfoExtensions).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)!;
#pragma warning restore S3011
    }

    private static async Task<object?> ConvertTask(Task task)
    {
        await task.ConfigureAwait(false);
        return Task.FromResult((object?)null);
    }

    private static async Task<object?> ConvertTaskOfT<T>(Task task)
    {
        var genericTask = (Task<T>)task;
        var result = await genericTask.ConfigureAwait(false);
        return result;
    }
}