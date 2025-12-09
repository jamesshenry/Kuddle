using Kuddle.AST;

namespace Kuddle.Extensions;

public static class KdlNodeExtensions
{
    extension(KdlNode node)
    {
        // --- Core Navigation ---

        /// <summary>
        /// Gets the property value if it exists, otherwise KdlNull.
        /// </summary>
        public KdlValue Prop(string key)
        {
            for (int i = node.Entries.Count - 1; i >= 0; i--)
            {
                if (node.Entries[i] is KdlProperty prop && prop.Key.Value == key)
                    return prop.Value;
            }
            return KdlValue.Null;
        }

        public KdlValue Arg(int index)
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
            return KdlValue.Null;
        }

        // --- The "TryGet" Navigation Pattern ---

        // Allows: if (node.TryGetProp("port", out int port)) { ... }

        public bool TryGetProp<T>(string key, out T result)
        {
            result = default!;
            var val = node.Prop(key);

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
