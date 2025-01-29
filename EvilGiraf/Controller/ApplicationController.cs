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

    [HttpPost("deploy/github")]
    [ProducesResponseType(typeof(DeployResponse), 201)]
    public IActionResult DeployGithub([FromBody] DeployGithubRequest deployGithubRequest)
    {
        if (string.IsNullOrEmpty(deployGithubRequest.Name) || string.IsNullOrEmpty(deployGithubRequest.Link))
        {
            return BadRequest("Name and Link are required.");
        }
        return Created("/deploy/github", new DeployResponse(ApplicationStatus.Running));
    }

    [HttpPost("delete")]
    [ProducesResponseType(204)]
    public IActionResult DeleteDeployment([FromBody] DeleteDeploymentRequest deleteDeploymentRequest)
    {
        if (string.IsNullOrEmpty(deleteDeploymentRequest.Name) || string.IsNullOrEmpty(deleteDeploymentRequest.Namespace))
        {
            return BadRequest("Name and Namespace are required.");
        }
        return Created("/delete", new DeleteResponse());
    }
}