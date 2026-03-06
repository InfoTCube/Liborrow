using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.TestHelpers;

public abstract class TestBase
{
    protected Guid TestUserId { get; } = Guid.NewGuid();
    protected const string TestUsername = "test_user";

    protected void SetupFakeUser(ControllerBase controller, Guid? userId = null, string? userName = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, (userId ?? TestUserId).ToString()),
            new Claim(ClaimTypes.Name, userName ?? TestUsername)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }
}