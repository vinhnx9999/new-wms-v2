using Wms.Theme.Web.Model.OutboundReceipt;
using Wms.Theme.Web.Util;

namespace Wms.Theme.Web.Model.Planning;

public class AddPickingPlanningDTO
{
    public List<PickingDTO> PickingList { get; set; } = [];
}

public class PickingDTO : BaseOutboundDetailDto
{
    public int? Id { get; set; } = 0;
    public int? ReceiptId { get; set; } = 0;
    /// <summary>
    /// Receipt Number
    /// </summary>
    public string ReceiptNo { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse ID
    /// </summary>
    public int WarehouseId { get; set; }
    /// <summary>
    /// Warehouse Name
    /// </summary>
    public string WarehouseName { get; set; } = string.Empty;
    /// <summary>
    /// Warehouse Address
    /// </summary>
    public string WarehouseAddress { get; set; } = string.Empty;
    public DateTime? ExpectedShipDate { get; set; }
    public DateTime? StartPickingTime { get; set; }
    /// <summary>
    /// Sku Code
    /// </summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>
    /// Sku Name
    /// </summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>
    /// Unit name
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// LocationName 
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Expire Date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    /// <summary>
    /// GatewayId
    /// </summary>
    public int? GatewayId { get; set; }
    public bool? IsVirtualLocation { get; set; }
    public bool IsOverdue => ExpectedShipDate != null && ExpectedShipDate < DateTime.UtcNow;
    public string? ExpectedShipTimer => ExpectedShipDate.ConvertDate2LocalTime();    
}
