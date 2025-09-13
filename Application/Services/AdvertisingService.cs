using Application.Enums;
using Application.Interfaces;
using Application.Utils;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class AdvertisingService(IAdvertisingPlatformRepository repository) : IAdvertisingService
{
    public async Task<Result<IEnumerable<string>>> GetAdvertisingByLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return Result<IEnumerable<string>>.Success([]);
        }

        var getResult = await repository.GetAll();
        if (!getResult.IsSuccess)
        {
            return Result<IEnumerable<string>>.Failure(getResult.Error);
        }
        
        var platforms = getResult.Value;
        var advertising = new HashSet<string>();

        foreach (var platform in platforms!)
        {
            foreach (var platformLocation in platform.Locations)
            {
                if (location.StartsWith(platformLocation) &&
                    (platformLocation.Length == location.Length ||
                     location[platformLocation.Length] == '/'))
                {
                    advertising.Add(platform.Name);
                    break;
                }
            }
        }
        
        return Result<IEnumerable<string>>.Success(advertising);
    }

    public async Task<Result<bool>> UploadAdvertising(IFormFile file)
    {
        try
        {
            if (file == null! || file.Length == 0)
            {
                return Result<bool>.Failure(new Error(ErrorType.BadRequest, 
                    "Отсутствуют данные для записи в базу данных"));
            }

            var clearResult = await repository.Clear();
            if (!clearResult.IsSuccess)
            {
                return Result<bool>.Failure(clearResult.Error);
            }
        
            using var sr = new StreamReader(file.OpenReadStream());
            var line = await sr.ReadLineAsync();

            while (line != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    line = await sr.ReadLineAsync();
                    continue;
                }
            
                var parts = line.Split(':');
                if (parts.Length != 2)
                {
                    line = await sr.ReadLineAsync();
                    continue;
                }
            
                var platform = new AdvertisingPlatformEntity
                {
                    Name = parts[0].Trim(),
                    Locations = parts[1].Split(',')
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList()
                };

                if (!string.IsNullOrWhiteSpace(platform.Name) && platform.Locations.Any())
                {
                    var addResult = await repository.Add(platform);
                    if (!addResult.IsSuccess)
                    {
                        return Result<bool>.Failure(addResult.Error);
                    }
                }
            
                line = await sr.ReadLineAsync();
            }
        
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new Error(ErrorType.BadRequest, 
                $"Ошибка при работе с входными данными: {ex.Message}"));
        }
    }
}