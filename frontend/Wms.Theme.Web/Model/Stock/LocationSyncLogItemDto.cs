namespace Wms.Theme.Web.Model.Stock
{
    public class LocationSyncLogItemDto
    {
        public string TraceId { get; set; } = default!;
        public DateTime? ActionTime { get; set; }
        public DateTime ReceivedTime { get; set; }
        public int ConflictInserted { get; set; }
        public int ConflictUpdated { get; set; }
        public string Status { get; set; } = default!;
    }

    public class LocationSyncConflictKeyDto
    {
        public string TraceId { get; set; } = default!;
        public string LocationName { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public bool WmsHasPallet { get; set; }
        public byte WcsStatus { get; set; }
    }
}
