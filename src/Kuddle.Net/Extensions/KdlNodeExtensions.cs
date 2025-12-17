using Kuddle.AST;

namespace Kuddle.Extensions;

public static class KdlNodeExtensions
{
    extension(KdlNode node)
    {
        public KdlValue? Prop(string key)
        {
            for (int i = node.Entries.Count - 1; i >= 0; i--)
            {
                if (node.Entries[i] is KdlProperty prop && prop.Key.Value == key)
                    return prop.Value;
            }
            return null;
        }

        public KdlValue? Arg(int index)
        {
            int count = 0;
            foreach (var entry in node.Entries)
            {
                if (entry is KdlArgument arg)
                {
                    if (count == index)
                        return arg.Value;
                    count++;
                }
            }
            return null;
        }

        public bool TryGetProp<T>(string key, out T result)
        {
            result = default!;
            var val = node.Prop(key);
            if (val is null)
            {
                return false;
            }

            if (typeof(T) == typeof(int) && val.TryGetInt(out int i))
            {
                result = (T)(object)i;
                return true;
            }
            if (typeof(T) == typeof(bool) && val.TryGetBool(out bool b))
            {
                result = (T)(object)b;
                return true;
            }
            if (typeof(T) == typeof(string) && val.TryGetString(out string? s))
            {
                result = (T)(object)s;
                return true;
            }
            // ... add other types

            return false;
        }
    }
}
