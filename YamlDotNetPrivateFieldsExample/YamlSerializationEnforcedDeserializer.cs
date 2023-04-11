using System.Reflection;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public class YamlSerializationEnforcedDeserializer<T> : INodeDeserializer {

  public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value) {
    if (!expectedType.IsAssignableTo(typeof(T))) {
      value = null;
      return false;
    }

    if (!reader.TryConsume<MappingStart>(out var mapping)) {
      value = null;
      return false;
    }

    // TODO: use activator, factory, check for and call a constructor, or something else here if you want, this is just for demo purposes
    value = RuntimeHelpers.GetUninitializedObject(expectedType);

    var properties = expectedType
      .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
      .Where(f => f.GetCustomAttribute<YamlSerializationEnforcedAttribute>() is not null)
      .ToDictionary(f => f.Name, k => k);

    while (!reader.TryConsume<MappingEnd>(out _)) {
      if (!reader.TryConsume<Scalar>(out var key))
        throw new YamlException(reader.Current!.Start, reader.Current.End,
          "Expected a scalar");

      if (!properties.TryGetValue(key.Value, out var property))
        throw new YamlException(reader.Current!.Start, reader.Current.End,
          $"Unknown property {key.Value}");

      var propertyValue = nestedObjectDeserializer(reader, property.FieldType);

      if (propertyValue is IValuePromise promise) {
        var scopedRefCopyValue = value;
        promise.ValueAvailable += resolved => property.SetValue(scopedRefCopyValue, resolved);
      }
      else {
        if (propertyValue is null) {
          if (property.FieldType.IsValueType)
            throw new YamlException(reader.Current!.Start, reader.Current.End,
              $"Cannot assign null to {property.FieldType} {property.Name}");

          property.SetValue(value, null);
          continue;
        }

        if (!property.FieldType.IsInstanceOfType(propertyValue))
          throw new YamlException(reader.Current!.Start, reader.Current.End,
            $"Cannot assign {propertyValue.GetType()} to {property.FieldType} {property.Name}");

        property.SetValue(value, propertyValue);
      }
    }

    return true;
  }

}