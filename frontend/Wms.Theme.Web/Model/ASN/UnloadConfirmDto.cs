namespace Wms.Theme.Web.Model.ASN
{
    public class UnloadConfirmDto
    {
        public List<int> Ids { get; set; } = new();
        public string UnloadPerson { get; set; } = string.Empty;
        public int UnloadPersonId { get; set; } = 0;
        public int InputQty { get; set; } = 0;
    }

    /// <summary>
    /// DTO for individual item unload confirmation request.
    /// </summary>
    public class UnloadItemRequest
    {
        public int Id { get; set; }
        public string UnloadPerson { get; set; } = string.Empty;
        public int InputQty { get; set; }
    }

    public class RejectDto
    {
        public List<int> Ids { get; set; } = new();
    }
}
