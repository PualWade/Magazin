using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    [Required]
    public string ProductName { get; set; }

    public string ?Description { get; set; }
    
    [Required]
    public decimal Price { get; set; }

    [Required]
    public string ?Currency { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string ?ImageUrl { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Навигационные свойства
    public Category Category { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }

    public ICollection<Review> Reviews { get; set; }
}