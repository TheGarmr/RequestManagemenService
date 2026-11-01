using Microsoft.AspNetCore.Mvc;

namespace RequestManagementService.API.Controllers;

[ApiController]
[Route("api/service-requests")]
public sealed class ServiceRequestsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create()
    {
        return NotImplementedProblem();
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        return NotImplementedProblem();
    }

    [HttpGet]
    public IActionResult List([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return NotImplementedProblem();
    }

    [HttpPatch("{id:guid}/status")]
    public IActionResult ChangeStatus(Guid id)
    {
        return NotImplementedProblem();
    }

    private ObjectResult NotImplementedProblem()
    {
        return Problem(statusCode: StatusCodes.Status501NotImplemented);
    }
}
