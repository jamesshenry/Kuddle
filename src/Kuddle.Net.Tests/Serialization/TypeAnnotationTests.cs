using System;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class TypeAnnotationTests
{
    #region Test Models

    public class PersonWithAnnotations
    {
        [KdlArgument(0, "uuid")]
        public Guid Id { get; set; }

        [KdlProperty("name")]
        public string Name { get; set; } = "";

        [KdlProperty("created", "date-time")]
        public DateTimeOffset CreatedAt { get; set; }

        [KdlProperty("age", "i32")]
        public int Age { get; set; }
    }

    public class ProductWithTypeAnnotations
    {
        [KdlArgument(0)]
        public string Name { get; set; } = "";

        [KdlProperty("sku", "uuid")]
        public Guid Sku { get; set; }

        [KdlProperty("price", "decimal64")]
        public decimal Price { get; set; }

        [KdlProperty("stock", "u32")]
        public int StockCount { get; set; }
    }

    public class EventWithChildAnnotations
    {
        [KdlArgument(0)]
        public string Name { get; set; } = "";

        [KdlNode("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [KdlNode("correlation-id")]
        public Guid CorrelationId { get; set; }
    }

    #endregion

    [Test]
    public async Task Serialize_WithUuidTypeAnnotation_WritesAnnotation()
    {
        // Arrange
        var person = new PersonWithAnnotations
        {
            Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
            Name = "Alice",
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Age = 30,
        };

        // Act
        var kdl = KdlSerializer.Serialize(person);

        // Assert
        await Assert.That(kdl).Contains("(uuid)\"123e4567-e89b-12d3-a456-426614174000\"");
        await Assert.That(kdl).Contains("(date-time)\"2024-01-01T00:00:00.0000000+00:00\"");
        await Assert.That(kdl).Contains("(i32)30");
    }

    [Test]
    public async Task Deserialize_WithUuidTypeAnnotation_ParsesCorrectly()
    {
        // Arrange
        var kdl = """
            personwithannotations (uuid)"123e4567-e89b-12d3-a456-426614174000" name="Alice" created=(date-time)"2024-01-01T00:00:00.0000000+00:00" age=(i32)30
            """;

        // Act
        var person = KdlSerializer.Deserialize<PersonWithAnnotations>(kdl);

        // Assert
        await Assert.That(person.Id).IsEqualTo(Guid.Parse("123e4567-e89b-12d3-a456-426614174000"));
        await Assert.That(person.Name).IsEqualTo("Alice");
        await Assert
            .That(person.CreatedAt)
            .IsEqualTo(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        await Assert.That(person.Age).IsEqualTo(30);
    }

    [Test]
    public async Task Serialize_WithMixedTypeAnnotations_WritesAllAnnotations()
    {
        // Arrange
        var product = new ProductWithTypeAnnotations
        {
            Name = "Widget",
            Sku = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            Price = 99.99m,
            StockCount = 42,
        };

        // Act
        var kdl = KdlSerializer.Serialize(product);

        // Assert
        await Assert.That(kdl).Contains("Widget");
        await Assert.That(kdl).Contains("(uuid)\"a1b2c3d4-e5f6-7890-abcd-ef1234567890\"");
        await Assert.That(kdl).Contains("(decimal64)99.99");
        await Assert.That(kdl).Contains("(u32)42");
    }

    [Test]
    public async Task Deserialize_WithMixedTypeAnnotations_ParsesAllValues()
    {
        // Arrange
        var kdl = """
            productwithtypeannotations "Widget" sku=(uuid)"a1b2c3d4-e5f6-7890-abcd-ef1234567890" price=(decimal64)99.99 stock=(u32)42
            """;

        // Act
        var product = KdlSerializer.Deserialize<ProductWithTypeAnnotations>(kdl);

        // Assert
        await Assert.That(product.Name).IsEqualTo("Widget");
        await Assert
            .That(product.Sku)
            .IsEqualTo(Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));
        await Assert.That(product.Price).IsEqualTo(99.99m);
        await Assert.That(product.StockCount).IsEqualTo(42);
    }

    [Test]
    public async Task Serialize_WithChildNodeAnnotations_WritesAnnotationsOnChildren()
    {
        // Arrange
        var evt = new EventWithChildAnnotations
        {
            Name = "UserLoggedIn",
            Timestamp = new DateTimeOffset(2024, 12, 17, 10, 30, 0, TimeSpan.Zero),
            CorrelationId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
        };

        // Act
        var kdl = KdlSerializer.Serialize(evt);

        // Assert
        await Assert
            .That(kdl)
            .Contains("timestamp (date-time)\"2024-12-17T10:30:00.0000000+00:00\"");
        await Assert
            .That(kdl)
            .Contains("correlation-id (uuid)\"11111111-2222-3333-4444-555555555555\"");
    }

    [Test]
    public async Task Deserialize_WithChildNodeAnnotations_ParsesChildValues()
    {
        // Arrange
        var kdl = """
            eventwithchildannotations "UserLoggedIn" {
                timestamp (date-time)"2024-12-17T10:30:00.0000000+00:00"
                correlation-id (uuid)"11111111-2222-3333-4444-555555555555"
            }
            """;

        // Act
        var evt = KdlSerializer.Deserialize<EventWithChildAnnotations>(kdl);

        // Assert
        await Assert.That(evt.Name).IsEqualTo("UserLoggedIn");
        await Assert
            .That(evt.Timestamp)
            .IsEqualTo(new DateTimeOffset(2024, 12, 17, 10, 30, 0, TimeSpan.Zero));
        await Assert
            .That(evt.CorrelationId)
            .IsEqualTo(Guid.Parse("11111111-2222-3333-4444-555555555555"));
    }

    [Test]
    public async Task RoundTrip_WithTypeAnnotations_PreservesValues()
    {
        // Arrange
        var original = new PersonWithAnnotations
        {
            Id = Guid.NewGuid(),
            Name = "Bob",
            CreatedAt = DateTimeOffset.UtcNow,
            Age = 25,
        };

        // Act
        var kdl = KdlSerializer.Serialize(original);
        var deserialized = KdlSerializer.Deserialize<PersonWithAnnotations>(kdl);

        // Assert
        await Assert.That(deserialized.Id).IsEqualTo(original.Id);
        await Assert.That(deserialized.Name).IsEqualTo(original.Name);
        await Assert.That(deserialized.CreatedAt).IsEqualTo(original.CreatedAt);
        await Assert.That(deserialized.Age).IsEqualTo(original.Age);
    }

    [Test]
    public async Task Deserialize_WithoutTypeAnnotationInKdl_StillWorks()
    {
        // Arrange - KDL without type annotation, but C# model has [KdlTypeAnnotation]
        // The deserializer should still work because Guid/DateTimeOffset have their own parsing logic
        var kdl = """
            personwithannotations "123e4567-e89b-12d3-a456-426614174000" name="Alice" created="2024-01-01T00:00:00.0000000+00:00" age=30
            """;

        // Act
        var person = KdlSerializer.Deserialize<PersonWithAnnotations>(kdl);

        // Assert - should parse without type annotations in the KDL
        await Assert.That(person.Id).IsEqualTo(Guid.Parse("123e4567-e89b-12d3-a456-426614174000"));
        await Assert.That(person.Name).IsEqualTo("Alice");
        await Assert
            .That(person.CreatedAt)
            .IsEqualTo(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        await Assert.That(person.Age).IsEqualTo(30);
    }
}
