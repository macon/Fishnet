using FluentAssertions;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests.OneTests;

using Contact = One<Name, Email>;
using ContactExt = One<Name, Email, Phone>;

public record Name(string FirstName, string LastName);
public record Email(string UserEmail);
public record Phone(string PhoneNo);

public class OneTests
{
    [Fact]
    public void MatchTests()
    {
        var contact = new Contact(new Name("John", "Doe"));

        var result = contact.Match(
            name => $"Name: {name.FirstName} {name.LastName}",
            email => $"Email: {email.UserEmail}");

        result.Should().Be("Name: John Doe");
    }

    [Fact]
    public void OneToOptTests()
    {
        var contact = new Contact(new Email("john.doe@nowhere.com"));

        contact.Opt1.Map(name => $"Name: {name.FirstName} {name.LastName}")
            .Should().BeOfType<Opt<string>>().Which
            .Should().Be(None);

        contact.Opt2.Map(email => $"Email: {email.UserEmail}")
            .Should().BeOfType<Opt<string>>()
            .Which.Should().Be(Some("Email: john.doe@nowhere.com"));
    }

    [Fact]
    public void MatchOf3Tests()
    {
        var contact = new ContactExt(new Phone("0123 7652 521"));

        var result = contact.Match(
            name => $"Name: {name.FirstName} {name.LastName}",
            email => $"Email: {email.UserEmail}",
            phone => $"Phone: {phone.PhoneNo}");

        result.Should().Be("Phone: 0123 7652 521");
    }

    [Fact]
    public void MapOptTests()
    {
        var contact = new Contact(new Name("John", "Doe"));

        var result = contact.Map(
            Some((Name name) => $"Name: {name.FirstName} {name.LastName}"),
            Some((Email _) => 1));

        var expected = Some(new One<string, int>("Name: John Doe"));
        result.IsSome.Should().BeTrue();
        result.Should().BeOfType<Opt<One<string, int>>>().Which.Should().Be(expected);
    }

    [Fact]
    public void MapTests()
    {
        var contact = new Contact(new Name("John", "Doe"));

        var result = contact.Map(
            t1Func: name => $"Name: {name.FirstName} {name.LastName}",
            t2Func: _ => 1);

        var expected = new One<string, int>("Name: John Doe");
        result.Should().BeOfType<One<string, int>>().Which.Should().Be(expected);
    }

    [Fact]
    public void MapCaseTests()
    {
        var contact = new Contact(new Name("John", "Doe"));

        var result = contact.Map(
            t1Func: name => $"Name: {name.FirstName} {name.LastName}"
            );

        var expected = Some(new One<string, Email>("Name: John Doe"));
        result.Should().BeOfType<Opt<One<string, Email>>>().Which.Should().Be(expected);
    }

    [Fact]
    public void MapNoneCaseTests()
    {
        var result = new Contact(new Email("JohnDoe@nowhere.com"))
            .Map(name => $"Name: {name.FirstName} {name.LastName}");

        result.Should().BeOfType<Opt<One<string, Email>>>().Which.Should().Be(None);
    }

    public class SwitchTests
    {
        [Fact]
        public void SwitchMatch() =>
            // The .Some property returns the variant wrapped in an ISome.
            // This allows you to use native switch expressions.
            (new Contact(new Name("Bob", "Moore")).Some switch
            {
                Some<Name> { Value: var n } => $"Name: {n.FirstName} {n.LastName}",
                Some<Email> e => $"Email: {e.Value}",
                _ => throw new ArgumentOutOfRangeException()
            })
            .Should().Be("Name: Bob Moore");
    }

    [Fact]
    public void DeconstructTests()
    {
        var result = new Contact(new Name("Bob", "Moore")) switch
        {
            ({ IsSome: true } name, _) => $"Name: {name.GetOrDie().FirstName} {name.GetOrDie().LastName}",
            (_, { IsSome: true } email) => $"Email: {email.GetOrDie()}",
            _ => throw new ArgumentOutOfRangeException()
        };

        result.Should().Be("Name: Bob Moore");
    }
}
