namespace Wms.Theme.Web.Model.Pallet
{
    public class PalletPageSearchResponse
    {
        public List<PalletPageSearchDTO> Rows { get; set; } = [];
        public int Totals { get; set; } = 0;
    }
}
