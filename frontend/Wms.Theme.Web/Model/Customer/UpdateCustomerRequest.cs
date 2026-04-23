namespace Wms.Theme.Web.Model.Customer
{
    public class UpdateCustomerRequest
    {
        /// <summary>
        /// Customer name
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// City
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
        /// contact tel
        /// </summary>
        public string? ContactTel { get; set; }

        /// <summary>
        /// customer_code
        /// </summary>
        public string? CustomerCode { get; set; } = "";
        /// <summary>
        /// tax_number
        /// </summary>
        public string? TaxNumber { get; set; } = "";
    }
}
