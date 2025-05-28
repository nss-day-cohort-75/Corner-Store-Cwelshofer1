using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CornerStore.Models.DTO;

public class OrderDTO
{

    public int Id { get; set; }
    public int CashierId { get; set; }
    public CashierDTO Cashier { get; set; }
    public decimal Total => OrderProducts?.Sum(op => op.Product.Price * op.Quantity) ?? 0;
    public DateTime? PaidOnDate { get; set; }
    public List<OrderProductDTO> OrderProducts { get; set; }
    public OrderProductDTO OrderProduct { get; set; }
    public ProductDTO Product { get; set; }
    public List<ProductDTO> Products { get; set; }
    
}