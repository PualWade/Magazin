using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }

    [Required]
    public long UserId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public string Currency { get; set; }

    [Required]
    public string OrderStatus { get; set; }

    public string PaymentMethod { get; set; }

    public string ShippingAddress { get; set; }

    public string Comments { get; set; }

    // Навигационные свойства
    public User User { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }

    public ICollection<Payment> Payments { get; set; }
}
