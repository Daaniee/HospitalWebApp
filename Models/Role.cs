using System.ComponentModel.DataAnnotations;

namespace hospitalwebapp.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public ICollection<RolePermission> RolePermissions { get; set; }  = new List<RolePermission>();
    }
}