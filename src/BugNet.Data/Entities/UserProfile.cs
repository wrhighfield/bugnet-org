namespace BugNet.Data.Entities
{
    [Table("UserProfiles", Schema = DataConstants.BugNetSchema)]
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        [MaxLength(255)]
        public string DisplayName { get; set; }

        [ForeignKey("AspNetUser")]
        [Required]
        public Guid UserId { get; set; }
    }
}
