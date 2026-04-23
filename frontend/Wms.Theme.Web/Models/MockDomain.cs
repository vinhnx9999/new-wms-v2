namespace Wms.Theme.Domain.Entities
{
    using Wms.Theme.Domain.Enums;

    public class Receipt
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = new();
        public DateTime ReceiptDate { get; set; }
        public ReceiptStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ReceiptLine> ReceiptLines { get; set; } = new();
    }

    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class SaleOrder
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = new();
        public string Customer { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
    }

    public class ReceiptLine
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}

namespace Wms.Theme.Domain.Enums
{
    public enum ReceiptStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}
