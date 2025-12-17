using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Kuddle.AST;
using Kuddle.Parser;
using Parlot.Fluent;

namespace Kuddle.Benchmarks;

[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class ParserBenchmarks
{
    private string _simpleDocument = string.Empty;
    private string _complexDocument = string.Empty;
    private string _largeDocument = string.Empty;

    private Parser<KdlDocument> _compiledParser = null!;
    private Parser<KdlDocument> _nonCompiledParser = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleDocument = """
            node1 "value1"
            node2 123
            node3 #true
            """;

        _complexDocument = """
            package {
                name "my-package"
                version "1.0.0"
                dependencies {
                    dep1 "^2.0.0"
                    dep2 "~1.5.0"
                }
            }

            config (type)"production" {
                host "example.com"
                port 8080
                ssl #true
                features enabled=#true timeout=30
            }

            users {
                user id=1 name="Alice" active=#true
                user id=2 name="Bob" active=#false
                user id=3 name="Charlie" active=#true
            }
            """;

        var largeDocBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            largeDocBuilder.AppendLine($"node{i} {{");
            for (int j = 0; j < 10; j++)
            {
                largeDocBuilder.AppendLine($"    child{j} \"value{j}\" prop{j}={j}");
            }
            largeDocBuilder.AppendLine("}");
        }
        _largeDocument = largeDocBuilder.ToString();

        _compiledParser = KdlGrammar.Document.Compile();
        _nonCompiledParser = KdlGrammar.Document;
    }

    [Benchmark]
    public KdlDocument? SimpleDocument_NonCompiled()
    {
        return _nonCompiledParser.Parse(_simpleDocument);
    }

    [Benchmark]
    public KdlDocument? SimpleDocument_Compiled()
    {
        return _compiledParser.Parse(_simpleDocument);
    }

    [Benchmark]
    public KdlDocument? ComplexDocument_NonCompiled()
    {
        return _nonCompiledParser.Parse(_complexDocument);
    }

    [Benchmark]
    public KdlDocument? ComplexDocument_Compiled()
    {
        return _compiledParser.Parse(_complexDocument);
    }

    [Benchmark]
    public KdlDocument? LargeDocument_NonCompiled()
    {
        return _nonCompiledParser.Parse(_largeDocument);
    }

    [Benchmark]
    public KdlDocument? LargeDocument_Compiled()
    {
        return _compiledParser.Parse(_largeDocument);
    }
}
