using Mapster;

namespace WMSSolution.WMS.Entities.ViewModels.Supplier
{
    /// <summary>
    /// Supplier add request
    /// </summary>
    public class AddSupplierRequest
    {
        /// <summary>
        /// Supplier name 
        /// </summary>
        [AdaptMember("supplier_name")]
        public string SupplierName { get; set; } = default!;

        /// <summary>
        /// Address
        /// </summary>
        [AdaptMember("address")]
        public string Address { get; set; } = default!;

        /// <summary>
        /// City
        /// </summary>
        [AdaptMember("city")]
        public string City { get; set; } = default!;

        /// <summary>
        /// Email
        /// </summary>
        [AdaptMember("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Contact Tel
        /// </summary>
        [AdaptMember("contact_tel")]
        public string? ContactTel { get; set; }

        /// <summary>
        /// Manager
        /// </summary>
        [AdaptMember("manager")]
        public string? Manager { get; set; }
    }
}
