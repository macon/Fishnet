
# Fishnet
> _An FP stocking filler._

---

### A collection of _functional style_ types for C#, including:

### Option

`Opt<T>`

Models an instance of a type that may or may not be present.

### Discriminated Union
`One<T, ..Tn>`

A type that can only be one of a named set.

### Result
`Res<T> / Res<T, E>`

Better validation handling.

### Try/Exc
`Try<T> / Exc<T>`

Better error handling.

---

Below are some basic examples and links to further info.

## [Opt\<T\>](test/Functional.Core.UnitTests/OptionTests/README.md)

Models the behavior of a value that may or may not be present.

```csharp
Some("Matt").IsSome
    .Should().BeTrue();

None.Of<string>().IsSome
    .Should().BeFalse();

Some(9)
    .Match(
        some: n => n > 10,
        none: () => false)
    .Should()
    .Be(false);
```

## [One\<T, Tn\>](test/Functional.Core.UnitTests/OneTests/README.md)

_(A C# version of a Discriminated Union.)_

Models the behavior of a type that may only contain a value of one of the named subtypes (i.e. `T1` or `T2`).

```csharp
One<int, string> ageOrName = 35;

ageOrName
    .Should().Be(35);

ageOrName.IsT1
    .Should().Be(true);

ageOrName.Match(
        age => $"Age: {age}",
        name => $"Name: {name}")
    .Should().Be("Age: 35");
```

## [Res\<T, E\>](test/Functional.Core.UnitTests/ResultTests/README.md)

Models a result that may be successful (T) or unsuccessful (E) without throwing exceptions.

```csharp
new Res<string, MyError>("yay").IsSuccess
    .Should().BeTrue();
```

## [Try\<T\> & Exc\<T\>](test/Functional.Core.UnitTests/TryTests/README.md)

These type combine enabling you to manage exception throwing code.

```csharp
private static Try<int> TryDivide(int a, int b) => () => a / b;

TryDivide(10, 0).Run().IsException
    .Should().BeTrue();
```

## [Res\<T\>](test/Functional.Core.UnitTests/ResTests/README.md)

`Res<T>` is a specialization of `Res<T, E>` where the error type is predetermined. In our case it's the
[Error](src/Functional.Core/Error.cs) type.

```csharp
