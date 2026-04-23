using Microsoft.AspNetCore.Mvc;

namespace Wms.Theme.Web.Components;

public class AlertViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string message, AlertType type = AlertType.Info, bool dismissible = true)
    {
        var model = new AlertViewModel
        {
            Message = message,
            Type = type,
            Dismissible = dismissible
        };
        return View(model);
    }
}

public enum AlertType { Success, Error, Warning, Info }

public class AlertViewModel
{
    public string Message { get; set; } = "";
    public AlertType Type { get; set; } = AlertType.Info;
    public bool Dismissible { get; set; } = true;
}
