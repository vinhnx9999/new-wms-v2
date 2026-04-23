namespace Wms.Theme.Web.Model.ShareModel
{
    public class PageSearchRequest
    {
        public int pageIndex { get; set; } = 1;

        /// <summary>
        /// rows per page
        /// </summary>
        public int pageSize { get; set; } = 20;

        /// <summary>
        /// Custom Classification
        /// </summary>
        public string sqlTitle { get; set; } = "";

        /// <summary>
        /// search condition
        /// </summary>
        public List<SearchObject> searchObjects { get; set; } = new List<SearchObject>();

    }


    // Enum condition

    public class SearchObject
    {
        /// <summary>
        /// sort
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// label
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// type
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// operator
        /// </summary>
        public Operators Operator { get; set; } = Operators.Equal;

        /// <summary>
        /// text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// value
        /// </summary>
        public object Value { get; set; } = new object();

        /// <summary>
        /// select item combox list
        /// </summary>
        public List<ComboxItem> comboxItem { get; set; } = new List<ComboxItem>();

        /// <summary>
        /// Group Object
        /// </summary>
        public string Group { get; set; } = string.Empty;

    }

    public enum Operators
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Equal =
        /// </summary>
        Equal = 1,
        /// <summary>
        /// GreaterThan 
        /// </summary>
        GreaterThan = 2,
        /// <summary>
        /// GreaterThanOrEqual 
        /// </summary>
        GreaterThanOrEqual = 3,
        /// <summary>
        /// LessThan
        /// </summary>
        LessThan = 4,
        /// <summary>
        /// LessThanOrEqual
        /// </summary>
        LessThanOrEqual = 5,
        /// <summary>
        /// Contains
        /// </summary>
        Contains = 6
    }
    /// <summary>
    /// Condition
    /// </summary>
    public enum Condition
    {
        /// <summary>
        /// OR
        /// </summary>
        OrElse = 1,
        /// <summary>
        /// AND
        /// </summary>
        AndAlso = 2
    }
    /// <summary>
    /// select item combox
    /// </summary>
    public class ComboxItem
    {
        /// <summary>
        /// value
        /// </summary>
        public string value { get; set; } = string.Empty;
        /// <summary>
        /// text
        /// </summary>
        public string text { get; set; } = string.Empty;
    }
}
