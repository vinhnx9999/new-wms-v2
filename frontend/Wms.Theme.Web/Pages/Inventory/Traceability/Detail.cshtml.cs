using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wms.Theme.Web.Pages.Inventory.Traceability
{
    public class DetailModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string LotNo { get; set; }

        public LotDetailViewModel Lot { get; set; }

        public void OnGet()
        {
            // MOCK DATA (Giả lập lấy từ DB)
            Lot = new LotDetailViewModel
            {
                LotNo = "L123",
                ProductCode = "SKU-100501",
                ProductName = "Nipun",
                Status = "Shipped", // Shipped, InStock
                InboundDate = new DateTime(2023, 01, 01),

                // Dữ liệu cho Sơ đồ cây (Tree Nodes)
                Nodes = new List<LotTreeNode>
                {
                    new() { Title = "Nhập kho (Inbound)", SubTitle = "PO-001\nNCC ABC\nSL: 100", Type = "Inbound" },
                    new() { Title = "Lưu kho (Storage)", SubTitle = "Kệ A-01 (50)\nKệ B-02 (50)", Type = "Storage" },
                    new() { Title = "Xuất kho (Outbound)", SubTitle = "SO-100\nKhách A\nSL: 50", Type = "Outbound" },
                    new() { Title = "Xuất kho (Outbound)", SubTitle = "SO-101\nKhách B\nSL: 50", Type = "Outbound" }
                },

                // Dữ liệu bảng Log
                Logs = new List<LotTransactionLog>
                {
                    new() { Time = "2023-01-01 10:00", Type = "Nhập kho", DocNo = "PO-001", Qty = "+100", Location = "A-01", User = "Admin" },
                    new() { Time = "2023-01-05 14:30", Type = "Chuyển vị trí", DocNo = "TRF-001", Qty = "-50 (A-01) / +50 (B-02)", Location = "A-01 -> B-02", User = "User1" },
                    new() { Time = "2023-01-10 09:15", Type = "Xuất kho", DocNo = "SO-100", Qty = "-50", Location = "A-01", User = "User2" },
                    new() { Time = "2023-01-15 11:00", Type = "Xuất kho", DocNo = "SO-101", Qty = "-50", Location = "B-02", User = "User2" }
                }
            };
        }
    }

    public class LotDetailViewModel
    {
        public string LotNo { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public DateTime InboundDate { get; set; }
        public List<LotTreeNode> Nodes { get; set; }
        public List<LotTransactionLog> Logs { get; set; }
    }

    public class LotTreeNode
    {
        public string Title { get; set; }
        public string SubTitle { get; set; } // Cho phép xuống dòng
        public string Type { get; set; } // Inbound, Storage, Outbound
    }

    public class LotTransactionLog
    {
        public string Time { get; set; }
        public string Type { get; set; }
        public string DocNo { get; set; }
        public string Qty { get; set; }
        public string Location { get; set; }
        public string User { get; set; }
    }
}
