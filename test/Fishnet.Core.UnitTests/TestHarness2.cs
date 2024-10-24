using FluentAssertions;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests;

public class TestHarness2
{
    [Fact(Skip = "This test is for demonstration purposes only.")]
    public void Test1()
    {
        const string id = "ob-barclays";

        // Sum up the last 3 transactions for the specified bank.
        var x = ProviderRepo
            .GetProvider(id)
            .GetTransactions()
            .Map(txs => txs.OrderByDescending(tx => tx.TrxDate).Take(3).Sum(x => x.AmountInMinor));

        x.Should().Be(Success(30000L));
    }

    [Fact(Skip = "This test is for demonstration purposes only.")]
    public void Test2()
    {
        const string id = "ob-whoever";

        // Sum up the last 3 transactions for the specified bank.
        var x = ProviderRepo
            .GetProvider(id)
            .GetTransactions()
            .Map(txs => txs.OrderByDescending(tx => tx.TrxDate).Take(3).Sum(x => x.AmountInMinor));

        x.IsError.Should().BeTrue();
    }

    [Fact(Skip = "This test is for demonstration purposes only.")]
    public void Test3()
    {
        const string id = "ob-lloyds";

        // Sum up the last 3 transactions for the specified bank.
        var provider = ProviderRepo
            .GetProvider(id);

        var func = Try(() => provider.GetTransactionsAndMightThrow());
        var z = func.Run();
        var r = z.Map(x => x.Map(txs => txs.OrderByDescending(tx => tx.TrxDate).Take(3).Sum(txn => txn.AmountInMinor)));

        r.IsException.Should().BeTrue();
    }
}


public static class ProviderRepo
{
    public static Res<ProviderType> GetProvider(string id)
        => Providers.ProviderMap.TryGetValue(id, out var provider)
            ? Success(provider)
            : Error.New($"Unknown provider id: {id}");
}

public static class PaymentRepo
{
    public static Res<IEnumerable<Transaction>> GetTransactions(this Res<ProviderType> provider) =>
        provider.Match(
            error: e => e,
            suc: p =>
                ProviderPayments.PaymentsStore.TryGetValue(p.Name, out var transactions)
                    ? Success(transactions)
                    : Error<IEnumerable<Transaction>>($"No transactions for {p.Name}"));

    public static Res<IEnumerable<Transaction>> GetTransactionsAndMightThrow(this Res<ProviderType> provider) =>
        provider.Match<Res<IEnumerable<Transaction>>>(
            error: e => e,
            suc: p =>
                ProviderPayments.PaymentsStore.TryGetValue(p.Name, out var transactions)
                    ? Success(transactions)
                    : throw new ApplicationException("No transactions for {p.Name}"));
}
