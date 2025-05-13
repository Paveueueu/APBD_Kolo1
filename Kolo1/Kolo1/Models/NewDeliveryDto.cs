namespace Kolokwium1.Models;

public class CustomerDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class DriverDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenceNumber { get; set; }
}

public class NewDeliveryDto
{
    public int DeliveryId { get; set; }
    public int CustomerId { get; set; }
    public string LicenceNumber {get; set;}
    public List<AddProductDto>? Products {get; set;}
}

public class AddProductDto
{
    public string Name {get; set;}
    public int Amount {get; set;}
}

public class ProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Amount { get; set; }
}



public class GetDeliveriesDto
{
    public DateTime Date {get; set;}
    public CustomerDto Customer {get; set;} 
    public DriverDto Driver {get; set;}
    public List<ProductDto> Products {get; set;}
}