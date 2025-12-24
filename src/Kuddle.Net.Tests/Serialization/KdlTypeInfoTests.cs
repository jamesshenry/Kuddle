// using Kuddle.Exceptions;
// using Kuddle.Serialization;

// namespace Kuddle.Tests.Serialization;

// public class KdlTypeInfoTests
// {
//     [Test]
//     public async Task ArgumentContinuity_MissingIndex_Throws()
//     {
//         var ex = Assert.Throws<KdlConfigurationException>(() =>
//             KdlTypeMapping.For<ArgContinuityBad>()
//         );
//         await Assert.That(ex).IsNotNull();
//     }

//     [Test]
//     public async Task PropertyAndNode_Mapping_Works()
//     {
//         var info = KdlTypeMapping.For<PropertyNodeMap>();
//         await Assert.That(info.Properties.Count).IsEqualTo(1);
//         await Assert.That(info.Children.Count).IsEqualTo(1);
//     }

//     [Test]
//     public async Task NodeDictionary_Property_IsDetected()
//     {
//         var info = KdlTypeMapping.For<NodeDictHolder>();
//         await Assert.That(info.Children.Count).IsEqualTo(1);

//         var dictInfo = KdlTypeMapping.For<Dictionary<string, string>>();
//         await Assert.That(dictInfo.IsDictionary).IsTrue();
//         await Assert.That(dictInfo.DictionaryDef).IsNotNull();
//         await Assert.That(dictInfo.DictionaryDef!.ValueType).IsEqualTo(typeof(string));
//     }

//     [Test]
//     public async Task CollectionDetection_Works_And_Dictionaries_Are_Not_Collections()
//     {
//         var arrInfo = KdlTypeMapping.For<int[]>();
//         await Assert.That(arrInfo.CollectionElementType).IsEqualTo(typeof(int));
//         await Assert.That(arrInfo.IsIEnumerable).IsTrue();

//         var dictInfo = KdlTypeMapping.For<Dictionary<string, string>>();
//         await Assert.That(dictInfo.IsIEnumerable).IsFalse();
//     }

//     [Test]
//     public async Task KdlTypeAttribute_Overrides_NodeName()
//     {
//         var info = KdlTypeMapping.For<CustomName>();
//         await Assert.That(info.NodeName).IsEqualTo("my-node");
//     }

//     [Test]
//     public async Task IsStrictNode_Behaves_Correctly()
//     {
//         var plain = KdlTypeMapping.For<PlainDocument>();
//         await Assert.That(plain.IsStrictNode).IsFalse();

//         var withType = KdlTypeMapping.For<CustomName>();
//         await Assert.That(withType.IsStrictNode).IsTrue();

//         var withProp = KdlTypeMapping.For<PropertyNodeMap>();
//         await Assert.That(withProp.IsStrictNode).IsTrue();
//     }

//     [Test]
//     public async Task For_Is_Cached()
//     {
//         var a = KdlTypeMapping.For<PropertyNodeMap>();
//         var b = KdlTypeMapping.For<PropertyNodeMap>();
//         await Assert.That(object.ReferenceEquals(a, b)).IsTrue();
//     }

//     [Test]
//     public async Task KdlTypeInfo_Throws_ForEveryAttributeCombination()
//     {
//         var typesToTest = new[]
//         {
//             typeof(Conflict_Property_Node),
//             typeof(Conflict_Property_Argument),
//             typeof(Conflict_Node_NodeDict),
//         };

//         foreach (var t in typesToTest)
//         {
//             var exception = Assert.Throws<KdlConfigurationException>(() => KdlTypeMapping.For(t));
//             await Assert.That(exception).IsNotNull();
//         }
//     }

//     // Test types

//     private class Conflict_Property_Node
//     {
//         [KdlProperty("k")]
//         [KdlNode("n")]
//         public string? Prop { get; set; }
//     }

//     private class Conflict_Property_Argument
//     {
//         [KdlProperty("k")]
//         [KdlArgument(0)]
//         public string? Prop { get; set; }
//     }

//     private class Conflict_Node_NodeDict
//     {
//         [KdlNode("n")]
//         [KdlNodeDictionary("d")]
//         public string? Prop { get; set; }
//     }

//     private class ArgContinuityBad
//     {
//         [KdlArgument(0)]
//         public string? A { get; set; }

//         [KdlArgument(2)]
//         public string? B { get; set; }
//     }

//     private class PropertyNodeMap
//     {
//         [KdlProperty("k")]
//         public string? Prop { get; set; }

//         [KdlNode("n")]
//         public string? Child { get; set; }
//     }

//     private class NodeDictHolder
//     {
//         [KdlNodeDictionary("d")]
//         public Dictionary<string, string>? Dict { get; set; }
//     }

//     [KdlType("my-node")]
//     private class CustomName { }

//     private class PlainDocument { }
// }
