using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// (опционально)
public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReviewId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    public string Comment { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public Product Product { get; set; }

    public User User { get; set; }
}
