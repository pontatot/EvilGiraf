using EvilGiraf.Dto;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvilGiraf.Controller;

[Authorize]
[ApiController]
[Route("deploy")]
public class DeploymentController(IDeploymentService deploymentService, IApplicationService applicationService, IKubernetesService kubernetesService) : ControllerBase
{
    [HttpPost("{id:int}")]
    [ProducesResponseType(201)]
    [ProducesResponseType(typeof(string), 404)]
    public async  Task<IActionResult> Deploy(int id)
    {
        var app = await applicationService.GetApplication(id);
        if (app is null)
            return NotFound($"Application {id} not found");
        
        _ = kubernetesService.Deploy(app);
        
        return Created($"deploy/{id:int}", null);
    }
    
    [HttpGet("{id:int}")]
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
}