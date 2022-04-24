# EnumConstants

[![license](https://img.shields.io/github/license/nagybalint001/enum-constants.svg?maxAge=2592000)](https://github.com/nagybalint001/enum-constants/blob/main/LICENSE) [![NuGet](https://img.shields.io/nuget/v/EnumConstants.svg?maxAge=2592000)](https://www.nuget.org/packages/EnumConstants/) ![downloads](https://img.shields.io/nuget/dt/EnumConstants)

Generaing string constants to a class from an enum type.

Sample code:

```csharp
[AutoConstants(typeof(OtherSampleEnum))]
public partial class Sample
{
    [AutoConstants(typeof(SampleEnum), "Asd")]
    public partial class SubClass
    {
        [AutoConstants(typeof(SampleEnum))]
        public partial class SubSubClass
        {
        }
    }

    public void Foo()
    {
        var foo = Sample.Value2; // Value2
        var bar = Sample.SubClass.Value1; // AsdValue1
        var baz = Sample.SubClass.SubSubClass.Value2; // Value2
    }
}

public enum SampleEnum
{
    Value1,
    Value2,
    Value3,
}

public enum OtherSampleEnum
{
    Value1,
    Value2,
    Value3,
}
```