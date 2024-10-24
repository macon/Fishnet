# One\<T1, T2\>

## Basics
```csharp
// Declaration and assignment
One<int, string> ageOrName = 35;
var zipOrEmail = new One<int, string>("matt@company.com");

ageOrName
    .Should().Be(35);

ageOrName.IsT1
    .Should().Be(true);

```

## Match
```csharp
One<int, string> ageOrName = 35;

ageOrName.Match(
        age => $"Age: {age}",
        name => $"Name: {name}")
    .Should().Be("Age: 35");
```

## Map
```csharp
// Map applies a function if the Opt is Some.
// The mapping function must return another Opt<T> (but T can be different).
// If the Opt<T> is None, the mapping function is not called and None is returned.
// In this case the mapping is Opt<string> => Opt<int>.
Some("John")
    .Map(s => s.Length)
    .Should().Be(Some(4));

Some(2)
    .Map(i => i * 2)
    .Should().Be(Some(4));

None.Of<int>()
    .Map(i => i * 2)
    .Should().Be(None);

```
