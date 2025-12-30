using Kuddle.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Kuddle.Extensions.Configuration;

public static class KuddleConfigurationExtensions
{
    extension(IConfigurationBuilder builder)
    {
        public IConfigurationBuilder AddKdlFile(string path)
        {
            throw new NotImplementedException();
        }

        public IConfigurationBuilder AddKdlFile(
            IFileProvider? provider,
            string path,
            bool optional,
            bool reloadOnChange,
            KdlSerializerOptions? serializerOptions
        )
        {
            throw new NotImplementedException();
        }
    }
}
