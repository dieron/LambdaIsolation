using System;
using System.Linq.Expressions;
using System.Reflection;

public static class DynamicExecutor
{
    public static TResult Execute<TInterface, TResult>(Expression<Func<TInterface, TResult>> expression)
    {
        if (expression.Body is MethodCallExpression methodCall)
        {
            var methodInfo = methodCall.Method;
            var arguments = GetArgumentValues(methodCall.Arguments);

            TInterface instance = Factory.Create<TInterface>();
            return (TResult)methodInfo.Invoke(instance, arguments);
        }
        throw new ArgumentException("Expression should be a method call.");
    }

    public static void Execute<TInterface>(Expression<Action<TInterface>> expression)
    {
        if (expression.Body is MethodCallExpression methodCall)
        {
            var methodInfo = methodCall.Method;
            var arguments = GetArgumentValues(methodCall.Arguments);

            TInterface instance = Factory.Create<TInterface>();
            methodInfo.Invoke(instance, arguments);
            return;
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

        DynamicExecutor.Execute<IExample>(x => x.Print("Hello, World!"));
    }
}