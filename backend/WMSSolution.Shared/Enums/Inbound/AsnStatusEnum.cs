namespace WMSSolution.Shared.Enums.Inbound
{
    /// <summary>
    /// Defines the status of an ASN
    /// </summary>
    public enum AsnStatusEnum
    {
        /// <summary>
        /// After the asn master is created, it is in the "Prepare Coming" status.
        /// PREPARE_COMING = New = 0
        /// </summary>
        PREPARE_COMING = 0,

        /// <summary>
        /// after the truck go into the stock ,
        /// The Manager confirm this Asn is has came , then the status change to "Prepare Unload"
        /// 0 -> 1
        /// </summary>
        PREPARE_UNLOAD = 1,

        /// <summary>
        /// after the goods has unloaded into stock ,
        /// The Manager confirm this Asn is ready for QC, then the status change to "Prepare QC"
        /// 1 -> 2
        /// </summary>
        PREPARE_QC = 2,

        /// <summary>
        /// After the goods has passed the QC ,
        /// The Manager confirm this Asn is ready for Putaway, then the status change to "Prepare Putaway"
        /// Ready moving to stock 
        /// 2 -> 3
        /// </summary>
        PREPARE_PUTAWAY = 3,

        /// <summary>
        /// After the goods has been chose location for Putaway ,
        /// The robot will coming to get the goods and putaway to the location
        /// System must wait until the robot has completed the putaway task
        /// The status only change when robot has confirm "the task has completed"
        /// 3 -> 4 
        /// </summary>
        WAITING_ROBOT = 4,

        /// <summary>
        /// The robot has completed the task  ,
        /// and notify for system to change the Asn status to "Complete"
        /// 4 -> 5
        /// </summary>
        COMPLETE = 5,

        /// <summary>
        /// Cancel base on the asn master 
        /// can't not change for the user set
        /// </summary>
        CANCELED = 8,
    }
}
