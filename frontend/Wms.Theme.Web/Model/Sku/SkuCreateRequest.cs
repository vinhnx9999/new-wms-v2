namespace Wms.Theme.Web.Model.Sku
{
    public class SkuCreateRequest
    {
        /// <summary>
        /// Category Id 
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Spu Id
        /// </summary>
        public int? SpuId { get; set; }

        /// <summary>
        /// Spu code 
        /// </summary>
        public string? SpuCode { get; set; }

        /// <summary>
        /// Specfication Code
        /// </summary>
        public List<string> SpecificationCodes { get; set; } = new List<string>();

        /// <summary>
        /// Sku code
        /// </summary>
        public required string SkuCode { get; set; }

        /// <summary>
        /// Sku name
        /// </summary>
        public required string SkuName { get; set; }

        /// <summary>
        /// Bar code
        /// </summary>
        public string? BarCode { get; set; }

        /// <summary>
        ///   Weight
        /// </summary>
        public decimal Weight { get; set; } = 0;

        /// <summary>
        /// Lenght 
        /// </summary>
        public decimal Lenght { get; set; } = 0;

        /// <summary>
        /// Width
        /// </summary>
        public decimal Width { get; set; } = 0;

        /// <summary>
        /// Height 
        /// </summary>
        public decimal Height { get; set; } = 0;

        /// <summary>
        /// Volume
        /// </summary>
        public decimal Volume { get; set; } = 0;

        /// <summary>
        /// cost
        /// </summary>
        public decimal Cost { get; set; } = 0;

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; } = 0;

        /// <summary>
        /// Uom
        /// </summary>
        public int SkuUomID { get; set; }
    }
}
