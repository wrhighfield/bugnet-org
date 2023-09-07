namespace BugNet.Data.Entities;

[Table(DataConstants.LogsTableName, Schema = DataConstants.BugNetSchema)]
public class Log
{
    [Key]
    public int Id { get; set; }
    public string Message { get; set; }
    public string MessageTemplate { get; set; }
    public string Level { get; set; }
    public DateTime TimeStamp { get; set; }
    [MaxLength(255)]
    public string UserName { get; set; }
    [MaxLength(55)]
    public string IpAddress { get; set; }
    [MaxLength(1000)]
    public string Resource { get; set; }
    [MaxLength(1000)]
	public string SourceContext { get; set; }
	public string Exception { get; set; }
    public string Properties { get; set; }
}