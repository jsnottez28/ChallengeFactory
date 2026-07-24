using Microsoft.AspNetCore.Identity;

namespace Web.Data;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
