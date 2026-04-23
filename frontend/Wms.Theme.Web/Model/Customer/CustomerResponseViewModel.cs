namespace Wms.Theme.Web.Model.Customer
{
    public class CustomerResponseViewModel
    {
        /// <summary>
        /// primary key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// customer's name
        /// </summary>

        public string CustomerName { get; set; } = string.Empty;

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

        /// <summary>
        /// creator
        /// </summary>
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// create time
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// last update time
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// valid
        /// </summary>
        public bool IsValid { get; set; } = true;
    }
}
