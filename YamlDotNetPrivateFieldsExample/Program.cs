using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class Program {

  public static void Main() {
    var original = new PrivateFieldExampleClass(42);
    
    Console.WriteLine("=== ORIGINAL ===");
    Console.WriteLine(original);
    Console.WriteLine("=== ORIGINAL ===");
    
    
    var serializer = new SerializerBuilder()
      .WithTypeInspector((prev) => new YamlSerializationEnforcedTypeInspector(prev))
      .WithNamingConvention(CamelCaseNamingConvention.Instance)
      .Build();
    var yaml = serializer.Serialize(original);
    Console.WriteLine("=== SERIALIZED ===");
    Console.WriteLine(yaml);
    Console.WriteLine("=== SERIALIZED ===");

    var deserializer = new DeserializerBuilder()
      .WithTypeInspector((prev) => new YamlSerializationEnforcedTypeInspector(prev))
      .WithNodeDeserializer(new YamlSerializationEnforcedDeserializer<PrivateFieldExampleClass>())
      .WithNamingConvention(CamelCaseNamingConvention.Instance)
      .Build();

    var deserialized = deserializer.Deserialize<PrivateFieldExampleClass>(yaml);
    
    Console.WriteLine("=== DESERIALIZED VALUE ===");
    Console.WriteLine(deserialized);
    Console.WriteLine("=== DESERIALIZED VALUE ===");
  }

}