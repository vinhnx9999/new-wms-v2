using Microsoft.AspNetCore.Mvc;

namespace Wms.Theme.Web.Components;

public class DataTableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(DataTableViewModel model)
    {
        model.ActionButtons ??= new List<ActionButton>();
        model.RowActions ??= new List<RowAction>();

        if (model.TotalRecords > 0 && model.PageSize > 0)
        {
            model.TotalPages = (int)Math.Ceiling((double)model.TotalRecords / model.PageSize);
        }

        return View(model);
    }
}

public class DataTableViewModel
{
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    public string Title { get; set; } = "";
    public bool ShowPagination { get; set; } = true;
    public int PageSize { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;
    public int TotalRecords { get; set; } = 0;
    public int TotalPages { get; set; } = 0;

    public List<ActionButton>? ActionButtons { get; set; } = new();
    public List<RowAction>? RowActions { get; set; } = new();
    public Dictionary<string, Dictionary<string, StatusBadge>> StatusMappings { get; set; } = new();
    public Dictionary<string, string> CheckboxAttributes { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> ColumnTypes { get; set; } = new();
    public Dictionary<string, string> ColumnClasses { get; set; } = new();
}

public class StatusBadge
{
    public string Text { get; set; } = "";
    public string CssClass { get; set; } = "bg-gray-100 text-gray-800";
}

public class ActionButton
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Href { get; set; } = "#";
    public string CssClass { get; set; } = "px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition";
    public string? OnClick { get; set; }
    public string Target { get; set; } = "_self";
}

public class RowAction
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string Href { get; set; } = "#";
    public string CssClass { get; set; } = "text-blue-600 hover:text-blue-900 text-sm font-medium";
    public string? OnClick { get; set; }
    public string Target { get; set; } = "_self";
}

