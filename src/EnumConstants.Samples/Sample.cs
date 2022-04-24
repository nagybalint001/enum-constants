namespace EnumConstants.Samples
{
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
            var bar = Sample.SubClass.Value1; // Asd_Value1
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
}
