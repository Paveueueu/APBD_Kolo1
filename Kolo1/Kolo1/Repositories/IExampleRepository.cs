using Kolokwium1.Models;

namespace Kolokwium1.Repositories;

public interface IExampleRepository
{
    Task<GetDeliveriesDto> GetDeliveries(int deliveryId);
    Task<int> AddDelivery(NewDeliveryDto dto);
}