namespace MixxFit.API.DTO.Global;

public class QueryParams
{
    public int Page { get; set; }
    public string Search { get; set; }
    public string Sort { get; set; }
    public DateTime? Date { get; set; }

    public QueryParams(int page, string search, string sort, DateTime? date)
    {
        Page = page;
        Search = search;
        Sort = sort.ToLower();
        Date = date;
    }
}