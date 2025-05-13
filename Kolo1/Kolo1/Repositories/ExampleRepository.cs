using System.Data.Common;
using Kolokwium1.Models;
using Microsoft.Data.SqlClient;

namespace Kolokwium1.Repositories;

public class ExampleRepository : IExampleRepository
{
    private readonly string _connectionString;
    private SqlConnection _connection;
    private SqlCommand _command;
    private DbTransaction _transaction;

    public ExampleRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<GetDeliveriesDto> GetDeliveries(int deliveryId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();


        // Get delivery data
        GetDeliveriesDto? result = null;
        command.CommandText = "SELECT [date] FROM Delivery WHERE delivery_id = @DeliveryId;";
        command.Parameters.AddWithValue("@DeliveryId", deliveryId);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result = new GetDeliveriesDto
            {
                Date = reader.GetDateTime(0),
                Products = []
            };
        }
        reader.Close();

        if (result == null)
            throw new Exception("No Delivery found");
        
        // Read customer data
        command.Parameters.Clear();
        command.CommandText = """
                              SELECT first_name, last_name, date_of_birth
                              FROM Customer JOIN Delivery ON Customer.customer_id = Delivery.customer_id
                              WHERE delivery_id = @DeliveryId;
                              """;
        command.Parameters.AddWithValue("@DeliveryId", deliveryId);
        await using var reader2 = await command.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            result.Customer = new CustomerDto
            {
                FirstName = reader2.GetString(0),
                LastName = reader2.GetString(1),
                DateOfBirth = reader2.GetDateTime(2),
            };
        }
        reader2.Close();
        
        // Read customer data
        command.Parameters.Clear();
        command.CommandText = """
                              SELECT first_name, last_name, licence_number
                              FROM Driver JOIN Delivery ON Driver.driver_id = Delivery.driver_id
                              WHERE delivery_id = @DeliveryId;
                              """;
        command.Parameters.AddWithValue("@DeliveryId", deliveryId);
        await using var reader3 = await command.ExecuteReaderAsync();
        while (await reader3.ReadAsync())
        {
            result.Driver = new DriverDto
            {
                FirstName = reader3.GetString(0),
                LastName = reader3.GetString(1),
                LicenceNumber = reader3.GetString(2)
            };
        }
        reader3.Close();

        
        // Get products
        command.Parameters.Clear();
        command.CommandText = """
                              SELECT [name], price, amount
                              FROM Product 
                                  JOIN Product_Delivery ON Product.product_id = Product_Delivery.product_id
                                  JOIN s30660.Delivery D on D.delivery_id = Product_Delivery.delivery_id
                              WHERE D.delivery_id = 1
                              """;
        command.Parameters.AddWithValue("@DeliveryId", deliveryId);
        await using var reader4 = await command.ExecuteReaderAsync();
        while (await reader4.ReadAsync())
        {
            result.Products.Add(new ProductDto
            {
                Name = reader4.GetString(0),
                Price = reader4.GetDecimal(1),
                Amount = reader4.GetInt32(2),
            });
        }
        reader4.Close();
        
        return result;
    }

    public async Task<int> AddDelivery(NewDeliveryDto dto)
    {
        await BeginTransactionAsync();

        if (await DoesDeliveryExist(dto.DeliveryId, dto.CustomerId))
            throw new ArgumentException("Delivery exists already");
        
        if (!await DoesCustomerExist(dto.CustomerId))
            throw new ArgumentException("Custoner does not exist");
        
        if (!await DoesDriverExist(dto.LicenceNumber))
            throw new ArgumentException("Driver does not exist");

        foreach (var product in dto.Products)
        {
            if (!await DoesProductExist(product.Name))
                throw new ArgumentException("Product does not exist");
        }

        try
        {
            var result = await InsertDelivery(dto.DeliveryId, dto.CustomerId, dto.LicenceNumber, dto.Products);
            await CommitTransactionAsync();
            return 0;
        }
        catch (Exception e)
        {
            await RollbackTransactionAsync();
            throw;
        }
        
    }

    public async Task BeginTransactionAsync()
    {
        _connection = new SqlConnection(_connectionString);
        _command = new SqlCommand();
        
        _command.Connection = _connection;
        await _connection.OpenAsync();

        _transaction = await _connection.BeginTransactionAsync();
        _command.Transaction = _transaction as SqlTransaction;
    }

    public async Task CommitTransactionAsync()
    {
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        await _connection.CloseAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _connection.CloseAsync();
    }
    
    public async Task<bool> DoesDeliveryExist(int deliveryId, int customerId)
    {
        _command.CommandText = "SELECT COUNT(1) FROM Product_Delivery WHERE delivery_id = @DeliveryId AND customer_id = @CustomerId;";
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@DeliveryId", deliveryId);
        _command.Parameters.AddWithValue("@CustomerId", customerId);

        var result = (int) (await _command.ExecuteScalarAsync() ?? throw new Exception());
        return result > 0;
    }
    
    public async Task<bool> DoesCustomerExist(int customerId)
    {
        _command.CommandText = "SELECT COUNT(1) FROM Customer WHERE customer_id = @CustomerId";
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@CustomerId", customerId);

        var result = (int) (await _command.ExecuteScalarAsync() ?? throw new Exception());
        return result > 0;
    }
    
    public async Task<bool> DoesDriverExist(string licenceNumber)
    {
        _command.CommandText = "SELECT COUNT(1) FROM Driver WHERE licence_number = @LicenceNumber";
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@LicenceNumber", licenceNumber);

        var result = (int) (await _command.ExecuteScalarAsync() ?? throw new Exception());
        return result > 0;
    }
    
    public async Task<int> GetDriverId(string licenceNumber)
    {
        _command.CommandText = "SELECT driver_id FROM Driver WHERE licence_number = @LicenceNumber";
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@LicenceNumber", licenceNumber);

        var result = (int) (await _command.ExecuteScalarAsync() ?? throw new Exception());
        return result;
    }
    
    public async Task<bool> DoesProductExist(string name)
    {
        _command.CommandText = "SELECT COUNT(1) FROM Product WHERE name = @Name";
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@Name", name);

        var result = (int) (await _command.ExecuteScalarAsync() ?? throw new Exception());
        return result > 0;
    }
    
    public async Task<int> GetProductId(string name)
    {
        _command.CommandText = "SELECT product_id FROM Product WHERE name = @Name";
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@Name", name);

        var result = (int) (await _command.ExecuteScalarAsync() ?? throw new Exception());
        return result;
    }
    
    public async Task<int> InsertDelivery(int deliveryId, int customerId, string licenceNumber, List<AddProductDto> products)
    {
        var driverId = await GetDriverId(licenceNumber);
        
        _command.CommandText = """
                                   INSERT INTO Delivery (delivery_id, customer_id, driver_id, [date])
                                   VALUES (@DeliveryId, @CustomerId, @LicenceNumber, @Date)
                               """;
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@DeliveryId", deliveryId);
        _command.Parameters.AddWithValue("@CustomerId", customerId);
        _command.Parameters.AddWithValue("@LicenceNumber", driverId);
        _command.Parameters.AddWithValue("@Date", DateTime.Now);

        foreach (var product in products)
        {
            await InsertProduct_Delivery(product, deliveryId);
        }
        
        await _command.ExecuteNonQueryAsync();
        return 0;
    }
    
    public async Task InsertProduct_Delivery(AddProductDto product, int deliveryId)
    {
        var productId = await GetProductId(product.Name);
        
        _command.CommandText = """
                                   INSERT INTO Product_Delivery (product_id, delivery_id, amount)
                                   VALUES (@ProductId, @DeliveryId, @Amount)
                               """;
        _command.Parameters.Clear();
        _command.Parameters.AddWithValue("@ProductId", productId);
        _command.Parameters.AddWithValue("@DeliveryId", deliveryId);
        _command.Parameters.AddWithValue("@Amount", product.Amount);
        await _command.ExecuteNonQueryAsync();
    }
}