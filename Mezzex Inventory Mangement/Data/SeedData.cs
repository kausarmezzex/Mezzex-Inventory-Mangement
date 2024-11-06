using Mezzex_Inventory_Mangement.Models;
using Microsoft.AspNetCore.Identity;

namespace Mezzex_Inventory_Mangement.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<SeedData>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Starting data seeding...");

                // Seed roles
                await SeedRolesAsync(roleManager, logger);

                // Seed permissions
                await SeedPermissionsAsync(context, logger);

                // Seed users and assign roles and permissions
                await SeedUsersAsync(userManager, roleManager, logger, context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
        {
            string[] roleNames = { "User", "Admin", "Administrator", "Account" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task SeedPermissionsAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.PermissionsName.Any())
            {
                var permissions = new[]
                {
                    new PermissionName { Name = "Create Users" },
                    new PermissionName { Name = "Create Company" },
                };

                await context.PermissionsName.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
                logger.LogInformation("Permissions seeded successfully.");
            }
        }

        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger,
            ApplicationDbContext context)
        {
            await CreateUserAsync(
                userManager, roleManager, logger,
                "islam@direct-pharmacy.co.uk", "Sonaislam@143#",
                "Islam", "Saifi", "Male","India","8723523233", new[] { "User", "Admin", "Administrator", "Account" });

            await CreateUserAsync(
                userManager, roleManager, logger,
                "faizraza349@gmail.com", "Kausar@786",
                "Kausar", "Raza", "Male", "India", "8052738480", new[] { "User", "Admin" });

            await AssignAllPermissionsToAdministrator(userManager, context, logger);
        }

        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger,
            string email,
            string password,
            string firstName,
            string lastName,
            string gender,
            string country,
            string phonenumber,
            string[] roles)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = gender,
                    Active = true,
                    PhoneNumber = phonenumber,
                    CountryName = country,
                };

                var createUserResult = await userManager.CreateAsync(user, password);
                if (createUserResult.Succeeded)
                {
                    logger.LogInformation($"User '{email}' created successfully.");

                    foreach (var role in roles)
                    {
                        if (await roleManager.RoleExistsAsync(role))
                        {
                            await userManager.AddToRoleAsync(user, role);
                            logger.LogInformation($"Role '{role}' assigned to user '{email}'.");
                        }
                        else
                        {
                            logger.LogWarning($"Role '{role}' does not exist.");
                        }
                    }

                    if (roles.Contains("Administrator"))
                    {
                        await AssignClaimsToUser(userManager, user, new[] { "CreateCategory", "CreateBrand", "CreateProduct", "ManageSettings" });
                    }
                }
                else
                {
                    logger.LogError($"Failed to create user '{email}': {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"User '{email}' already exists.");
            }
        }

        private static async Task AssignClaimsToUser(UserManager<ApplicationUser> userManager, ApplicationUser user, string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!(await userManager.GetClaimsAsync(user)).Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    var claimResult = await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Permission", permission));
                    if (claimResult.Succeeded)
                    {
                        // Log success or continue silently
                    }
                    else
                    {
                        // Handle claim assignment failure if necessary
                    }
                }
            }
        }

        private static async Task AssignAllPermissionsToAdministrator(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger logger)
        {
            var administrator = await userManager.FindByEmailAsync("superadmin@example.com");
            if (administrator != null)
            {
                var permissions = context.PermissionsName.Select(p => p.Name).ToArray();
                await AssignClaimsToUser(userManager, administrator, permissions);
                logger.LogInformation("All permissions assigned to Administrator.");
            }
            else
            {
                logger.LogWarning("Administrator user not found.");
            }
        }
    }
}
