namespace Wms.Theme.Web.Model.ShareModel;

public class ResultModel<T>
{
    public bool IsSuccess { get; set; }
    public int Code { get; set; }
    public string ErrorMessage { get; set; } = "";
    public T Data { get; set; }
}

public class PageData<T>
{
    public List<T> Rows { get; set; } = [];
    public int Totals { get; set; }
}
