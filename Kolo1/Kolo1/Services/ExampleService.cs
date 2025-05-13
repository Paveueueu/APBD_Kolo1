using Kolokwium1.Models;
using Kolokwium1.Repositories;

namespace Kolokwium1.Services;

public class ExampleService : IExampleService
{
    private readonly IExampleRepository _repository;
    
    public ExampleService(IExampleRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<GetDeliveriesDto> GetDeliveries(int deliveryId)
    {
        return await _repository.GetDeliveries(deliveryId);
    }

    public async Task<int> AddDelivery(NewDeliveryDto dto)
    {
        return await _repository.AddDelivery(dto);
    }
}