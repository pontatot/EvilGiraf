using EvilGiraf.Interface;
using Microsoft.AspNetCore.Mvc;
using EvilGiraf.Dto;
using EvilGiraf.Extensions;

namespace EvilGiraf.Controller;

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
}