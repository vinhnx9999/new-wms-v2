using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Wms.Theme.Web.Pages.Inventory.Adjustment
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public AdjustmentEditViewModel Input { get; set; } = new();

        // Danh sách Dropdown
        public List<SelectListItem> Warehouses { get; set; } = new();
        public List<SelectListItem> Reasons { get; set; } = new();

        public void OnGet(int id)
        {
            // 1. Khởi tạo dữ liệu cho Dropdown
            Warehouses = new List<SelectListItem>
            {
                new SelectListItem { Value = "W01", Text = "Kho A - Main Warehouse" },
                new SelectListItem { Value = "W02", Text = "Kho B - Phụ liệu" }
            };

            Reasons = new List<SelectListItem>
            {
                new SelectListItem { Value = "Damaged", Text = "Hư hỏng/Damaged" },
                new SelectListItem { Value = "Lost", Text = "Thất lạc/Lost" },
                new SelectListItem { Value = "Found", Text = "Tìm thấy/Found" }
            };

            // 2. MOCK DATA: Giả lập lấy dữ liệu từ DB dựa trên ID = 123 (như trong ảnh)
            // Thực tế: var data = _service.GetById(id);
            Input = new AdjustmentEditViewModel
            {
                Id = id,
                WarehouseId = "W02",
                Reason = "Found",
                Note = "Cập nhật lại số lượng kiểm kê kho phụ liệu.",
                Items = new List<AdjustmentEditItemViewModel> // <--- ĐÃ SỬA TÊN
                {
                    new AdjustmentEditItemViewModel
                    {
                        Sku = "SKU-8888: Áo thun cotton",
                        Bin = "Bin A-01",
                        Lot = "Lot L1",
                        SystemQty = 50,
                        ActualQty = 52
                    },
                    new AdjustmentEditItemViewModel
                    {
                        Sku = "SKU-9999: Quần jeans",
                        Bin = "Bin B-05",
                        Lot = "Lot L2",
                        SystemQty = 20,
                        ActualQty = 18
                    }
                }
            };
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                // Nếu lỗi thì load lại dropdown để không bị null trên view
                OnGet(Input.Id);
                return Page();
            }

            // Logic lưu vào Database...
            // _service.Update(Input);

            return RedirectToPage("./Index"); // Quay về danh sách
        }
    }

    // --- VIEW MODELS ---
    public class AdjustmentEditViewModel
    {
        public int Id { get; set; }
        public string WarehouseId { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
        // Sử dụng List của Item Edit
        public List<AdjustmentEditItemViewModel> Items { get; set; } = new();
    }

    public class AdjustmentEditItemViewModel
    {
        public string Sku { get; set; }
        public string Bin { get; set; }
        public string Lot { get; set; }
        public int SystemQty { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int? ActualQty { get; set; }
    }
}


