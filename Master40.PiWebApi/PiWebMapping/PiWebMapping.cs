namespace Master40.PiWebApi.PiWebMapping
{
    public struct PiWebMapping
    {
        public PiWebMapping(string attributeNameSource, ushort attributeNamePiWeb)
        {
            AttributeNameSource = attributeNameSource;
            AttributeNamePiWeb = attributeNamePiWeb;
        }
        public string AttributeNameSource { get; }
        public ushort AttributeNamePiWeb { get; }
    }
}
