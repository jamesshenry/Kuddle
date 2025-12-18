using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Kuddle.AST;
using Kuddle.Parser;
using Kuddle.Serialization;
using Parlot.Fluent;

namespace Kuddle.Benchmarks;

[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class SerializerBenchmarks
{
    private string _simpleDocument = string.Empty;
    private string _complexDocument = string.Empty;
    private string _largeDocument = string.Empty;

    private Parser<KdlDocument> _compiledParser = null!;
    private Parser<KdlDocument> _nonCompiledParser = null!;

    // Serialization benchmarks
    private Package _simplePackage = null!;
    private string _simplePackageKdl = string.Empty;

    private Project _complexProject = null!;
    private string _complexProjectKdl = string.Empty;

    private List<Package> _largePackageList = null!;
    private string _largePackageListKdl = string.Empty;

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

        // Setup serialization objects
        _simplePackage = new Package
        {
            Name = "my-lib",
            Version = "1.0.0",
            Description = "A library",
        };
        _simplePackageKdl = KdlSerializer.Serialize(_simplePackage);

        _complexProject = new Project
        {
            Name = "my-app",
            Version = "2.0.0",
            Dependencies =
            [
                new Dependency { Package = "lodash", Version = "4.17.21" },
                new Dependency { Package = "react", Version = "18.0.0" },
                new Dependency { Package = "typescript", Version = "4.5.0" },
            ],
            DevDependencies =
            [
                new Dependency { Package = "jest", Version = "27.0.0" },
                new Dependency { Package = "eslint", Version = "8.0.0" },
            ],
        };
        _complexProjectKdl = KdlSerializer.Serialize(_complexProject);

        _largePackageList = [];
        for (int i = 0; i < 100; i++)
        {
            _largePackageList.Add(
                new Package
                {
                    Name = $"package{i}",
                    Version = $"1.{i}.0",
                    Description = $"Description for package {i}",
                }
            );
        }
        _largePackageListKdl = KdlSerializer.SerializeMany(_largePackageList);
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

    // Serialization benchmarks
    [Benchmark]
    public string SerializeSimplePackage()
    {
        return KdlSerializer.Serialize(_simplePackage);
    }

    [Benchmark]
    public string SerializeComplexProject()
    {
        return KdlSerializer.Serialize(_complexProject);
    }

    [Benchmark]
    public string SerializeLargePackageList()
    {
        return KdlSerializer.SerializeMany(_largePackageList);
    }

    // Deserialization benchmarks
    [Benchmark]
    public Package? DeserializeSimplePackage()
    {
        return KdlSerializer.Deserialize<Package>(_simplePackageKdl);
    }

    [Benchmark]
    public Project? DeserializeComplexProject()
    {
        return KdlSerializer.Deserialize<Project>(_complexProjectKdl);
    }

    [Benchmark]
    public List<Package>? DeserializeLargePackageList()
    {
        return KdlSerializer.DeserializeMany<Package>(_largePackageListKdl).ToList();
    }

    // Test models
    public class Package
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string? Version { get; set; }

        [KdlProperty("description")]
        public string? Description { get; set; }
    }

    public class Project
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string Version { get; set; } = "1.0.0";

        [KdlNode("dependency")]
        public List<Dependency> Dependencies { get; set; } = [];

        [KdlNode("devDependency")]
        public List<Dependency> DevDependencies { get; set; } = [];
    }

    public class Dependency
    {
        [KdlArgument(0)]
        public string Package { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string Version { get; set; } = "*";

        [KdlProperty("optional")]
        public bool Optional { get; set; }
    }
}
