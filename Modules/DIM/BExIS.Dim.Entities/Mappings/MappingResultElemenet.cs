namespace BExIS.Dim.Entities.Mappings
{
    public class MappingPartyResultElemenet
    {
        public string Value { get; set; }
        public long PartyId { get; set; }
    }

    public class MappingEntityResultElement
    {
        public string Value { get; set; }
        public long EntityId { get; set; }
        public long EntityTypeId { get; set; }

        public string Url { get; set; }
    }
}