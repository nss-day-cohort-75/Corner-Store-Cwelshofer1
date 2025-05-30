using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CornerStore.Models.DTO;
namespace CornerStore.Models;

public class ProductDTO
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Brand { get; set; }
    public int CategoryId { get; set; }
    public CategoryDTO Category { get; set; }
    public List<CashierDTO> Cashiers { get; set; }
    public OrderProductDTO OrderProduct { get; set; }
}