namespace CornerStore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Cashier
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public List<Order> Orders { get; set; }
}