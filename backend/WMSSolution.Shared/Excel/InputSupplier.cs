namespace WMSSolution.Shared.Excel;

public class SupplyChain : InputExcelBase
{
    public string IsFollow { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class InputSupplier : SupplyChain
{
    public string GroupSupplier { get; set; } = string.Empty;
}

public class InputCustomer : SupplyChain
{
    public string GroupCustomer { get; set; } = string.Empty;
}
