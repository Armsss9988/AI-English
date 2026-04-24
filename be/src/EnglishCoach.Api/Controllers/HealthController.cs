using EnglishCoach.Contracts.Health;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCoach.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        var response = HealthResponseFactory.Create("1.0.0");
        return Ok(response);
    }

    [HttpGet("error")]
    public ActionResult<ApiErrorResponse> Error()
    {
        return StatusCode(500, new ApiErrorResponse(
            "INTERNAL_ERROR",
            "An unexpected error occurred",
            null
        ));
    }
}

public record ApiErrorResponse(string Code, string Message, string? Details);