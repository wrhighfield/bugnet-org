namespace BugNet.Data;

public class ApplicationRole : IdentityRole<Guid>
{
	public override string ToString() => $"AspNetRole: [{Name}:{Id}]";
}

public class ApplicationUser : IdentityUser<Guid>
{
	public override string ToString() => $"AspNetUser: [{Email}:{Id}]";
}