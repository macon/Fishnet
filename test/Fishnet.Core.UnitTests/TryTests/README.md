# Try\<T\> & Exc\<T\>

## Basics

### Exc\<T\>

`Exc<T>` allows you to model a type that may contain a `T` or an `Exception`.

### Try\<T\>

`Try<T>` allows you to ensure a function (that returns `T`) is executed without throwing exceptions.

`Try<T>` is the delegate:

```csharp
public delegate Exc<T> Try<T>();
```

As you can see it works alongside `Exc<T>` and you can use it like this:

```csharp
private static Try<int> TryDivide(int a, int b) => () => a / b;

TryDivide(10, 0).Run().IsException
    .Should().BeTrue();
```

As you can see you must return a lambda that represents your operation. The `Run()` method will execute the lambda and return an `Exc<T>`.

## Match

```csharp
TryDivide(10, 0).Run()
    .Match<string>(
        success: i => i.ToString(),
        ex: e => e.Message)
    .Should().Be("Attempted to divide by zero.");
```

A slightly more realistic example could be:

Given an external service that may throw an exception:
```csharp
public static class DodgyExternalService
{
    public static string GetName(int id) =>
        id <= 10 ? $"Employee-{id}" : throw new Exception("Boom");
}
```

You can write the following to ensure exceptions don't bubble up:

```csharp
Try(() => DodgyExternalService.GetName(9)).Run()
    .Should().Be(Ok("Employee-9"));

Try(() => ExternalService.GetName(11)).Run().IsSuccess
    .Should().BeFalse();

Try(() => ExternalService.GetName(11)).Run()
    .Match(
        success: s => s,
        ex: e => e.Message)
    .Should().Be("Boom");
```

Notice we use the static helper `Ok("Employee-9")` to create a `Exc<T>` that represents a successful operation.

See the [TryTests](./TryTests.cs) for more complete examples.
