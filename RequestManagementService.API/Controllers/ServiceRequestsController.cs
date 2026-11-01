using MediatR;
using Microsoft.AspNetCore.Mvc;
using RequestManagementService.API.Contracts;
using RequestManagementService.Application.Common;
using RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Features.ServiceRequests.CreateServiceRequest;
using RequestManagementService.Application.Features.ServiceRequests.GetServiceRequestById;
using RequestManagementService.Application.Features.ServiceRequests.ListServiceRequests;

namespace RequestManagementService.API.Controllers;

[ApiController]
[Route("api/service-requests")]
public sealed class ServiceRequestsController : ControllerBase
{
    private readonly ISender _sender;

    public ServiceRequestsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpPost]
    [ProducesResponseType<ServiceRequestItem>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestItem>> Create(
        [FromBody] CreateServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _sender.Send(
            new CreateServiceRequestCommand(request.Title, request.Description, request.Priority),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ServiceRequestItem>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ServiceRequestItem> GetById(Guid id, CancellationToken cancellationToken)
    {
        return _sender.Send(new GetServiceRequestByIdQuery(id), cancellationToken);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<ServiceRequestItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<PagedResult<ServiceRequestItem>> List(
        [FromQuery] string? status,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return _sender.Send(new ListServiceRequestsQuery(status, page, pageSize), cancellationToken);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<ServiceRequestItem>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<ServiceRequestItem> ChangeStatus(
        Guid id,
        [FromBody] ChangeServiceRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        return _sender.Send(new ChangeServiceRequestStatusCommand(id, request.Status), cancellationToken);
    }
}
