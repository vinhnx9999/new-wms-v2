namespace WMSSolution.WMS.Entities.ViewModels.PurchaseOrders
{
    /// <summary>
    /// 
    /// </summary>
    public sealed record PageSearchPOResponse
    {
        /// <summary>
        /// Id 
        /// </summary>
        public int Id { get; set; } = default!;

        /// <summary>
        /// Po no 
        /// </summary>
        public string PoNo { get; set; } = string.Empty;

        /// <summary>
        /// Expected Delivery date
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; } = default!;

        /// <summary>
        /// Po status
        /// </summary>
        public int PoStatus { get; set; } = default!;

        /// <summary>
        /// Total amount 
        /// </summary>
        public decimal TotalAmount { get; set; } = default!;

        /// <summary>
        ///  Creator
        /// </summary>
        public string Creator { get; set; } = default!;

        /// <summary>
        /// Order date 
        /// </summary>
        public DateTime OrderDate { get; set; } = default!;
    }
}
