namespace BugNET.HttpModules
{
    /// <summary>
    /// A class to store the users properties fetched from an Active Directory
    /// </summary>
    public class UserProperties
    {
        /// <summary>
        /// Gets or sets the name of the first.
        /// </summary>
        /// <value>The name of the first.</value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the last.
        /// </summary>
        /// <value>The name of the last.</value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        public string DisplayName { get; set; }
    }
}
