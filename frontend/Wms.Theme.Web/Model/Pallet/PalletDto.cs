namespace Wms.Theme.Web.Model.Pallet
{
    public class PalletDto
    {
        public int Id { get; set; }
        public string PalletCode { get; set; } = string.Empty;
        public int PalletStatus { get; set; }
        public bool IsFull { get; set; }
        public bool IsMixed { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal CurrentWeight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string? PalletType { get; set; }
    }
}

