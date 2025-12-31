using Kuddle.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Kuddle.Extensions.Configuration;

public static class KdlConfigurationExtensions
{
    extension(IConfigurationBuilder builder)
    {
        public IConfigurationBuilder AddKdlFile(
            string path,
            IFileProvider? provider = null,
            bool optional = false,
            bool reloadOnChange = false,
            KdlSerializerOptions? serializerOptions = null
        )
        {
            return builder.AddKdlFile(s =>
            {
                s.Path = path;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.SerializerOptions = serializerOptions;

                s.ResolveFileProvider();
            });
        }

        public IConfigurationBuilder AddKdlFile(Action<KdlConfigurationSource> configureSource)
        {
            var source = new KdlConfigurationSource();
            configureSource(source);
            return builder.Add(source);
        }
    }
}
