using System;
using System.Diagnostics.CodeAnalysis;
using Kuddle.AST;

namespace Kuddle.Extensions;

public static class KuddleValueExtensions
{
    extension(KdlValue value)
    {
        public bool IsNull => value is KdlNull;
        public bool IsNumber => value is KdlNumber;
        public bool IsString => value is KdlString;
        public bool IsBool => value is KdlBool;

        public bool TryGetInt(out int result)
        {
            result = 0;
            if (value is KdlNumber num)
            {
                try
                {
                    result = num.ToInt32();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public bool TryGetLong(out long result)
        {
            result = 0;
            if (value is KdlNumber num)
            {
                try
                {
                    result = num.ToInt64();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        // --- Floats ---

        public bool TryGetDouble(out double result)
        {
            result = 0;
            if (value is KdlNumber num)
            {
                try
                {
                    result = num.ToDouble();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public bool TryGetDecimal(out decimal result)
        {
            result = 0;
            if (value is KdlNumber num)
            {
                try
                {
                    result = num.ToDecimal();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        // --- Booleans ---

        public bool TryGetBool(out bool result)
        {
            if (value is KdlBool b)
            {
                result = b.Value;
                return true;
            }
            result = false;
            return false;
        }

        // --- Strings ---

        public bool TryGetString([NotNullWhen(true)] out string? result)
        {
            if (value is KdlString s)
            {
                result = s.Value;
                return true;
            }
            result = null;
            return false;
        }

        // --- Complex Types (UUID, Date, IP) ---

        public bool TryGetUuid(out Guid result)
        {
            result = Guid.Empty;
            return value is KdlString s && Guid.TryParse(s.Value, out result);
        }

        public bool TryGetDateTime(out DateTimeOffset result)
        {
            result = default;
            return value is KdlString s && DateTimeOffset.TryParse(s.Value, out result);
        }
    }
}
