public class PrivateFieldExampleClass {

  [YamlSerializationEnforced]
  private int _example;

  public PrivateFieldExampleClass(int example)
    => _example = example;

  public override string ToString()
    => $"PrivateFieldExampleClass: {_example}";
  
}