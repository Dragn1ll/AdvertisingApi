using Application.Utils;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IAdvertisingService
{
    Task<Result<IEnumerable<string>>> GetAdvertisingByLocation(string location);
    
    Task<Result<bool>> UploadAdvertising(IFormFile file);
}