namespace EleCho.JsonModelGenerator.Core
{
    public class JsonModelWriterOptions
    {
        public int Indent { get; set; } = 4;
        public NamingStyle NamingStyle { get; set; } = NamingStyle.Pascal;
        public ClassNestStyle ClassNestStyle { get; set; } = ClassNestStyle.Nest;
        public CollectionTypeStyle CollectionTypeStyle { get; set; } = CollectionTypeStyle.Array;
        public CollectionItemNamingStyle CollectionItemNamingStyle { get; set; } = CollectionItemNamingStyle.AutomaticallyTranslate;
        
        public AttributeStyle AttributeStyle { get; set; } = AttributeStyle.None;
    }
}
