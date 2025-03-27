using EvilGiraf.Dto;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvilGiraf.Controller;

[Authorize]
[ApiController]
[Route("")]
public class DeploymentController(IDeploymentService deploymentService, IApplicationService applicationService, IKubernetesService kubernetesService) : ControllerBase
{
    [HttpPost("application/{id:int}/deploy")]
    [ProducesResponseType(201)]
    [ProducesResponseType(typeof(string), 404)]
    public async  Task<IActionResult> Deploy(int id)
    {
        var app = await applicationService.GetApplication(id);
        if (app is null)
            return NotFound($"Application {id} not found");
        
        _ = kubernetesService.Deploy(app);
        
        return Created($"application/{id:int}/deploy", null);
    }
    
    [HttpGet("application/{id:int}/deploy")]
    [ProducesResponseType(typeof(DeployResponse), 200)]
    [ProducesResponseType(typeof(string), 404)]
    public async  Task<IActionResult> Status(int id)
    {
        var app = await applicationService.GetApplication(id);
        if (app is null)
            return NotFound($"Application {id} not found");

        var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
        if (deployment is null)
            return NotFound($"Application {id} is not deployed");
        
        return Ok(new DeployResponse(deployment.Status));
    }
    
    [HttpGet("list/{namespace}")]
    public async Task<IActionResult> ListDeployments(string @namespace)
    {
        var deployments = await deploymentService.ListDeployments(@namespace);
        return Ok(deployments);
    }
}