using EvilGiraf.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EvilGiraf.Controller;

[ApiController]
[Route("application")]
public class ApplicationController(IApplicationService applicationService) : ControllerBase
{
    
}