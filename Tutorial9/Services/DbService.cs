using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class DbService : IDbService
{
   // string connectionString = "Server=(localdb)\\;Database=localDB2;Trusted_Connection=True;";
    
   private readonly IConfiguration _configuration;
   public DbService(IConfiguration configuration)
   {
       _configuration = configuration;
   }

    public Task<int> addProduct(Warehouse warehouse)
{
    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
    {
        connection.Open();

        var checkexisting = new SqlCommand("SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse", connection);
        checkexisting.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
        object res1 = checkexisting.ExecuteScalar();
        if (res1 == null || res1 == DBNull.Value || Convert.ToInt32(res1) == 0)
            return Task.FromResult(-101);

        var checkproductexisting = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct", connection);
        checkproductexisting.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        object res2 = checkproductexisting.ExecuteScalar();
        if (res2 == null || res2 == DBNull.Value || Convert.ToInt32(res2) == 0)
            return Task.FromResult(-102);

        var checkIfProductInOrder = new SqlCommand(
            "SELECT IdOrder FROM dbo.[Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt <= @CreatedAt AND FulfilledAt IS NULL", connection);
        checkIfProductInOrder.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        checkIfProductInOrder.Parameters.AddWithValue("@Amount", warehouse.Amount);
        checkIfProductInOrder.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);
        object orderIdObj = checkIfProductInOrder.ExecuteScalar();
        if (orderIdObj == null || orderIdObj == DBNull.Value)
            return Task.FromResult(-103);

        int orderId = Convert.ToInt32(orderIdObj);

        var checkIfOrderInProductWarehouse = new SqlCommand(
            "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder", connection);
        checkIfOrderInProductWarehouse.Parameters.AddWithValue("@IdOrder", orderId);
        object res3 = checkIfOrderInProductWarehouse.ExecuteScalar();
        if (res3 != null && res3 != DBNull.Value && Convert.ToInt32(res3) > 0)
            return Task.FromResult(-104);

        DateTime today = DateTime.Now;
        var updateFulfilledAt = new SqlCommand(
            "UPDATE dbo.[Order] SET FulfilledAt = @today WHERE IdOrder = @IdOrder", connection);
        updateFulfilledAt.Parameters.AddWithValue("@today", today);
        updateFulfilledAt.Parameters.AddWithValue("@IdOrder", orderId);
        updateFulfilledAt.ExecuteNonQuery();

        string insertQuery = @"
            INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            SELECT @IdWarehouse, @IdProduct, @IdOrder, @Amount, p.Price * @Amount, @today
            FROM Product p
            WHERE p.IdProduct = @IdProduct";

        var insert = new SqlCommand(insertQuery, connection);
        insert.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
        insert.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
        insert.Parameters.AddWithValue("@IdOrder", orderId);
        insert.Parameters.AddWithValue("@Amount", warehouse.Amount);
        insert.Parameters.AddWithValue("@today", today);
        insert.ExecuteNonQuery();

        string resQuery = "SELECT MAX(IdProductWarehouse) FROM Product_Warehouse";
        object result = new SqlCommand(resQuery, connection).ExecuteScalar();

        if (result != null && result != DBNull.Value)
        {
            int id = Convert.ToInt32(result);
            return Task.FromResult(id);
        }

        return Task.FromResult(-105);
    }
}

}