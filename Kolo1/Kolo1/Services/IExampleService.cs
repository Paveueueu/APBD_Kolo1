using Kolokwium1.Models;

namespace Kolokwium1.Services;

public interface IExampleService
{
    Task<GetDeliveriesDto> GetDeliveries(int deliveryId);
    Task<int> AddDelivery(NewDeliveryDto dto);
}