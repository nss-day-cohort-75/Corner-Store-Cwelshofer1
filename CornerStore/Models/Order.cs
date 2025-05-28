using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CornerStore.Models;

public class Order
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int CashierId { get; set; }

    [ForeignKey("CashierId")]
    public Cashier Cashier { get; set; }

    [NotMapped]
    public decimal Total => OrderProducts?.Sum(op => op?.Product?.Price * op.Quantity) ?? 0;
    public DateTime? PaidOnDate { get; set; }
    public List<OrderProduct> OrderProducts { get; set; }
    [NotMapped]
    public OrderProduct OrderProduct { get; set; }
    [NotMapped]
    public List<Product> Products { get; set; }
}