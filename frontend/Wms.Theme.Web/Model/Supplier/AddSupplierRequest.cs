namespace Wms.Theme.Web.Model.Supplier;

public class AddSupplierRequest
{
    /// <summary>
    /// Supplier name 
    /// </summary>
    public string SupplierName { get; set; } = default!;

    /// <summary>
    /// Address
    /// </summary>
    public string Address { get; set; } = default!;

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = default!;

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Contact Tel
    /// </summary>
    public string? ContactTel { get; set; }

    /// <summary>
    /// Manager
    /// </summary>
    public string? Manager { get; set; }
}
