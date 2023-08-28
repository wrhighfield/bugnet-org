// Altairis Web Security Toolkit
// Copyright © Michal A. Valasek - Altairis, 2006-2012 | www.altairis.cz 
// Licensed under terms of Microsoft Permissive License (MS-PL)

using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Profile;

namespace Altairis.Web.Security
{
    [Obsolete(
        "This class is no longer supported and developed. Use SqlTableProfileProvider instead - http://code.msdn.microsoft.com/aspnet4profile")]
    public class SimpleSqlProfileProvider : ProfileProvider
    {
        private const string CUSTOM_PROVIDER_DATA_FORMAT = "^[a-zA-Z0-9_]+;[a-zA-Z0-9_]+(;[0-9]{1,})?$";

        // Initialization and configuration

        private string applicationName, connectionString;

        private System.Collections.Specialized.NameValueCollection configuration;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Validate arguments
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(name)) name = "SimpleSqlProfileProvider";
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Simple SQL profile provider");
            }

            // Initialize base class
            base.Initialize(name, config);

            // Basic init
            configuration = config;
            applicationName = GetConfig("applicationName", "");

            // Initialize connection string
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (connectionStringSettings == null || connectionStringSettings.ConnectionString.Trim() == "")
                throw new ProviderException("Connection string cannot be blank.");
            connectionString = connectionStringSettings.ConnectionString;

            // Initialize table name
            TableName = GetConfig("tableName", "Profiles");

            // Initialize key column name
            KeyColumnName = GetConfig("keyColumnName", "UserName");

            // Initialize last update column name
            LastUpdateColumnName = GetConfig("lastUpdateColumnName", "LastUpdate");
        }

        /// <summary>
        /// Gets or sets the name of the currently running application.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.String"/> that contains the application's shortened name, which does not contain a full path or extension, for example, SimpleAppSettings.</returns>
        public override string ApplicationName
        {
            get => applicationName;
            set => applicationName = value;
        }

        /// <summary>
        /// Gets the name of the database table to store profile data into.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Gets the name of the table column used as primary search key (user name).
        /// </summary>
        /// <value>The name of the key column.</value>
        public string KeyColumnName { get; private set; }

        /// <summary>
        /// Gets the name of the table column used for storing last update time.
        /// </summary>
        /// <value>The name of the last update time column.</value>
        public string LastUpdateColumnName { get; private set; }

        // Profile provider implementation

        /// <summary>
        /// Deletes profile properties and information for profiles that match the supplied list of user names.
        /// </summary>
        /// <param name="usernames">A string array of user names for profiles to be deleted.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteProfiles(string[] usernames)
        {
            if (usernames == null) throw new ArgumentNullException();
            if (usernames.Length == 0) return 0; // no work here

            var count = 0;
            try
            {
                using (HostingEnvironment.Impersonate())
                using (var db = OpenDatabase())
                using (var cmd = new SqlCommand(ExpandCommand("DELETE FROM $Profiles WHERE $UserName=@UserName"), db))
                {
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100);
                    foreach (var userName in usernames)
                    {
                        cmd.Parameters["@UserName"].Value = userName;
                        count += cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        /// <summary>
        /// Deletes profile properties and information for the supplied list of profiles.
        /// </summary>
        /// <param name="profiles">A <see cref="T:System.Web.Profile.ProfileInfoCollection"/>  of information about profiles that are to be deleted.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if (profiles == null) throw new ArgumentNullException();
            if (profiles.Count == 0) return 0; // no work here

            var count = 0;
            try
            {
                using (HostingEnvironment.Impersonate())
                using (var db = OpenDatabase())
                using (var cmd = new SqlCommand(ExpandCommand("DELETE FROM $Profiles WHERE $UserName=@UserName"), db))
                {
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100);
                    foreach (ProfileInfo pi in profiles)
                    {
                        cmd.Parameters["@UserName"].Value = pi.UserName;
                        count += cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        /// <summary>
        /// Returns the collection of settings property values for the specified application instance and settings property group.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application use.</param>
        /// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyCollection"/> containing the settings property group whose values are to be retrieved.</param>
        /// <returns>
        /// A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> containing the values for the specified settings property group.
        /// </returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context,
            SettingsPropertyCollection collection)
        {
            var svc = new SettingsPropertyValueCollection();

            // Validate arguments
            if (collection.Count < 1) return svc;
            var userName = (string) context["UserName"];
            if (string.IsNullOrEmpty(userName)) return svc;

            using (var dt = new DataTable())
            {
                // Get profile row from db
                using (HostingEnvironment.Impersonate())
                using (var db = OpenDatabase())
                using (var cmd = new SqlCommand(ExpandCommand("SELECT * FROM $Profiles WHERE $UserName=@UserName"), db))
                {
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = userName;
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                // Process properties
                foreach (SettingsProperty prop in collection)
                {
                    var value = new SettingsPropertyValue(prop);
                    if (dt.Rows.Count == 0)
                    {
                        value.PropertyValue = value.Property.PropertyType == typeof(DateTime)
                            ? null
                            : Convert.ChangeType(value.Property.DefaultValue, value.Property.PropertyType);

                        value.IsDirty = false;
                        value.Deserialized = true;
                    }
                    else
                    {
                        var columnName = GetPropertyMapInfo(prop).ColumnName;
                        if (dt.Columns.IndexOf(columnName) == -1)
                            throw new ProviderException(
                                $"Column '{columnName}' required for property '{prop.Name}' was not found in table '{TableName}'.");
                        var columnValue = dt.Rows[0][columnName];

                        value.IsDirty = false;
                        value.Deserialized = true;
                        if (!(columnValue is DBNull || columnValue == null))
                            value.PropertyValue = columnValue;
                        else
                            value.PropertyValue = null;
                    }

                    svc.Add(value);
                }
            }

            return svc;
        }

        /// <summary>
        /// Sets the values of the specified group of property settings.
        /// </summary>
        /// <param name="context">A <see cref="T:System.Configuration.SettingsContext"/> describing the current application usage.</param>
        /// <param name="collection">A <see cref="T:System.Configuration.SettingsPropertyValueCollection"/> representing the group of property settings to set.</param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            // Validate arguments
            if (!(bool) context["IsAuthenticated"])
                throw new NotSupportedException("This provider does not support anonymous profiles");
            var userName = (string) context["UserName"];
            if (string.IsNullOrEmpty(userName) || collection.Count == 0 || !HasDirtyProperties(collection))
                return; // no work here

            // Construct command
            using (var cmd = new SqlCommand())
            {
                var insertCommandText1 = new StringBuilder("INSERT INTO $Profiles ($UserName, $LastUpdate");
                var insertCommandText2 = new StringBuilder(" VALUES (@UserName, GETDATE()");
                var updateCommandText = new StringBuilder("UPDATE $Profiles SET $LastUpdate=GETDATE()");
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = userName;

                // Cycle trough collection
                var i = 0;
                foreach (SettingsPropertyValue propVal in collection)
                {
                    var pmi = GetPropertyMapInfo(propVal.Property);

                    // Always add parameter
                    var p = new SqlParameter("@Param" + i, pmi.Type);
                    if (pmi.Length != 0) p.Size = pmi.Length;
                    if (propVal.Deserialized && propVal.PropertyValue == null) p.Value = DBNull.Value;
                    else p.Value = propVal.PropertyValue;
                    cmd.Parameters.Add(p);

                    // Always add to insert
                    insertCommandText1.Append(", " + pmi.ColumnName);
                    insertCommandText2.Append(", @Param" + i);

                    // Add dirty properties to update
                    if (propVal.IsDirty) updateCommandText.Append(", " + pmi.ColumnName + "=@Param" + i);

                    i++;
                }

                // Complete command
                insertCommandText1.Append(")");
                insertCommandText2.Append(")");
                updateCommandText.Append(" WHERE $UserName=@UserName");
                cmd.CommandText = ExpandCommand("IF EXISTS (SELECT * FROM $Profiles WHERE $UserName=@UserName) BEGIN " +
                                                updateCommandText + " END ELSE BEGIN " + insertCommandText1 +
                                                insertCommandText2 + " END");

                // Execute command
                using (HostingEnvironment.Impersonate())
                using (var db = OpenDatabase())
                {
                    cmd.Connection = db;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Retrieves profile information for profiles in which the user name matches the specified user names.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
        /// </returns>
        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption,
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            // Validate arguments
            if (pageIndex < 0) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (authenticationOption == ProfileAuthenticationOption.Anonymous)
            {
                // Anonymous profiles not supported
                totalRecords = 0;
                return new ProfileInfoCollection();
            }

            using (var dt = new DataTable())
            {
                // Prepare sql command
                using (var db = OpenDatabase())
                using (var cmd = new SqlCommand("", db))
                {
                    if (string.IsNullOrEmpty(usernameToMatch))
                    {
                        cmd.CommandText =
                            ExpandCommand(
                                "SELECT $UserName AS UserName, $LastUpdate AS LastUpdate FROM $Profiles WHERE $UserName=@UserName ORDER BY $UserName");
                    }
                    else
                    {
                        cmd.CommandText =
                            ExpandCommand(
                                "SELECT $UserName AS UserName, $LastUpdate AS LastUpdate FROM $Profiles WHERE $UserName=@UserName ORDER BY $UserName");
                        cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = usernameToMatch;
                    }

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                // Prepare paging
                var pic = new ProfileInfoCollection();
                totalRecords = dt.Rows.Count;
                var minIndex = pageIndex * pageSize;
                if (minIndex > totalRecords - 1) return pic;
                var maxIndex = minIndex + pageSize - 1;
                if (maxIndex > totalRecords - 1) maxIndex = totalRecords - 1;

                // Populate collection from data table
                for (var i = minIndex; i <= maxIndex; i++)
                    pic.Add(new ProfileInfo(Convert.ToString(dt.Rows[i]["UserName"]),
                        false,
                        DateTime.Now,
                        Convert.ToDateTime(dt.Rows[i]["LastUpdate"]),
                        0));
                return pic;
            }
        }

        /// <summary>
        /// Retrieves user profile data for all profiles in the data source.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information for all profiles in the data source.
        /// </returns>
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption,
            int pageIndex, int pageSize, out int totalRecords)
        {
            return FindProfilesByUserName(authenticationOption, string.Empty, pageIndex, pageSize, out totalRecords);
        }

        // Private support functions

        private struct PropertyMapInfo
        {
            public string ColumnName;
            public SqlDbType Type;
            public int Length;
        }

        /// <summary>
        /// Gets information about how the property value is stored in database (column name, type, size).
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        private PropertyMapInfo GetPropertyMapInfo(SettingsProperty prop)
        {
            // Perform general validation
            if (prop == null) throw new ArgumentNullException();
            var cpd = Convert.ToString(prop.Attributes["CustomProviderData"]);
            if (string.IsNullOrEmpty(cpd))
                throw new ProviderException(
                    $"CustomProviderData is missing or empty for property {prop.Name}.");
            if (!System.Text.RegularExpressions.Regex.IsMatch(cpd, CUSTOM_PROVIDER_DATA_FORMAT))
                throw new ProviderException(
                    $"Invalid format of CustomProviderData for property {prop.Name}.");
            var parts = cpd.Split(';');

            var pmi = new PropertyMapInfo
            {
                ColumnName = parts[0]
            };
            try
            {
                pmi.Type = (SqlDbType) Enum.Parse(typeof(SqlDbType), parts[1], true);
            }
            catch
            {
                throw new ProviderException($"SqlDbType '{parts[1]}' specified for property {prop.Name} is invalid.");
            }

            if (parts.Length == 3) pmi.Length = Convert.ToInt32(parts[2]);
            return pmi;
        }

        /// <summary>
        /// Determines whether property collection contains dirty properties.
        /// </summary>
        /// <param name="props">The property collection.</param>
        /// <returns>
        /// 	<c>true</c> if collection has dirty properties; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasDirtyProperties(SettingsPropertyValueCollection props)
        {
            return props.Cast<SettingsPropertyValue>().Any(prop => prop.IsDirty);
        }

        /// <summary>
        /// Expands the SQL command placeholders ($Something) with configured name.
        /// </summary>
        /// <param name="sql">The SQL command text.</param>
        /// <returns>Expanded SQL command text.</returns>
        private string ExpandCommand(string sql)
        {
            sql = sql.Replace("$Profiles", TableName);
            sql = sql.Replace("$UserName", KeyColumnName);
            sql = sql.Replace("$LastUpdate", LastUpdateColumnName);
            return sql;
        }

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        /// <returns></returns>
        private SqlConnection OpenDatabase()
        {
            var db = new SqlConnection(connectionString);
            db.Open();
            return db;
        }

        /// <summary>
        /// Reads configuration value. If not present, returns default value.
        /// </summary>
        /// <param name="name">The configuration property name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        private string GetConfig(string name, string defaultValue)
        {
            // Validate input arguments
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Name");

            // Get value from configuration
            var Value = configuration[name];
            if (string.IsNullOrEmpty(Value)) Value = defaultValue;
            return Value;
        }

        #region Inactive profiles - not implemented

        /// <summary>
        /// When overridden in a derived class, deletes all user-profile data for profiles in which the last activity date occurred before the specified date.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are deleted.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <returns>
        /// The number of profiles deleted from the data source.
        /// </returns>
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption,
            DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, retrieves profile information for profiles in which the last activity date occurred on or before the specified date and the user name matches the specified user name.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/> value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user profile information for inactive profiles where the user name matches the supplied <paramref name="usernameToMatch"/> parameter.
        /// </returns>
        public override ProfileInfoCollection FindInactiveProfilesByUserName(
            ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate,
            int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, retrieves user-profile data from the data source for profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <param name="pageIndex">The index of the page of results to return.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">When this method returns, contains the total number of profiles.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Profile.ProfileInfoCollection"/> containing user-profile information about the inactive profiles.
        /// </returns>
        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption,
            DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, returns the number of profiles in which the last activity date occurred on or before the specified date.
        /// </summary>
        /// <param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"/> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
        /// <param name="userInactiveSinceDate">A <see cref="T:System.DateTime"/> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"/>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
        /// <returns>
        /// The number of profiles in which the last activity date occurred on or before the specified date.
        /// </returns>
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption,
            DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}