using Microsoft.Extensions.Configuration;

namespace Kuddle.Extensions.Configuration;

internal class KdlConfigurationProvider : FileConfigurationProvider
{
    private readonly KdlConfigurationSource _source;

    public KdlConfigurationProvider(KdlConfigurationSource source)
        : base(source)
    {
        _source = source;
    }

    public override void Load(Stream stream)
    {
        try
        {
            Data = KdlConfigurationFileParser.Parse(stream, _source.SerializerOptions);
        }
        catch (Exception ex)
        {
            throw new FormatException("kdl parse failed", ex);
        }
    }
}
