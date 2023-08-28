using System;
using System.Collections.Generic;
using System.Web;
using BugNET.DAL;
using BugNET.Entities;
using BugNET.Common;
using log4net;

namespace BugNET.BLL
{
    public static class RoleManager
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Saves the role object
        /// </summary>
        /// <returns><c>true</c> if successful</returns>
        public static bool SaveOrUpdate(Role entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.ProjectId <= Globals.NewId)
                throw new ArgumentException("Cannot save role, the project id is invalid");
            if (string.IsNullOrEmpty(entity.Name)) throw new ArgumentException("The role name cannot be empty or null");

            if (entity.Id > Globals.NewId)
                return DataProviderManager.Provider.UpdateRole(entity);

            var tempId = DataProviderManager.Provider.CreateNewRole(entity);
            if (tempId <= 0) return false;
            entity.Id = tempId;
            return true;
        }

        private const string ROLE_PERMISSION_CACHE = "RolePermission";

        /// <summary>
        /// Associates the default roles created at installation to a project.
        /// </summary>
        /// <param name="projectId">The project id.</param>
        public static void CreateDefaultProjectRoles(int projectId)
        {
            if (projectId <= Globals.NewId)
                throw new ArgumentOutOfRangeException(nameof(projectId));

            foreach (var role in Globals.DefaultRoles)
            {
                var r = new Role {ProjectId = projectId, Name = role, Description = role, AutoAssign = false};
                var newRoleId = DataProviderManager.Provider.CreateNewRole(r);

                int[] permissions = null;
                //add permissions to roles
                switch (role)
                {
                    case "Project Administrators":
                        permissions = Globals.AdministratorPermissions;
                        break;
                    case "Read Only":
                        permissions = Globals.ReadOnlyPermissions;
                        break;
                    case "Reporter":
                        permissions = Globals.ReporterPermissions;
                        break;
                    case "Developer":
                        permissions = Globals.DeveloperPermissions;
                        break;
                    case "Quality Assurance":
                        permissions = Globals.QualityAssurancePermissions;
                        break;
                }

                if (permissions == null) continue;
                foreach (var i in permissions) AddPermission(newRoleId, i);
            }
        }

        /// <summary>
        /// Get all roles by project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>List of role objects</returns>
        public static List<Role> GetByProjectId(int projectId)
        {
            if (projectId <= Globals.NewId)
                throw new ArgumentOutOfRangeException(nameof(projectId));

            return DataProviderManager.Provider.GetRolesByProject(projectId);
        }

        /// <summary>
        /// Gets the role by id.
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <returns></returns>
        public static Role GetById(int roleId)
        {
            if (roleId <= Globals.NewId)
                throw new ArgumentOutOfRangeException(nameof(roleId));

            return DataProviderManager.Provider.GetRoleById(roleId);
        }

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="projectId">The project id.</param>
        /// <param name="description">The description.</param>
        /// <param name="autoAssign">if set to <c>true</c> [auto assign].</param>
        /// <returns></returns>
        public static int CreateRole(string roleName, int projectId, string description, bool autoAssign)
        {
            if (Exists(roleName, projectId)) return 0;
            var r = new Role
                {ProjectId = projectId, Name = roleName, Description = description, AutoAssign = autoAssign};
            SaveOrUpdate(r);
            return r.Id;
        }

        /// <summary>
        /// Roles the exists.
        /// </summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        private static bool Exists(string roleName, int projectId)
        {
            if (projectId <= 0) throw new ArgumentOutOfRangeException(nameof(projectId));
            if (string.IsNullOrEmpty(roleName)) throw new ArgumentOutOfRangeException(nameof(roleName));

            return DataProviderManager.Provider.RoleExists(roleName, projectId);
        }

        /// <summary>
        /// Gets the roles for user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="projectId">The project id.</param>
        /// <returns></returns>
        public static List<Role> GetForUser(string userName, int projectId)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
                return DataProviderManager.Provider.GetRolesByUserName(userName, projectId);

            // performance enhancement
            // WRH 2012-04-06
            // use the current loaded user roles if we are looking at the same user
            return userName.ToLower().Equals(HttpContext.Current.User.Identity.Name.ToLower())
                ? CurrentUserRoles.FindAll(p => p.ProjectId == projectId)
                : DataProviderManager.Provider.GetRolesByUserName(userName, projectId);
        }

        /// <summary>
        /// Gets the roles for user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static List<Role> GetForUser(string userName)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
                return DataProviderManager.Provider.GetRolesByUserName(userName);

            // performance enhancement
            // WRH 2012-04-06
            // use the current loaded user roles if we are looking at the same user
            return userName.ToLower().Equals(HttpContext.Current.User.Identity.Name.ToLower())
                ? CurrentUserRoles
                : DataProviderManager.Provider.GetRolesByUserName(userName);
        }

        /// <summary>
        /// Gets all roles.
        /// </summary>
        /// <returns></returns>
        public static List<Role> GetAll()
        {
            return DataProviderManager.Provider.GetAllRoles();
        }

        /// <summary>
        /// Adds a user to a role
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="roleId">The role id.</param>
        public static void AddUser(string userName, int roleId)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));
            if (roleId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(roleId));

            DataProviderManager.Provider.AddUserToRole(userName, roleId);
            HttpContext.Current.Cache.Remove(ROLE_PERMISSION_CACHE);
        }

        /// <summary>
        /// Removes a user from a role
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="roleId">The role id.</param>
        public static void RemoveUser(string userName, int roleId)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentOutOfRangeException(nameof(userName));
            if (roleId <= 0) throw new ArgumentOutOfRangeException(nameof(roleId));

            DataProviderManager.Provider.RemoveUserFromRole(userName, roleId);
            HttpContext.Current.Cache.Remove(ROLE_PERMISSION_CACHE);
        }

        /// <summary>
        /// Deletes the role.
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <returns></returns>
        public static bool Delete(int roleId)
        {
            if (roleId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(roleId));

            if (!DataProviderManager.Provider.DeleteRole(roleId)) return false;
            HttpContext.Current.Cache.Remove(ROLE_PERMISSION_CACHE);
            return true;
        }

        /// <summary>
        /// Retrieves the Role Permissions DataView from the cache if exists, otherwise loads 
        /// it into the cache
        /// </summary>
        /// <returns>Role Permissions DataView</returns>
        private static List<RolePermission> GetPermissions()
        {
            var permissions = (List<RolePermission>) HttpContext.Current.Cache[ROLE_PERMISSION_CACHE];

            if (permissions != null) return permissions;
            permissions = DataProviderManager.Provider.GetRolePermissions();
            HttpContext.Current.Cache.Insert(ROLE_PERMISSION_CACHE, permissions);

            return permissions;
        }

        /// <summary>
        /// Checks the Role Permission DataView if a permission row exists
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="role"></param>
        /// <param name="permissionKey"></param>
        /// <returns>[true] if row exists</returns>
        public static bool HasPermission(int projectId, string role, string permissionKey)
        {
            //check if the role for a project has permission
            var permission = GetPermissions().Find(
                p => p.ProjectId == projectId && p.RoleName == role && p.PermissionKey == permissionKey);

            return permission != null;
        }

        /// <summary>
        /// Gets all permissions by role
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <returns>List of permission objects</returns>
        public static IEnumerable<Entities.Permission> GetPermissionsById(int roleId)
        {
            if (roleId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(roleId));

            return DataProviderManager.Provider.GetPermissionsByRoleId(roleId);
        }

        /// <summary>
        /// Deletes a permission object from a role
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <param name="permissionId">The permission id.</param>
        /// <returns>[true] if successful</returns>
        public static bool DeletePermission(int roleId, int permissionId)
        {
            if (roleId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(roleId));
            if (permissionId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(permissionId));

            if (!DataProviderManager.Provider.DeletePermission(roleId, permissionId)) return false;
            HttpContext.Current.Cache.Remove(ROLE_PERMISSION_CACHE);
            return true;
        }

        /// <summary>
        /// Adds a permission object to a role
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <param name="permissionId">The permission id.</param>
        /// <returns>[true] if successful</returns>
        public static bool AddPermission(int roleId, int permissionId)
        {
            if (roleId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(roleId));
            if (permissionId <= Globals.NewId) throw new ArgumentOutOfRangeException(nameof(permissionId));

            if (!DataProviderManager.Provider.AddPermission(roleId, permissionId)) return false;
            HttpContext.Current.Cache.Remove(ROLE_PERMISSION_CACHE);
            return true;
        }

        private const string CURRENT_USER_ROLES = "CURRENT_USER_ROLES";

        /// <summary>
        /// performance enhancement
        /// WRH 2012-04-06
        /// Singleton pattern for the current users roles
        /// We load them the first time and keep them around for the length of the request
        /// </summary>
        private static List<Role> CurrentUserRoles
        {
            get
            {
                var ctx = HttpContext.Current;
                if (ctx == null) return null;


                if (ctx.Items[CURRENT_USER_ROLES] is List<Role> items) return items;
                var roles = DataProviderManager.Provider.GetRolesByUserName(ctx.User.Identity.Name);
                CurrentUserRoles = roles;
                return roles;
            }
            set
            {
                var ctx = HttpContext.Current;
                if (ctx == null) return;

                ctx.Items[CURRENT_USER_ROLES] = value;
            }
        }
    }
}