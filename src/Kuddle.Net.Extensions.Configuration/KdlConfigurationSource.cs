using Microsoft.Extensions.Configuration;

namespace Kuddle.Extensions.Configuration;

public class KdlConfigurationSource : FileConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new KdlConfigurationProvider(this);
    }
}
