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

        public int MaxConsecutive(T target)
        {
            int max = 0;
            while (true)
            {
                int start = span.IndexOf(target);
                if (start < 0)
                    break;

                span = span.Slice(start);

                int length = span.IndexOfAnyExcept(target);
                if (length < 0)
                    length = span.Length;

                if (length > max)
                    max = length;

                if (length == span.Length)
                    break;
                span = span.Slice(length);
            }
            return max;
        }
    }
}
