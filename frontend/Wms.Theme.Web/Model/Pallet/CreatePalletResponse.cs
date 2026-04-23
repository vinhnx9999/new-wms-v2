namespace Wms.Theme.Web.Model.Pallet
{
    public class CreatePalletResponse
    {
        /// <summary>
        /// Pallet Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Pallet Code
        /// </summary>
        public string PalletCode { get; set; } = default!;
    }
}

