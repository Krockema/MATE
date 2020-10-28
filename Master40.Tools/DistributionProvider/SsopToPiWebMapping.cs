namespace Master40.Tools.DistributionProvider
{
    public struct SsopToPiWebMapping
        {
            public SsopToPiWebMapping(string attributeNameSsop, ushort attributeNamePiWeb)
            {
                AttributeNameSsop = attributeNameSsop;
                AttributeNamePiWeb = attributeNamePiWeb;
            }
            public string AttributeNameSsop { get; }
            public ushort AttributeNamePiWeb { get; }
        }
}
