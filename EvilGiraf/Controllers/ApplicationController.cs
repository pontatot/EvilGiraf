using Microsoft.AspNetCore.Mvc;

namespace EvilGiraf.Controllers;

[ApiController]
[Route("")]
public class ApplicationController : ControllerBase
{
    [HttpPost("deploy/docker")]
    public IActionResult DeployDocker([FromBody] DeployRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Link))
        {
            return BadRequest("Name and Link are required.");
        }
        return Created("/deploy/docker", new { Status = "Running" });
    }
}

public record DeployRequest(
    string Name,
    string Link,
    string? Secret,
    string? Version
);
