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
}