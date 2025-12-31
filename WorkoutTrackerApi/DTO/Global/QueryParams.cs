namespace WorkoutTrackerApi.DTO.Global;

public class QueryParams
{
    public int Page { get; set; }
    public int PageSize { get; set; } 
    public string? Search { get; set; }
    public string Sort { get; set; }

    public QueryParams(int page, int pageSize, string? search, string sort)
    {
        Page = page;
        PageSize = pageSize;
        Search = search;
        Sort = sort;
    }
}