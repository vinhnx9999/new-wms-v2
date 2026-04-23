namespace WMSSolution.WMS.Entities.ViewModels;

/// <summary>
/// Unit
/// </summary>
public class UnitDTO
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; } = 0;
    /// <summary>
    /// Name
    /// </summary>
    public string UnitName { get; set; } = "";
    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Specification
/// </summary>
public class SpecificationDTO
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; } = 0;
    /// <summary>
    /// Name
    /// </summary>
    public string DisplayName { get; set; } = "";
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; } = "";
}