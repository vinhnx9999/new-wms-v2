namespace Wms.Theme.Web.Model.Supplier
{
    public class SupplierVM
    {
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; } = 0;

        /// <summary>
        /// supplier_name
        /// </summary>
        public string SupplierName { get; set; } = default!;

        /// <summary>
        /// city
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// address
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// manager
        /// </summary>

        public string Manager { get; set; } = string.Empty;

        /// <summary>
        /// Contact tel
        /// </summary>
        public string ContactTel { get; set; } = string.Empty;

        /// <summary>
        /// creator
        /// </summary>
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// create_time
        /// </summary>
        public DateTime CreateTime { get; set; } = default!;
    }
}
