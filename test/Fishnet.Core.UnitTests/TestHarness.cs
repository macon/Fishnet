using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests;
using static TestUtil;

public static class ProviderPayments
{
    public static IReadOnlyDictionary<string, IEnumerable<Transaction>> PaymentsStore => new ReadOnlyDictionary<string, IEnumerable<Transaction>>(_paymentStore);

    private static IDictionary<string, IEnumerable<Transaction>> _paymentStore { get; }

    static ProviderPayments()
    {
        _paymentStore = new Dictionary<string, IEnumerable<Transaction>>
        {
            [Providers.Barclays] = new[]
            {
                new Transaction(JohnDoe, DateTime.UtcNow, new AccountId.AccountAndSort("87234019", "12-34-56"), 10000),
                new Transaction(JohnDoe, RandomDate(), new AccountId.AccountAndSort("51230982", "12-34-56"), RandomAmountMinor()),
                new Transaction(JohnDoe, RandomDate(), new AccountId.AccountAndSort("67290812", "12-34-56"), RandomAmountMinor()),
                new Transaction(KarenBrown, DateTime.UtcNow, new AccountId.AccountAndSort("87234019", "12-34-56"), 10000),
                new Transaction(KarenBrown, RandomDate(), new AccountId.AccountAndSort("51230982", "12-34-56"), RandomAmountMinor()),
                new Transaction(KarenBrown, RandomDate(), new AccountId.AccountAndSort("67290812", "12-34-56"), RandomAmountMinor()),
                new Transaction(FredSmith, DateTime.UtcNow, new AccountId.AccountAndSort("87234019", "12-34-56"), 10000),
                new Transaction(FredSmith, RandomDate(), new AccountId.AccountAndSort("51230982", "12-34-56"), RandomAmountMinor()),
                new Transaction(FredSmith, RandomDate(), new AccountId.AccountAndSort("67290812", "12-34-56"), RandomAmountMinor())
            }
        };
    }

    public static readonly Psu JohnDoe = new("John Doe");
    public static readonly Psu FredSmith = new("Fred Smith");
    public static readonly Psu KarenBrown = new("Karen Brown");
}

public static class TestUtil
{
    private static Random _random = new();

    public static DateTime RandomDate()
        => DateTime
            .Parse($"2024-{_random.Next(1, DateTime.Now.Month)}-{_random.Next(1, 30)}T12:12:09Z");

    public static uint RandomAmountMinor()
        => (uint)_random.Next(10000, 200000);
}

public static class Providers
{
    public const string Barclays = "ob-barclays";
    public const string Lloyds = "ob-lloyds";
    public const string Natwest = "ob-natwest";

    public static readonly ProviderType BarclaysBank = new(Barclays);
    public static readonly ProviderType LloydsBank = new(Lloyds);
    public static readonly ProviderType NatwestBank = new(Natwest);

    public static readonly IReadOnlyDictionary<string, ProviderType> ProviderMap = new Dictionary<string, ProviderType>
    {
        [Barclays] = BarclaysBank,
        [Lloyds] = LloydsBank,
        [Natwest] = NatwestBank
    };
}

public class TestHarness
{
    private readonly IDictionary<string, Psu> _psus = new Dictionary<string, Psu>
    {
        ["John Doe"] = JohnDoe,
        ["John Smith"] = FredSmith,
        ["Karen Brown"] = KarenBrown,
    };

    public static readonly ProviderType BarclaysBank = new("ob-barclays");
    public static readonly ProviderType LloydsBank = new("ob-lloyds");
    public static readonly ProviderType NatwestBank = new("ob-natwest");

    public static readonly Psu JohnDoe = new("John Doe");
    public static readonly Psu FredSmith = new("Fred Smith");
    public static readonly Psu KarenBrown = new("Karen Brown");

    public static readonly PsuProvider JohnDoeBarclays = new(JohnDoe, BarclaysBank, new AccountId.AccountAndSort("12345678", "20-23-97"));
    public static readonly PsuProvider FredSmithNatwest = new(FredSmith, NatwestBank, new AccountId.AccountAndSort("87120472", "14-90-87"));
    public static readonly PsuProvider KarenBrownLloyds = new(KarenBrown, LloydsBank, new AccountId.AccountAndSort("98124356", "98-31-71"));

    public static readonly PsuTransactions JohnDoeTransactions =
        new(JohnDoe, new[]
        {
            new Transaction(JohnDoe, DateTime.Now, new AccountId.AccountAndSort("87234019", "12-34-56"), 1000),
            new Transaction(JohnDoe, DateTime.Now, new AccountId.AccountAndSort("51230982", "12-34-56"), 1500),
            new Transaction(JohnDoe, DateTime.Now, new AccountId.AccountAndSort("67290812", "12-34-56"), 50000)
        });

    public static readonly PsuTransactions FredSmithTransactions =
        new(FredSmith, new[]
        {
            new Transaction(FredSmith, DateTime.Now, new AccountId.AccountAndSort("87234019", "12-34-56"), 2000),
            new Transaction(FredSmith, DateTime.Now, new AccountId.AccountAndSort("51230982", "12-34-56"), 3500),
            new Transaction(FredSmith, DateTime.Now, new AccountId.AccountAndSort("67290812", "12-34-56"), 60000)
        });

    public static readonly PsuTransactions KarenBrownTransactions =
        new(KarenBrown, new[]
        {
            new Transaction(KarenBrown, DateTime.Now, new AccountId.AccountAndSort("87234019", "12-34-56"), 2000),
            new Transaction(KarenBrown, DateTime.Now, new AccountId.AccountAndSort("51230982", "12-34-56"), 3500),
            new Transaction(KarenBrown, DateTime.Now, new AccountId.AccountAndSort("67290812", "12-34-56"), 60000)
        });

    public static IEnumerable<Transaction> _allTxs = new[]
    {
        JohnDoeTransactions.Transactions,
        FredSmithTransactions.Transactions,
        KarenBrownTransactions.Transactions
    }.SelectMany(x => x);


}

public record ProviderType(string Name);

public static class Provider
{
    public static Opt<ProviderType> GetProvider(string id)
        => id == Providers.Barclays
            ? Some(Providers.BarclaysBank)
            : None;
}

public record Psu(string Name);

public record Transaction(Psu Psu, DateTime TrxDate, AccountId BeneficiaryAccount, uint AmountInMinor);

public record PsuTransactions(Psu Psu, IEnumerable<Transaction> Transactions);

public record Transactions(IEnumerable<Transaction> TransactionList);

public record PsuProvider(Psu Psu, ProviderType Bank, AccountId RemitterAccount);


[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Allows for approximation of discriminated union type")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract record AccountId
{
    private AccountId() { }
    public sealed record AccountAndSort(string AccountNumber, string SortCode) : AccountId;
    public sealed record IBAN(string IBan) : AccountId;
    public sealed record BBAN(string BBan) : AccountId;
    public sealed record NRB(string Nrb) : AccountId;
}

