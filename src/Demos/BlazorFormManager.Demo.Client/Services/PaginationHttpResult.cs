namespace BlazorFormManager.Demo.Client.Services
{
    public class PaginationHttpResult<TItem>
    {
        public int TotalItemCount { get; set; }
        public TItem[] Items { get; set; }
    }
}
