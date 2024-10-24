# Res\<T, E\>

## Basics

Given the type `MyError` used to represent errors:


```csharp
internal record MyError(string Message)
{
    public override string ToString() => $"Error: {Message}";
}
```

then the following code demonstrates the use of `Result<T, E>`:

```csharp
new Res<string, MyError>("yay").IsSuccess
    .Should().BeTrue();
```

## Match
```csharp
new Res<string, MyError>("yay")
    .Match(
        e => e.Message,
        s => s)
    .Should().Be("yay");
```

## Map
```csharp
new Res<string, MyError>("yay")
    .Map(s => s.Length)
    .Should().Be(new Res<int, MyError>(3));
```

## LINQ Query

```csharp
var res = new Res<string, MyError>("yay");

var resLen = from r in res
             select r.Length;

resLen
    .Should().Be(new Res<int, MyError>(3));
```
