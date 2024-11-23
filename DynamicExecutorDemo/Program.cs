using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

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
			var parameters = methodCall.Method.GetParameters();
			var argumentValues = GetArgumentValues(methodCall.Arguments);

			// Serialize with type information
			var dto = new MethodCallDto
			{
				InterfaceType = typeof(TInterface).AssemblyQualifiedName,
				MethodName = methodCall.Method.Name,
				Arguments = parameters.Select((p, i) => new MethodParameterDto
				{
					TypeName = p.ParameterType.AssemblyQualifiedName,
					Value = argumentValues[i]
				}).ToArray()
			};

			var options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			var json = JsonSerializer.Serialize(dto, options);
			Console.WriteLine($"Serialized call: {json}");

			// Deserialize and convert types
			var deserializedDto = JsonSerializer.Deserialize<MethodCallDto>(json);
			var type = Type.GetType(deserializedDto.InterfaceType);
			var instance = Factory.Create<TInterface>();
			var methodInfo = type.GetMethod(deserializedDto.MethodName);

			// Convert arguments to correct types
			var typedArguments = deserializedDto.Arguments
				.Select((arg, i) =>
				{
					var paramType = Type.GetType(arg.TypeName);
					return JsonSerializer.Deserialize(
						JsonSerializer.Serialize(arg.Value),
						paramType
					);
				}).ToArray();

			return (TResult)methodInfo.Invoke(instance, typedArguments);
		}
		throw new ArgumentException("Expression should be a method call.");
	}


	private static object[] GetArgumentValues(System.Collections.ObjectModel.ReadOnlyCollection<Expression> arguments)
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


	// Similar changes for void Execute<TInterface> method...
}

public static class Factory
{
	public static TInterface Create<TInterface>()
	{
		// Implement your factory logic here. For demo purposes, use Activator.
		if (typeof(TInterface) == typeof(IExample))
		{
			return (TInterface)(object)new ExampleImplementation();
		}
		throw new NotImplementedException($"Factory doesn't know how to create type {typeof(TInterface).Name}");
	}
}

public interface IExample
{
	int Add(int x, int y);
	void Print(string message);
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
}



// Demo usage
class Program
{
	static void Main(string[] args)
	{
		var result = DynamicExecutor.Execute<IExample, int>(x => x.Add(5, 3));
		Console.WriteLine($"Result: {result}");

		// DynamicExecutor.Execute<IExample>(x => x.Print("Hello, World!"));
	}
}