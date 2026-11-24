using Drugly.DTO;
using Drugly.Server.Models;
using Drugly.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Drugly.Server.Controllers;

/// <summary>A controller that handles routes for managing images</summary>
public class ImageController : DruglyController
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IImageDatabaseService _databaseService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(
        IImageDatabaseService databaseService,
        ILogger<ImageController> logger,
        IAuthorizationService authorizationService
    )
    {
        _databaseService = databaseService;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        Stream response;
        try
        {
            response = await _databaseService.GetImageById(id, out var contentType);
            Response.ContentType = contentType;
        }
        catch (ImageNotFoundException ex)
        {
            _logger.LogError(ex, "Failed to find image {Id}", id);
            return NotFound(ApiResponse.Error("Image not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch image {Id}", id);
            return InternalServerError(ApiResponse.Error("Internal server error"));
        }

        _logger.LogInformation("Image {Id} retrieved successfully", id);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Upload()
    {
        if (Request.Headers.ContentLength is null or 0)
        {
            return BadRequest(ApiResponse.Error("Bad Content-Length header"));
        }

        var content = Request.Body;

        if (!_authorizationService.IsUserAuthorized(Request.Headers, AccountType.Doctor))
        {
            _logger.LogInformation("User is not authorized");
            return Forbid(ApiResponse.Error("User is not authorized"));
        }

        var id = Guid.NewGuid().ToString();
        var contentType = Request.ContentType ?? "image/bmp";
        try
        {
            await _databaseService.SetImageById(id, contentType, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set image {Id}", id);
            return InternalServerError(ApiResponse.Error("Internal server error"));
        }

        _logger.LogInformation("Image {Id} set successfully", id);
        return Ok(new ApiResponse<string> { Data = id });
    }
}
