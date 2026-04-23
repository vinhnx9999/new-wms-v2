namespace Wms.Theme.Web.Model.Units;

public class UnitDTO
{
    public int Id { get; set; } = 0;
    public string UnitName { get; set; } = "";
    public string? Description { get; set; }
}

public class SpecificationDTO
{
    public int Id { get; set; } = 0;
    public string DisplayName { get; set; } = "";
    public string Code { get; set; } = "";
}