using EvilGiraf.Dto;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using Microsoft.AspNetCore.Mvc;

namespace EvilGiraf.Controller;

[ApiController]
[Route("")]
public class DeploymentController(IDeploymentService deploymentService, IApplicationService applicationService) : ControllerBase
{
    [HttpPost("application/{id:int}/deploy")]
    [ProducesResponseType(typeof(DeployResponse), 201)]
    [ProducesResponseType(typeof(string), 404)]
    public async  Task<IActionResult> Deploy(int id)
    {
        var app = await applicationService.GetApplication(id);
        if (app is null)
            return NotFound($"Application {id} not found");

        var deployment = await deploymentService.ReadDeployment(app.Name, app.Id.ToNamespace());
        if (deployment is null)
        {
            deployment = await deploymentService.CreateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                app.Link, []));
        }
        else
        {
            await deploymentService.UpdateDeployment(new DeploymentModel(app.Name, app.Id.ToNamespace(), 1,
                app.Link, []));
        }
        
        return Created($"application/{id:int}/deploy", new DeployResponse(deployment.Status));
    }
}