using EvilGiraf.Dto;
using EvilGiraf.Model;
using Microsoft.AspNetCore.Mvc;

namespace EvilGiraf.Controllers;

[ApiController]
[Route("")]
public class ApplicationController : ControllerBase
{
    [HttpPost("deploy/docker")]
    [ProducesResponseType(typeof(DeployResponse), 201)]
    public IActionResult DeployDocker([FromBody] DeployDockerRequest deployDockerRequest)
    {
        if (string.IsNullOrEmpty(deployDockerRequest.Name) || string.IsNullOrEmpty(deployDockerRequest.Link))
        {
            return BadRequest("Name and Link are required.");
        }
        return Created("/deploy/docker", new DeployResponse(ApplicationStatus.Running));
    }
}