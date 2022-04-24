namespace EnumConstants.Samples
{
    public partial class Sample
    {
        [AutoConstants(typeof(SampleEnum), "Asd")]
        public partial class SubClass
        {
        }

        [AutoConstants(typeof(SampleEnum))]
        public partial class OtherSubClass
        {
        }

        public void Foo()
        {
            var asd = SubClass.Value1; // Asd_Value1
            var bar = OtherSubClass.Value2; // Value2
        }
    }

    public enum SampleEnum
    {
        Value1,
        Value2,
        Value3,
    }
}
