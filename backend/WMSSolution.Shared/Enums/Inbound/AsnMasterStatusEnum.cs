namespace WMSSolution.Shared.Enums.Inbound
{
    public enum AsnMasterStatusEnum
    {
        /// <summary>
        /// NEW ASN HAS CAME 
        /// </summary>
        CREATED = 0,
        /// <summary>
        /// IN PROCESSING 
        /// THIS VALUE CAN BE CHANGE ONLY ONNCE 
        /// IF THE THE DETAIL SET FOR THE STATUS 2 
        /// </summary>
        IN_PROGRESS = 1,
        /// <summary>
        /// COMPLETED WHEN ALL THE ASN DETAIL HAS COMPLETED
        /// </summary>
        COMPLETED = 2,
        /// <summary>
        /// CANCELED BASE ON THE ASN MASTER
        /// </summary>
        CANCELED = 8,
    }
}
