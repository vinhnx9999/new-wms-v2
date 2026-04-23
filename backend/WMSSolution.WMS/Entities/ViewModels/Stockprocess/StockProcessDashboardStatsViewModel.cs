namespace WMSSolution.WMS.Entities.ViewModels.Stockprocess
{
    /// <summary>
    /// 
    /// </summary>
    public class StockProcessDashboardStatsViewModel
    {
        public int PendingCount { get; set; }
        public int ApprovedTodayCount { get; set; }
        public int TotalLossQuantity { get; set; }
    }
}
