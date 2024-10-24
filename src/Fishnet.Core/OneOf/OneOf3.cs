// // ReSharper disable InconsistentNaming
// // ReSharper disable CheckNamespace
// namespace Fishnet.Core;
// using static Prelude;
//
// public readonly record struct One<T1, T2, T3>
// {
//     public Opt<T1> Opt1 { get; }
//     public Opt<T2> Opt2 { get; }
//     public Opt<T3> Opt3 { get; }
//     internal Variant Index { get; }
//     internal enum Variant { T1, T2, T3 }
//
//     internal T1 ValT1 => Opt1.GetOrDie();
//     internal T2 ValT2 => Opt2.GetOrDie();
//     internal T3 ValT3 => Opt3.GetOrDie();
//
//     /// <summary>Used in switch expressions. Match against Some&lt;T&gt; to get the value.</summary>
//     public ISome Some => Index switch
//     {
//         Variant.T1 => new Some<T1>(ValT1),
//         Variant.T2 => new Some<T2>(ValT2),
//         Variant.T3 => new Some<T3>(ValT3),
//         _ => throw new FunctionalStateException($"{nameof(Index)} out of range")
//     };
//
//     public bool IsT1 => Index == Variant.T1;
//     public bool IsT2 => Index == Variant.T2;
//     public bool IsT3 => Index == Variant.T3;
//
//     public void Deconstruct(out Opt<T1> t1, out Opt<T2> t2, out Opt<T3> t3) => (t1, t2, t3) = (Opt1, Opt2, Opt3);
//
//     public One(T1 value) => (Opt1, Index) = (value, Variant.T1);
//     public One(T2 value) => (Opt2, Index) = (value, Variant.T2);
//     public One(T3 value) => (Opt3, Index) = (value, Variant.T3);
//
//     public static implicit operator One<T1, T2, T3>(T1 value) => new(value);
//     public static implicit operator One<T1, T2, T3>(T2 value) => new(value);
//     public static implicit operator One<T1, T2, T3>(T3 value) => new(value);
// }
//
// public static partial class OneExt
// {
//     public static TR Match<T1, T2, T3, TR>(
//         this One<T1, T2, T3> one,
//         Func<T1, TR> t1Func,
//         Func<T2, TR> t2Func,
//         Func<T3, TR> t3Func
//     ) =>
//         one switch
//         {
//             { Index: One<T1, T2, T3>.Variant.T1 } => t1Func(one.ValT1),
//             { Index: One<T1, T2, T3>.Variant.T2 } => t2Func(one.ValT2),
//             { Index: One<T1, T2, T3>.Variant.T3 } => t3Func(one.ValT3),
//             _ => throw new FunctionalStateException($"{nameof(Index)} out of range")
//         };
//
//     /// <summary>
//     /// Projects a One&lt;T1, T2, T3&gt; to a One&lt;R1, R2, R3&gt; using the provided mapping functions.
//     /// </summary>
//     public static One<T1R, T2R, T3R> Map<T1, T2, T3, T1R, T2R, T3R>(
//         this One<T1, T2, T3> one,
//         Func<T1, T1R> t1Func,
//         Func<T2, T2R> t2Func,
//         Func<T3, T3R> t3Func
//     ) =>
//         one.Match(
//             t1 => new One<T1R, T2R, T3R>(t1Func(t1)),
//             t2 => new One<T1R, T2R, T3R>(t2Func(t2)),
//             t3 => new One<T1R, T2R, T3R>(t3Func(t3)));
//
//     /// <summary>
//     /// Possibly projects a One&lt;T1, T2&gt; to an Opt&lt;One&lt;R1, R2&gt;&gt; using the optionally provided mapping functions.
//     /// </summary>
//     public static Opt<One<R1, R2, R3>> Map<T1, T2, T3, R1, R2, R3>(
//         this One<T1, T2, T3> one,
//         Opt<Func<T1, R1>> t1Func,
//         Opt<Func<T2, R2>> t2Func,
//         Opt<Func<T3, R3>> t3Func
//     ) =>
//         one.Match<T1, T2, T3, Opt<One<R1, R2, R3>>>(
//             t1 => t1Func
//                 .Map(f => f(t1))
//                 .Match(
//                     none: () => Opt<One<R1, R2, R3>>.None,
//                     some: s => Some(new One<R1, R2, R3>(s))),
//
//             t2 => t2Func
//                 .Map(f => f(t2))
//                 .Match(
//                     none: () => Opt<One<R1, R2, R3>>.None,
//                     some: s => Some(new One<R1, R2, R3>(s))),
//
//             t3 => t3Func
//                 .Map(f => f(t3))
//                 .Match(
//                     none: () => Opt<One<R1, R2, R3>>.None,
//                     some: s => Some(new One<R1, R2, R3>(s)))
//             );
//
//     /// <summary>
//     /// Possibly projects a <c>One&lt;T1, T2&gt;</c> to an <c>Opt&lt;One&lt;R1, T2&gt;&gt;</c> using the T1->R1 mapping function.
//     /// </summary>
//     /// <returns><c>None</c> if <c>one != T1</c></returns>
//     public static Opt<One<R1, T2, T3>> Map<T1, T2, T3, R1>(
//         this One<T1, T2, T3> one,
//         Func<T1, R1> t1Func
//     ) =>
//         one.IsT1
//             ? Some(new One<R1, T2, T3>(t1Func(one.ValT1)))
//             : Opt<One<R1, T2, T3>>.None;
//
//     /// <summary>
//     /// Possibly projects a <c>One&lt;T1, T2&gt;</c> to an <c>Opt&lt;One&lt;T1, R2&gt;&gt;</c> using the T2->R2 mapping function.
//     /// </summary>
//     /// <returns><c>None</c> if <c>one != T2</c></returns>
//     public static Opt<One<T1, R2, T3>> Map<T1, T2, T3, R2>(
//         this One<T1, T2, T3> one,
//         Func<T2, R2> t2Func
//     ) =>
//         one.Match<T1, T2, T3, Opt<One<T1, R2, T3>>>(
//             _ => Opt<One<T1, R2, T3>>.None,
//             t2 =>  Some(new One<T1, R2, T3>(t2Func(t2))),
//             _ => Opt<One<T1, R2, T3>>.None);
// }
