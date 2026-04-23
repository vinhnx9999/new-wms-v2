using System.ComponentModel;
using WMSSolution.Shared.Enums;

namespace WMSSolution.WMS.Entities.ViewModels.IntegrationWCS.Inbound
{
    /// <summary>
    /// dto for get task by condition
    /// </summary>
    public class InboundTaskConditionRequest
    {
        /// <summary>
        /// Status
        /// </summary>
        public IntegrationStatus Status { get; set; }

        /// <summary>
        /// Date from
        /// </summary>
        [DefaultValue(null)]
        public DateTime? From { get; set; }
        /// <summary>
        /// Date to
        /// </summary>
        [DefaultValue(null)]
        public DateTime? To { get; set; }
    }
}
