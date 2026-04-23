using Wms.Theme.Web.Model.Sorted;

namespace Wms.Theme.Web.Model.ASN
{
    public class AddQcDto
    {
        public int Id { get; set; }
        public int SortedQuantity { get; set; }
        public bool IsAutoNum { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
    public class QcActionDto
    {
        public List<int> Ids { get; set; } = new();
    }

    public class UpdateQcDto
    {
        public List<UpdateAnsSortedRequest> Items { get; set; } = new();
    }
}
