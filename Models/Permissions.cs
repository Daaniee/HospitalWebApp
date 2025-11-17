using System.ComponentModel.DataAnnotations;

namespace hospitalwebapp.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Action { get; set; }

        // 🔗 Navigation for many-to-many
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
