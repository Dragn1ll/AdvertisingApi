using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisingApi.Endpoints;

public static class AdvertisingEndpoints
{
    public static void MapAdvertisingApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/advertising");

        group.MapGet("/api/advertising/get", GetAdvertisingByLocation);
        group.MapPost("/api/advertising/upload", UploadAdvertising);
    }

    private static async Task<IResult> GetAdvertisingByLocation(
        [FromQuery] string location,
        IAdvertisingService advertisingService
    )
    {
        var advertisingResult = await advertisingService.GetAdvertisingByLocation(location);
        
        return advertisingResult.IsSuccess 
            ? Results.Ok(advertisingResult.Value) 
            : Results.Problem(advertisingResult.Error!.Message, statusCode: (int)advertisingResult.Error.ErrorType);
    }

    private static async Task<IResult> UploadAdvertising(
        [FromForm] IFormFile file,
        IAdvertisingService advertisingService
    )
    {
        var result = await advertisingService.UploadAdvertising(file);
        
        return result.IsSuccess 
            ? Results.Ok() 
            : Results.Problem(result.Error!.Message, statusCode: (int)result.Error.ErrorType);
    }
}