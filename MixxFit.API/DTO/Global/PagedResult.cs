namespace MixxFit.API.DTO.Global;

public class PagedResult<T> where T : class
{
    public IReadOnlyList<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCountPaginated { get; set; }
    public int TotalCount { get; set; }

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCountPaginated, int totalCount)
    {
        if (page < 1)
            throw new ArgumentException("Page cannot be 0 or have negative value");
        if (pageSize < 0)
            throw new ArgumentException("Page size cannot be 0 or have negative value");
        
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCountPaginated = totalCountPaginated;
        TotalCount = totalCount;
    }
    
}