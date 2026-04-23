namespace WMSSolution.WMS.Entities.ViewModels.Goodslocation;

/// <summary>
/// Lightweight location response (no stock/products)
/// </summary>
public class LocationOnlyViewModel : LocationDto
{
    /// <summary>
    /// ID of the location
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Virtual location 
    /// </summary>
    public bool VirtualLocation { get; set; }
}