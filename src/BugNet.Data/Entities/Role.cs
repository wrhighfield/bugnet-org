namespace BugNet.Data.Entities;

[Table("Roles", Schema = DataConstants.BugNetSchema)]
public class Role
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    [MaxLength(255)]
    public string Key { get; set; }

    public List<Permission> Permissions { get; } = new();
}