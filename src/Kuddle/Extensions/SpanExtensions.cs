using System;

namespace Kuddle.Extensions;

public static class SpanExtensions
{
    extension<T>(ReadOnlySpan<T> span)
    {
        public bool Any(Func<T, bool> predicate)
        {
            foreach (var item in span)
            {
                if (predicate(item))
                    return true;
            }
            return false;
        }
    }
}
