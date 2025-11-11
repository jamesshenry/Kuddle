using System;

namespace Kuddle.Extensions;

public static class SpanExtensions
{
    public static bool Any<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        foreach (var item in span)
        {
            if (predicate(item))
                return true;
        }
        return false;
    }
}
