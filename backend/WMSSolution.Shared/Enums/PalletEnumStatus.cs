namespace WMSSolution.Shared.Enums
{
    public enum PalletEnumStatus
    {
        /// <summary>
        ///  mark as pallet available for choosing
        /// </summary>
        Available = 1,
        /// <summary>
        /// mark as pallet Has full not for choosing
        /// </summary>
        InUse = 2,
        /// <summary>
        /// Indicates that the item is damaged and may not be suitable for normal use.
        /// </summary>
        Damaged = 3,
        /// <summary>
        /// Indicates that the item is currently in transit between locations.
        /// Not suitable for choosing
        /// </summary>
        InTransit = 4,
    }
}
