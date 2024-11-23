using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

public record MethodParameterDto
{
    public string TypeName { get; init; }
    public object Value { get; init; }
}

public record MethodCallDto
{
    public string InterfaceType { get; init; }
    public string MethodName { get; init; }
    public MethodParameterDto[] Arguments { get; init; }
}

public static class DynamicExecutor
{
    public static TResult Execute<TInterface, TResult>(Expression<Func<TInterface, TResult>> expression)
    {
        if (expression.Body is MethodCallExpression methodCall)
        {
            var dto = CreateMethodCallDto<TInterface>(methodCall);
            var json = SerializeMethodCall(dto);

            Console.WriteLine($"Serialized call: {json}");

            var deserializedDto = DeserializeMethodCall(json);
            return (TResult)InvokeMethod<TInterface>(deserializedDto);
        }
        throw new ArgumentException("Expression should be a method call.");
    }

    public static void Execute<TInterface>(Expression<Action<TInterface>> expression)
    {
        if (expression.Body is MethodCallExpression methodCall)
        {
            var dto = CreateMethodCallDto<TInterface>(methodCall);
            var json = SerializeMethodCall(dto);

            Console.WriteLine($"Serialized call: {json}");

            var deserializedDto = DeserializeMethodCall(json);
            InvokeMethod<TInterface>(deserializedDto);
            return;
        }
        throw new ArgumentException("Expression should be a method call.");
    }

    private static MethodCallDto CreateMethodCallDto<TInterface>(MethodCallExpression methodCall)
    {
        var parameters = methodCall.Method.GetParameters();
        var argumentValues = GetArgumentValues(methodCall.Arguments);

        return new MethodCallDto
        {
            InterfaceType = typeof(TInterface).AssemblyQualifiedName,
            MethodName = methodCall.Method.Name,
            Arguments = parameters.Select((p, i) => new MethodParameterDto
            {
                TypeName = p.ParameterType.AssemblyQualifiedName,
                Value = argumentValues[i]
            }).ToArray()
        };
    }

    private static string SerializeMethodCall(MethodCallDto dto)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        return JsonSerializer.Serialize(dto, options);
    }

    private static MethodCallDto DeserializeMethodCall(string json)
    {
        return JsonSerializer.Deserialize<MethodCallDto>(json);
    }

    private static object InvokeMethod<TInterface>(MethodCallDto dto)
    {
        var type = Type.GetType(dto.InterfaceType);
        var methodInfo = type.GetMethod(dto.MethodName);
        var arguments = dto.Arguments.Select(arg =>
        {
            var paramType = Type.GetType(arg.TypeName);
            var json = JsonSerializer.Serialize(arg.Value);
            return JsonSerializer.Deserialize(json, paramType);
        }).ToArray();

        var instance = Factory.Create<TInterface>();
        return methodInfo.Invoke(instance, arguments);
    }

    private static object[] GetArgumentValues(ReadOnlyCollection<Expression> arguments)
    {
        var argumentValues = new object[arguments.Count];
        for (int i = 0; i < arguments.Count; i++)
        {
            var lambda = Expression.Lambda(arguments[i]);
            var compiled = lambda.Compile();
            argumentValues[i] = compiled.DynamicInvoke();
        }
        return argumentValues;
    }
}

public static class Factory
{
    public static TInterface Create<TInterface>()
    {
        if (typeof(TInterface) == typeof(IExample))
        {
            return (TInterface)(object)new ExampleImplementation();
        }
        throw new InvalidOperationException($"No implementation registered for {typeof(TInterface).Name}");
    }
}

public interface IExample
{
    int Add(int x, int y);
    void Print(string message);

	string Concat(string[] values);
}

public class ExampleImplementation : IExample
{
    public int Add(int x, int y)
    {
        return x + y;
    }

    public void Print(string message)
    {
        Console.WriteLine(message);
    }

	public string Concat(string[] values)
	{
		return string.Concat(values);
	}
}

// Usage
class Program
{
    static void Main(string[] args)
    {
        // For methods that return a value
        var result = DynamicExecutor.Execute<IExample, int>(x => x.Add(5, 3));
        Console.WriteLine($"Result: {result}");

        // For methods that return void
        DynamicExecutor.Execute<IExample>(x => x.Print("Hello, World!"));

		// passing array of strings and concatenating them
		var result2 = DynamicExecutor.Execute<IExample, string>(x => x.Concat(new string[] { "Hello", " ", "World" }));
		Console.WriteLine($"Result: {result2}");
    }
}