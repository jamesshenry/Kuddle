using System;
using System.Text;

namespace Kuddle.Extensions;

public static class SpanExtensions
{
    extension<T>(ReadOnlySpan<T> span)
    {
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

    extension(string input)
    {
        public string ToKebabCase()
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder result = new();

            bool previousCharacterIsSeparator = true;

            for (int i = 0; i < input.Length; i++)
            {
                char currentChar = input[i];

                if (char.IsUpper(currentChar) || char.IsDigit(currentChar))
                {
                    if (
                        !previousCharacterIsSeparator
                        && (
                            i > 0
                            && (
                                char.IsLower(input[i - 1])
                                || (i < input.Length - 1 && char.IsLower(input[i + 1]))
                            )
                        )
                    )
                    {
                        result.Append('-');
                    }

                    result.Append(char.ToLowerInvariant(currentChar));

                    previousCharacterIsSeparator = false;
                }
                else if (char.IsLower(currentChar))
                {
                    result.Append(currentChar);

                    previousCharacterIsSeparator = false;
                }
                else if (currentChar == ' ' || currentChar == '_' || currentChar == '-')
                {
                    if (!previousCharacterIsSeparator)
                    {
                        result.Append('-');
                    }

                    previousCharacterIsSeparator = true;
                }
            }

            return result.ToString();
        }
    }
}
