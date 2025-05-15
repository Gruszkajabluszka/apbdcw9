using System.Data.Common;
using System.Data;
using apbdcw9.Modles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace apbdcw9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;
    public  WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
  
    public async Task<int?> AddProduct([FromBody] ProductRequestDTO requestDto){
        
       
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command= new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        
        // SqlTransaction transaction = connection.BeginTransaction();
        // command.Transaction = transaction;
    
        try
        {
            command.CommandText = "Select COUNT(1) FROM Product Where IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
            int productExists = (int)await command.ExecuteScalarAsync();
            if (productExists == 0)
                throw new Exception("Product does not exist");
            
            command.Parameters.Clear();
            
            command.CommandText = "Select Count(1) FROM Warehouse Where IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", requestDto.IdWarehouse);
            int warehouseExists = (int)await command.ExecuteScalarAsync();
            if (warehouseExists == 0)
                throw new Exception("Warehouse does not exist");
            if (requestDto.Amount <= 0)
            {
                throw new Exception("Amount is less than 0");
            }
            
            command.CommandText = @"SELECT Count(*) FROM [Order] WHERE IdProduct = @IdProduct AND Amount >0";
            command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
            command.Parameters.AddWithValue("@Amount", requestDto.Amount);
            int product = (int)await command.ExecuteScalarAsync();
            if (product == 0)
            {
                throw new Exception("Product does not exist");
            }
            
            command.Parameters.Clear();
            
            command.CommandText ="Select COUNT(*) From [Order] Where IdProduct = @IdProduct AND Amount>0 And CreatedAt<@CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
            command.Parameters.AddWithValue("@Amount", requestDto.Amount);
            command.Parameters.AddWithValue("@CreatedAt", requestDto.CreatedAt);
            
            int ex1=(int)await command.ExecuteScalarAsync();
            if (ex1 == 0)
                throw new Exception("Wrong date or ");
            
            command.Parameters.Clear();

            command.CommandText = "Select FulfilledAt From [Order] Where IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
            if (await command.ExecuteScalarAsync() != DBNull.Value)
            {
                throw new Exception("Already Fulfilled");
            }
            
            
           
            command.Parameters.Clear();
            
            command.CommandText = @"SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = (SELECT IdOrder FROM [Order] where [Order].IdProduct = @IdProduct AND [Order].Amount = @Amount)"; ;
            command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
            command.Parameters.AddWithValue("@Amount", requestDto.Amount);
            int ex3=(int)await command.ExecuteScalarAsync();
            // if (ex3 == 0)
            // {
            //     throw new Exception("Not Fulfilled");
            // }
            //
            command.Parameters.Clear();
    
          
            command.CommandText = "UPDATE [Order] SET FulfilledAt = @Now WHERE IdOrder = (SELECT IdOrder FROM [Order] where [Order].IdProduct = @IdProduct)";
            command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
            int ex4 = (int)await command.ExecuteNonQueryAsync();
            if (ex4 == 0)
            {
                throw new Exception("Not Fulfilled");
            }
            
            command.Parameters.Clear();
            
            command.CommandText = @"
                 INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
        VALUES 
            (@IdWarehouse, @IdProduct, (SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount >0),
            @Amount, (SELECT Price FROM Product WHERE IdProduct = @IdProduct) * @Amount, @CreatedAt);
        
        Select SCOPE_IDENTITY();";
            
            command.Parameters.AddWithValue("@IdWarehouse", requestDto.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
            command.Parameters.AddWithValue("@Amount", requestDto.Amount);
            command.Parameters.AddWithValue("@CreatedAt", requestDto.CreatedAt);

            
            int res = (int)await command.ExecuteNonQueryAsync();
            if(res == 0)
                throw new Exception("huh?");
            
            return Convert.ToInt32(res);
        }
        catch (Exception e)
        {
            //await transaction.RollbackAsync();
            throw e;
            throw;
        }
        
    }
    public async Task<int?> AddProductUsingProcedure([FromBody] ProductRequestDTO requestDto)
{
    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
    await using SqlCommand command = new SqlCommand("AddProductToWarehouse", connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.AddWithValue("@IdProduct", requestDto.IdProduct);
    command.Parameters.AddWithValue("@IdWarehouse", requestDto.IdWarehouse);
    command.Parameters.AddWithValue("@Amount", requestDto.Amount);
    command.Parameters.AddWithValue("@CreatedAt", requestDto.CreatedAt);

    await connection.OpenAsync();
    var scalar = await command.ExecuteScalarAsync();
    int newId = Convert.ToInt32(scalar);

    return newId;
}

}