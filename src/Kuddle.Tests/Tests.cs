namespace Kuddle.Tests;

public class Tests
{
    [Test]
    public async Task Fluent()
    {
        var parser = FluentParser.Expression;
        var expression = parser.Parse("1 + 1 / 3 * 4");

        await Assert.That(expression).IsNotNull();
    }

    [Test]
    public async Task Basic()
    {
        var parser = KdlParser.V2();

        var result = parser.Parse("node deadbeef");

        await Assert.That(result).IsNotNull();
    }

    [Test]
    [Arguments(1, 2, 3)]
    [Arguments(2, 3, 5)]
    public async Task DataDrivenArguments(int a, int b, int c)
    {
        Console.WriteLine("This one can accept arguments from an attribute");

        var result = a + b;

        await Assert.That(result).IsEqualTo(c);
    }

    [Test]
    [MethodDataSource(nameof(DataSource))]
    public async Task MethodDataSource(int a, int b, int c)
    {
        Console.WriteLine("This one can accept arguments from a method");

        var result = a + b;

        await Assert.That(result).IsEqualTo(c);
    }

    public static IEnumerable<(int a, int b, int c)> DataSource()
    {
        yield return (1, 1, 2);
        yield return (2, 1, 3);
        yield return (3, 1, 4);
    }
}
