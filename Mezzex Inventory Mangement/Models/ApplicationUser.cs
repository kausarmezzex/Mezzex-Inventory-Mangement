using Microsoft.AspNetCore.Identity;

namespace Mezzex_Inventory_Mangement.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }

        // New properties
        public string? CountryName { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }
    }
}
