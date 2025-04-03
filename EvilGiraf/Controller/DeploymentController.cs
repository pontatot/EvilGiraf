using EvilGiraf.Dto;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvilGiraf.Controller;

[Authorize]
[ApiController]
[Route("api/deploy")]
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
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(string), 404)]
    public async  Task<IActionResult> Status(int id)
    {
        var app = await applicationService.GetApplication(id);
        if (app is null)
            return NotFound($"Application {id} not found");

        var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
        if (deployment is null)
            return NoContent();
        
        return Ok(new DeployResponse(deployment.Status));
    }
    
    [HttpGet("")]
    [ProducesResponseType(typeof(List<DeployResponse>), 200)]
    public async Task<IActionResult> ListDeployments()
    {
        var applications = await applicationService.ListApplications();
        var listDeployResponses = new List<DeployResponse>();
        foreach (var app in applications)
        {
            var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
            if (deployment is not null)
            {
                listDeployResponses.Add(new DeployResponse(deployment.Status));
            }
                
        }
        return Ok(listDeployResponses);
    }
}