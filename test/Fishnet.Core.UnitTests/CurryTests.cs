using FluentAssertions;

namespace Fishnet.Core.UnitTests;

public class CurryTests
{
    /*
     * Currying is the technique of translating a function that takes multiple
     * arguments into a sequence of functions, each with a single argument.
     *
     * Why? It's the basis of function composition (partial application.)
     *
     * Remember FP works at the function level, not the class level.
     * So instead of having a class constructor that takes an abstract type (DI),
     * we have a curried function that takes an abstract param which can be applied later.
     *
     * e.g.
     * 
     * class PaymentRepository(IDatabaseProvider db) { ... }
     * 
     * becomes:
     * 
     * Func<IDatabaseProvider, Payment, bool> savePayment = (db, p) => db.Store(p);
     * var SavePaymentPg = SavePayment.Curry()(new PostgresDb());
     * var result = SavePaymentPg(new Payment(100));
     */

    [Fact]
    public void BasicCurry()
    {
        var multiply = (int x, int y) => x * y;     // -> Func<int, int, int>

        var curriedMultiply = multiply.Curry();     // -> Func<int, Func<int, int>>

        var multiplyBy2 = curriedMultiply(2);       // -> Func<int, int> = (int y) => 2 * y

        multiplyBy2(3)
            .Should().Be(6);
    }
}
