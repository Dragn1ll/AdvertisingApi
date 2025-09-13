using Application.Enums;
using Application.Interfaces;
using Application.Utils;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Persistence.DataAccess.Repositories;

public class AdvertisingPlatformRepository(AppDbContext dbContext, ILogger<AdvertisingPlatformRepository> logger) 
    : IAdvertisingPlatformRepository
{
    public async Task<Result> Add(AdvertisingPlatformEntity advertisingPlatform)
    {
        try
        {
            await dbContext.AddAsync(advertisingPlatform);
            await dbContext.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message);
            return Result.Failure(new Error(ErrorType.ServerError, 
                "Не удалось внести данные в базу данных"));
        }
    }

    public async Task<Result> Clear()
    {
        try
        {
            dbContext.AdvertisingPlatforms.RemoveRange(dbContext.AdvertisingPlatforms);
            await dbContext.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message);
            return Result.Failure(new Error(ErrorType.ServerError, 
                "Не удалось очистить базу данных"));
        }
    }

    public async Task<Result<IEnumerable<AdvertisingPlatformEntity>>> GetAll()
    {
        try
        {
            var advertisingPlatforms = await dbContext.AdvertisingPlatforms.ToListAsync();
            
            return Result<IEnumerable<AdvertisingPlatformEntity>>.Success(advertisingPlatforms);
        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message);
            return Result<IEnumerable<AdvertisingPlatformEntity>>.Failure(new Error(ErrorType.ServerError, 
                "Не удалось получить данные из базы данных"));
        }
    }
}