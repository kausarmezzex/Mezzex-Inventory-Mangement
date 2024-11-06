using Microsoft.AspNetCore.Identity;

namespace Mezzex_Inventory_Mangement.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
