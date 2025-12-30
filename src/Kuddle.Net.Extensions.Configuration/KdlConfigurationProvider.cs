using Microsoft.Extensions.Configuration;

namespace Kuddle.Extensions.Configuration;

internal class KdlConfigurationProvider : FileConfigurationProvider
{
    public KdlConfigurationProvider(FileConfigurationSource source)
        : base(source) { }

    public override void Load(Stream stream)
    {
        try
        {
            Data = KdlConfigurationFileParser.Parse(stream);
        }
        catch (Exception ex)
        {
            throw new FormatException("kdl parse failed", ex);
        }
    }
}

internal sealed class KdlConfigurationFileParser
{
    internal static IDictionary<string, string?> Parse(Stream stream)
    {
        throw new NotImplementedException();
    }
}
