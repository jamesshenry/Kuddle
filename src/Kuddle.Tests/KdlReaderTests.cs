namespace Kuddle.Tests;

public class KdlReaderTests
{
    private readonly string[] _testFiles;
    private readonly string[] _expectedFiles;

    public KdlReaderTests()
    {
        _testFiles = Directory.GetFiles("_test_cases\\input", "*");
        _expectedFiles = Directory.GetFiles("_test_cases\\expected_kdl", "*");
    }

    public async Task(T parameter) { }
}
