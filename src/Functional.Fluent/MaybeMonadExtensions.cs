﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Functional.Fluent
{
    public static class MaybeMonadExtensions
    {

        public static Maybe<TResult> Select<T, TResult>(this Maybe<T> o, Func<T, TResult> evaluator)
        {
            return With(o, evaluator);
        }

        public static Maybe<TResult> SelectMany<T, T2, TResult>(this Maybe<T> a, Func<T, Maybe<T2>> func, Func<T, T2, TResult> select)
        {
            if (a == null || !a.HasValue) return Maybe<TResult>.Nothing;
            var f = func(a.Value);
            if (f == null || !f.HasValue) return Maybe<TResult>.Nothing;
            return select(a.Value, f.Value).ToMaybe();
        }

        public static Maybe<TResult> SelectMany<T, T2, TResult>(this Maybe<T> a, Func<T, T2> func, Func<T, T2, TResult> select)
        {
            if (a == null || !a.HasValue) return Maybe<TResult>.Nothing;
            var f = func(a.Value);
            if (f == null) return Maybe<TResult>.Nothing;
            return select(a.Value, f).ToMaybe();
        }


        public static Maybe<TResult> With<TInput, TResult>(this Maybe<TInput> o, Func<TInput, TResult> evaluator)
        {
            if (o == null || !o.HasValue) return Maybe<TResult>.Nothing;
            return new Maybe<TResult>(evaluator(o.Value));
        }

        public static MaybeEnumerable<TResult> With<TInput, TResult>(this MaybeEnumerable<TInput> o, Func<TInput, TResult> evaluator)
        {
            if (o == null || !o.HasValue) return MaybeEnumerable<TResult>.Empty;
            return new MaybeEnumerableApplicator<TInput, TResult>(o, evaluator);
        }

        public static Maybe<TResult> Return<TInput, TResult>(this Maybe<TInput> o,
            Func<TInput, TResult> evaluator, TResult failureValue)
        {
            if (o == null || !o.HasValue) return new Maybe<TResult>(failureValue);
            return new Maybe<TResult>(evaluator(o.Value));
        }

        public static MaybeEnumerable<TResult> Return<TInput, TResult>(this MaybeEnumerable<TInput> o,
            Func<TInput, TResult> evaluator, TResult failureValue)
        {
            if (o == null || !o.HasValue) return MaybeEnumerable<TResult>.Empty;
            return new MaybeEnumerableApplicator<TInput, TResult>(o, evaluator, () => failureValue);
        }

        public static Maybe<TResult> Return<TInput, TResult>(this Maybe<TInput> o,
            Func<TInput, TResult> evaluator, Func<TResult> failureValue)
        {
            if (o == null || !o.HasValue) return new Maybe<TResult>(failureValue());
            return new Maybe<TResult>(evaluator(o.Value));
        }

        public static MaybeEnumerable<TResult> Return<TInput, TResult>(this MaybeEnumerable<TInput> o,
            Func<TInput, TResult> evaluator, Func<TResult> failureValue)
        {
            if (o == null || !o.HasValue) return MaybeEnumerable<TResult>.Empty;
            return new MaybeEnumerableApplicator<TInput, TResult>(o, evaluator, failureValue);
        }

        public static Maybe<TInput> If<TInput>(this Maybe<TInput> o, Func<TInput, bool> evaluator)
        {
            if (o == null || !o.HasValue) return Maybe<TInput>.Nothing;
            return evaluator(o.Value) ? o : Maybe<TInput>.Nothing;
        }

        public static Maybe<TInput> Unless<TInput>(this Maybe<TInput> o, Func<TInput, bool> evaluator)
        {
            if (o == null || !o.HasValue) return Maybe<TInput>.Nothing;
            return evaluator(o.Value) ? Maybe<TInput>.Nothing : o;
        }

        public static Maybe<TInput> Do<TInput>(this Maybe<TInput> o, Action<TInput> action)
        {
            if (o == null || !o.HasValue) return Maybe<TInput>.Nothing;
            action(o.Value);
            return o;
        }

        public static MaybeEnumerable<TInput> Do<TInput>(this MaybeEnumerable<TInput> o, Action<TInput> action)
        {
            if (o == null || !o.HasValue) return MaybeEnumerable<TInput>.Empty;
            foreach (var e in o.Where(e => e.HasValue))
                action(e.Value);
            return o;
        }

        public static Maybe<TInput> Do<TInput>(this Maybe<TInput> o, params Action<TInput>[] actions)
        {
            if (o == null || !o.HasValue) return Maybe<TInput>.Nothing;
            foreach (var action in actions)
                action(o.Value);
            return o;
        }

        public static MaybeEnumerable<TInput> Do<TInput>(this MaybeEnumerable<TInput> o, params Action<TInput>[] actions)
        {
            if (o == null || !o.HasValue) return MaybeEnumerable<TInput>.Empty;
            foreach (var action in actions)
                foreach (var e in o.Where(e => e.HasValue))
                    action(e.Value);
            return o;
        }

        public static Maybe<TInput> ApplyIf<TInput>(this Maybe<TInput> o, Func<TInput, bool> evaluator, Func<TInput, TInput> action)
        {
            if (o == null || !o.HasValue) return Maybe<TInput>.Nothing;
            return evaluator(o.Value) ? new Maybe<TInput>(action(o.Value)) : o;
        }

        public static MaybeEnumerable<TInput> ApplyIf<TInput>(this MaybeEnumerable<TInput> o, Func<TInput, bool> evaluator, Func<TInput, TInput> action)
        {
            if (o == null || !o.HasValue) return MaybeEnumerable<TInput>.Empty;
            return new MaybeEnumerableConditionalApplicator<TInput>(o, evaluator, action);
        }

        public static Maybe<TInput> ApplyUnless<TInput>(this Maybe<TInput> o, Func<TInput, bool> evaluator, Func<TInput, TInput> action)
        {
            if (o == null || !o.HasValue) return Maybe<TInput>.Nothing;
            return evaluator(o.Value) ? o : new Maybe<TInput>(action(o.Value));
        }

        public static MaybeEnumerable<TInput> ApplyUnless<TInput>(this MaybeEnumerable<TInput> o, Func<TInput, bool> evaluator, Func<TInput, TInput> action)
        {
            if (o == null || !o.HasValue) return MaybeEnumerable<TInput>.Empty;
            return new MaybeEnumerableConditionalApplicator<TInput>(o, evaluator, action, true);
        }

        public static Maybe<TInput> IsNull<TInput>(this Maybe<TInput> o, Func<TInput> func)
        {
            if (o == null || !o.HasValue) return new Maybe<TInput>(func());
            return o;
        }

        public static Maybe<TInput> IsNull<TInput>(this Maybe<TInput> o, TInput defaultValue)
        {
            if (o == null || !o.HasValue) return new Maybe<TInput>(defaultValue);
            return o;
        }

        public static Maybe<TOutput> SelectOne<TInput, TOutput>(this Maybe<TInput> o, params Func<Maybe<TInput>, Maybe<TOutput>>[] selectors)
        {
            foreach (var selector in selectors)
            {
                var result = selector(o);
                if (result.HasValue)
                    return result;
            }
            return Maybe<TOutput>.Nothing;
        }

        public static IEnumerable<Maybe<T>> Lift<T>(this Maybe<IEnumerable<T>> v)
        {
            return !v.HasValue ? Enumerable.Empty<Maybe<T>>() : v.Value.Select(z => z.ToMaybe());
        }

        public static IEnumerable<Maybe<T>> Lift<T>(this Maybe<IEnumerable<Maybe<T>>> v)
        {
            return !v.HasValue ? Enumerable.Empty<Maybe<T>>() : v.Value;
        }
        
        public static Maybe<T> ToMaybe<T>(this Maybe<T> value)
        {
            return value;
        }

        public static Maybe<T> ToMaybe<T>(this T value)
        {
            return new Maybe<T>(value);
        }

        public static MaybeEnumerable<T> ToMaybe<T>(this IEnumerable<T> value)
        {
            return new MaybeEnumerable<T>(value);
        }

        public static MaybeEnumerable<T> ToMaybe<T>(this IEnumerable<Maybe<T>> value)
        {
            return new MaybeEnumerable<T>(value);
        }
     
        public static Maybe<T> ToMaybe<T>(this T value, T defaultValue)
        {
            var m = new Maybe<T>(value);
            return m.IsNull(defaultValue);
        }

        public static Maybe<T> ToMaybe<T>(this T value, Func<T> defaultValue)
        {
            var m = new Maybe<T>(value);
            return m.IsNull(defaultValue);
        }

        public static Maybe<IEnumerable<T>> ToMaybeNonEmpty<T>(this IEnumerable<T> value)
        {
            if (value == null || !value.Any()) return Maybe<IEnumerable<T>>.Nothing;
            return new Maybe<IEnumerable<T>>(value);
        }

        public static Maybe<TU> Map<TV, TU>(this Maybe<TV> v, Func<Maybe<TV>, TU> mapFunc)
        {
            return mapFunc(v).ToMaybe();
        }
        
        public static ListMatcherContext<TV> Match<TV>(this MaybeEnumerable<TV> v)
        {
            return new ListMatcherContext<TV>(v.Value.Select(x => x.Value).ToMaybeNonEmpty());
        }        
    }
}
