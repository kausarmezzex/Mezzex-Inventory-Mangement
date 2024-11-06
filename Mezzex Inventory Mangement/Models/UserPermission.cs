﻿namespace Mezzex_Inventory_Mangement.Models
{
    public class UserPermission
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }

        public ApplicationUser User { get; set; }
        public PermissionName Permission { get; set; }
    }
}
