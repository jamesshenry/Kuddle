using Kuddle.AST;

namespace Kuddle.Tests;

public class KdlNodeTests
{
    public async Task KdlNode_CanBeInstantiatedWithName()
    {
        var sut = new KdlNode("A");
    }

    public async Task KdlNode_CanBeInstantiatedWithNameAndType()
    {
        var sut = new KdlNode("1.0.1", "semver");
    }
}
