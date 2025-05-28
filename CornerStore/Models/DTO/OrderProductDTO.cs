using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CornerStore.Models.DTO;

public class OrderProductDTO
{

    public int Id { get; set; }
    public int ProductId { get; set; }
    public ProductDTO Product { get; set; }
    public int OrderId { get; set; }
    public OrderDTO Order { get; set; }
    public int Quantity { get; set; }
    public List<ProductDTO> Products { get; set; }
}