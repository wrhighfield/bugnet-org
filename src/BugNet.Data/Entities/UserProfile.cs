﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugNet.Data.Entities
{
    [Table("UserProfiles", Schema = "BugNet")]
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
        public Guid UserId { get; set; }
        public virtual BugNetUser User { get; set; }
    }
}
