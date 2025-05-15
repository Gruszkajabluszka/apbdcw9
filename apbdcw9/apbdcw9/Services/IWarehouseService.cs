using apbdcw9.Modles;
using Microsoft.Data.SqlClient;


namespace apbdcw9.Services;

public interface IWarehouseService
{
    Task<int?> AddProduct(ProductRequestDTO requestDto);
    Task<int?> AddProductUsingProcedure(ProductRequestDTO requestDto);
}