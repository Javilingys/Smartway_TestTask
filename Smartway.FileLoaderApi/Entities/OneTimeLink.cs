namespace Smartway.FileLoaderApi.Entities;

public class OneTimeLink
{
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime Expiry { get; set; }
    public bool WasUsed { get; set; }
    public Guid GroupId { get; set; }
}
