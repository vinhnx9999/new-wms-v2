namespace WMSSolution.Shared.Excel;

public class InputExcelBase
{
    public string Code { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;

    public override bool Equals(object obj)
    {
        return obj is InputExcelBase other &&
               Code == other.Code &&
               Property == other.Property;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Property);
    }
}
