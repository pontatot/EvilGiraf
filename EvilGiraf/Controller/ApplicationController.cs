using EvilGiraf.Interface;
using Microsoft.AspNetCore.Mvc;
using EvilGiraf.Dto;
using EvilGiraf.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace EvilGiraf.Controller;

[Authorize]
[ApiController]
[Route("application")]
public class ApplicationController(IApplicationService applicationService) : ControllerBase
{
    [HttpPost("")]
    [ProducesResponseType(typeof(ApplicationResultDto), 201)]
    public async  Task<IActionResult> Create([FromBody] ApplicationCreateDto request)
    {
        var application = await applicationService.CreateApplication(request);

        return Created($"", application.ToDto());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApplicationResultDto), 200)]
    [ProducesResponseType(typeof(string), 404)]
    public async Task<IActionResult> Get(int id)
    {
        var application = await applicationService.GetApplication(id);
        if (application is null)
            return NotFound($"Application {id} not found");

        return Ok(application.ToDto());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(string), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await applicationService.DeleteApplication(id);

        if(response is null)
            return NotFound($"Application {id} not found");

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApplicationResultDto), 200)]
    [ProducesResponseType(typeof(string), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] ApplicationUpdateDto request)
    {
        var application = await applicationService.UpdateApplication(id, request);

        if(application is null)
            return NotFound($"Application {id} not found");

        return Ok(application.ToDto());
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(List<ApplicationResultDto>), 200)]
    public async Task<IActionResult> List()
    {
        var applications = await applicationService.ListApplications();

        return Ok(applications.Select(a => a.ToDto()));
    }
}