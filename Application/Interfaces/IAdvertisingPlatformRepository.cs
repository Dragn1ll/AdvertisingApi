using Application.Utils;
using Domain.Entities;

namespace Application.Interfaces;

public interface IAdvertisingPlatformRepository
{
    Task<Result> Add(AdvertisingPlatformEntity advertisingPlatform);
    
    Task<Result> Clear();
    
    Task<Result<IEnumerable<AdvertisingPlatformEntity>>> GetAll();
}