namespace Wms.Theme.Web.Model.Category;

public class CategoryViewModel
{    
    public int id { get; set; } = 0;

    public string category_name { get; set; } = string.Empty;


    public int parent_id { get; set; } = 0;


    public string creator { get; set; } = string.Empty;


    public DateTime create_time { get; set; }

    public DateTime last_update_time { get; set; }

    public bool is_valid { get; set; } = true;

}
