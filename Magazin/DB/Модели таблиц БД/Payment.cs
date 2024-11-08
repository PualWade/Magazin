using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentId { get; set; }

    [Required]
    public int OrderId { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; }

    [Required]
    public string PaymentStatus { get; set; }

    public string TransactionId { get; set; }

    public string PaymentMethod { get; set; }

    // Навигационное свойство
    public Order Order { get; set; }
}
