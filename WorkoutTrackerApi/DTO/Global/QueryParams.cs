namespace WorkoutTrackerApi.DTO.Global;

public class QueryParams
{
    public int Page { get; set; }
    public int PageSize { get; set; } 
    public string Search { get; set; }
    public string Sort { get; set; }
    public DateTime? Date { get; set; }

    public QueryParams(int page, int pageSize, string search, string sort, DateTime? date)
    {
        Page = page;
        PageSize = pageSize;
        Search = search;
        Sort = sort;
        Date = date;
    }
}