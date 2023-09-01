namespace BugNet.Data.Entities;

[Table("Permissions", Schema = DataConstants.BugNetSchema)]
public class Permission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    [MaxLength(255)]
    public string Key { get; set; }

    public List<Role> Roles { get; } = new();
}