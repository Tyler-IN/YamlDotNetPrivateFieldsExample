using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

public sealed class YamlSerializationEnforcedTypeInspector : TypeInspectorSkeleton {

  private readonly ITypeInspector _wrapped;

  public YamlSerializationEnforcedTypeInspector(ITypeInspector wrapped)
    => this._wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));

  public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container) {
    return type
      .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
      .Where(f => f.GetCustomAttribute<YamlSerializationEnforcedAttribute>() is not null)
      .Select(p => (IPropertyDescriptor)new ReflectionFieldDescriptor(p, _wrapped));
  }

  // based on ReadableFieldsTypeInspector.ReflectionFieldDescriptor from YamlDotNet
  private sealed class ReflectionFieldDescriptor : IPropertyDescriptor {

    private readonly FieldInfo _fieldInfo;

    private readonly ITypeInspector _typeInspector;

    public ReflectionFieldDescriptor(FieldInfo fieldInfo, ITypeInspector typeInspector) {
      _fieldInfo = fieldInfo;
      _typeInspector = typeInspector;
      ScalarStyle = ScalarStyle.Any;
    }

    public string Name
      => _fieldInfo.Name;

    public Type Type
      => _fieldInfo.FieldType;

    public Type? TypeOverride { get; set; }

    public int Order { get; set; }

    public bool CanWrite
      => !_fieldInfo.IsInitOnly;

    public ScalarStyle ScalarStyle { get; set; }

    public void Write(object? target, object? value)
      => _fieldInfo.SetValue(target, value);

    public T? GetCustomAttribute<T>() where T : Attribute
      => (T?)_fieldInfo.GetCustomAttributes(typeof(T), true).FirstOrDefault();

    public IObjectDescriptor Read(object? target) {
      var value = _fieldInfo.GetValue(target);
      return new ObjectDescriptor(value, TypeOverride ?? value?.GetType() ?? Type, Type, ScalarStyle);
    }

  }

}