namespace WMSSolution.Shared.Enums.Location;

public enum GoodsLocationTypeEnum
{
    None = 0,
    StorageSlot = 1,
    HorizontalPath = 2,
    LiftPoint = 3,
    VirtualLocation = 4
}

public static class GoodsLocationTypeEnumExtension
{
    public static string GetDescription(this GoodsLocationTypeEnum enumValue)
    {
        return enumValue switch
        {
            GoodsLocationTypeEnum.None => "None",
            GoodsLocationTypeEnum.StorageSlot => "Storage Slot",
            GoodsLocationTypeEnum.HorizontalPath => "Horizontal Path",
            GoodsLocationTypeEnum.LiftPoint => "Lift Point",
            GoodsLocationTypeEnum.VirtualLocation => "Virtual Location",
            _ => "Unknown"
        };
    }

    public static GoodsLocationTypeEnum GetLocationType(this string type)
    {
        if (type == "StorageSlot") return GoodsLocationTypeEnum.StorageSlot;
        if (type == "HorizontalPath") return GoodsLocationTypeEnum.HorizontalPath;
        if (type == "LiftPoint") return GoodsLocationTypeEnum.LiftPoint;
        if (type == "VirtualLocation") return GoodsLocationTypeEnum.VirtualLocation;

        return GoodsLocationTypeEnum.None;
    }

    /// <summary>
    /// Handle for chosing Location Status
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static GoodLocationStatusEnum GetLocationStatus(this int? status)
    {
        switch (status)
        {
            case 0:
                return GoodLocationStatusEnum.AVAILABLE;
            case 1:
                return GoodLocationStatusEnum.OCCUPIED;
            case 2:
                return GoodLocationStatusEnum.BLOCKED;
            default:
                return GoodLocationStatusEnum.NONE;
        }
    }
}
