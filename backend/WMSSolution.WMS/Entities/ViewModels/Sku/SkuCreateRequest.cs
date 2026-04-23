using Mapster;
using WMSSolution.Shared.Enums;
using WMSSolution.Shared.Excel;

namespace WMSSolution.WMS.Entities.ViewModels.Sku;

/// <summary>
/// Sku Create Request
/// </summary>
public class SkuCreateRequest
{
    /// <summary>
    /// Category Id 
    /// </summary>
    [AdaptMember("category_id")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Spu Id
    /// </summary>
    [AdaptMember("spu_id")]
    public int? SpuId { get; set; }

    /// <summary>
    /// Spu code 
    /// </summary>
    [AdaptMember("spu_code")]
    public string? SpuCode { get; set; }

    /// <summary>
    /// Specfication Code
    /// </summary>
    [AdaptIgnore]
    public List<string> SpecificationCodes { get; set; } = new List<string>();

    /// <summary>
    /// Sku code
    /// </summary>
    [AdaptMember("sku_code")]
    public required string SkuCode { get; set; }

    /// <summary>
    /// Sku name
    /// </summary>
    [AdaptMember("sku_name")]
    public required string SkuName { get; set; }

    /// <summary>
    /// Bar code
    /// </summary>
    [AdaptMember("bar_code")]
    public string? BarCode { get; set; }

    /// <summary>
    ///   Weight
    /// </summary>
    [AdaptMember("weight")]
    public decimal Weight { get; set; } = 0;

    /// <summary>
    /// Lenght 
    /// </summary>
    [AdaptMember("lenght")]
    public decimal Lenght { get; set; } = 0;

    /// <summary>
    /// Width
    /// </summary>
    [AdaptMember("width")]
    public decimal Width { get; set; } = 0;

    /// <summary>
    /// Height 
    /// </summary>
    [AdaptMember("height")]
    public decimal Height { get; set; } = 0;

    /// <summary>
    /// Volume
    /// </summary>
    [AdaptMember("volume")]
    public decimal Volume { get; set; } = 0;

    /// <summary>
    /// cost
    /// </summary>
    [AdaptMember("cost")]
    public decimal Cost { get; set; } = 0;

    /// <summary>
    /// Price
    /// </summary>
    [AdaptMember("price")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// Uom
    /// </summary>
    [AdaptIgnore]
    public int SkuUomID { get; set; }
}


/// <summary>
/// Sub-DTO for UOM details during SKU creation
/// </summary>
public class SkuUomRequest
{
    /// <summary>
    /// unit name
    /// </summary>
    public required string UnitName { get; set; }
    /// <summary>
    /// Conversation rate
    /// </summary>
    public int ConversionRate { get; set; }
    /// <summary>
    /// Is base unit ?
    /// </summary>
    public bool IsBaseUnit { get; set; }
    /// <summary>
    /// Operator
    /// </summary>
    public ConversionOperator Operator { get; set; } = ConversionOperator.Multiply;
}

public class SkuDataExcel
{
    public List<InputSku> InputSkus { get; set; } = [];
}