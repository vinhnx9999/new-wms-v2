namespace Wms.Theme.Web.Model.Supplier
{
    public class UpdateSupplierRequest
    {
        /// <summary>
        /// supplier_code
        /// </summary>
        public string? SupplierCode { get; set; }

        /// <summary>
        /// supplier_name
        /// </summary>
        public string? SupplierName { get; set; }

        /// <summary>
        /// city
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// manager
        /// </summary>
        public string? Manager { get; set; }

        /// <summary>
        /// contact_tel
        /// </summary>
        public string? ContactTel { get; set; }

        /// <summary>
        /// tax_number
        /// </summary>
        public string? TaxNumber { get; set; }
    }
}
