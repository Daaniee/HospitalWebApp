using System.ComponentModel.DataAnnotations;

namespace hospitalwebapp.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<Staff> Staffs { get; set; }
        public ICollection<Permission> Permissions { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}