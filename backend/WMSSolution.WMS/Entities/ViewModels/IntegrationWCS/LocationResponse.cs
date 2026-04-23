namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS;

/// <summary>
/// Handle Base response from WCS
/// </summary>
public class BaseResponse
{
    /// <summary>
    /// IsSuccess
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// Code
    /// </summary>
    public int Code { get; set; }
    /// <summary>
    /// Error Message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Wcs Response
/// </summary>
public class WcsResponse : BaseResponse
{
    /// <summary>
    /// Data
    /// </summary>
    public List<LocationResponse> Data { get; set; } = [];
}

/// <summary>
/// Return per page
/// </summary>
public class WcsPageResponse : BaseResponse
{
    public PagedResult Data { get; set; }
}

/// <summary>
/// Paged Result
/// </summary>
public class PagedResult
{
    /// <summary>
    /// Items
    /// </summary>
    public List<LocationResponse> Items { get; set; } = [];
}

/// <summary>
/// Wcs Block Response
/// </summary>
public class WcsBlockResponse
{
    /// <summary>
    /// IsSuccess
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// Code
    /// </summary>
    public int Code { get; set; }
    /// <summary>
    /// Error Message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    /// <summary>
    /// Data
    /// </summary>
    public List<BlockLocation> Data { get; set; } = [];
}

/// <summary>
/// Block Response from WCS
/// </summary>
public class BlockLocation
{
    /// <summary>
    /// Gets or sets the unique identifier for the block location response.
    /// </summary>
    public string Id { get; set; } = "";
    /// <summary>
    /// BlockCode
    /// </summary>
    public string BlockCode { get; set; } = "";
    /// <summary>
    /// Block Name
    /// </summary>
    public string BlockName { get; set; } = "";
    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = "";
}

/// <summary>
/// WCS locations
/// </summary>
public class LocationResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the location response.
    /// </summary>
    public string Address { get; set; } = "";
    /// <summary>
    /// zone
    /// </summary>
    public string Zone { get; set; } = "";
    /// <summary>
    /// Level
    /// </summary>
    public int Level { get; set; } = 1;
    /// <summary>
    /// Type
    /// </summary>
    public string Type { get; set; } = "";
    /// <summary>
    /// Status
    /// </summary>
    public int Status { get; set; } = 1;
    /// <summary>
    /// Gets or sets the X coordinate of the location.
    /// </summary>
    public int CoordX { get; set; }
    /// <summary>
    /// Gets or sets the Y coordinate of the location.
    /// </summary>
    public int CoordY { get; set; }
    /// <summary>
    /// Gets or sets the Z coordinate of the location.
    /// </summary>
    public int CoordZ { get; set; }
    /// <summary>
    /// Storage priority
    /// </summary>
    public int StoragePriority { get; set; } = 1;
    /// <summary>
    /// Use By
    /// </summary>
    public List<string> InUseBy { get; set; } = [];
}