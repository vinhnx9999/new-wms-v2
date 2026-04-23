namespace Wms.Theme.Web.Model.Customer
{
    public class AddCustomerRequest
    {
        /// <summary>
        /// primary key
        /// </summary>
        public int ID { get; set; } = 0;

        /// <summary>
        /// customer's name
        /// </summary>

        public string CustomerName { get; set; } = default!;

        /// <summary>
        /// city
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// address
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// manager
        /// </summary>
        public string Manager { get; set; } = string.Empty;

        /// <summary>
        /// email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// contact tel
        /// </summary>
        public string ContactTel { get; set; } = string.Empty;


    }
}
