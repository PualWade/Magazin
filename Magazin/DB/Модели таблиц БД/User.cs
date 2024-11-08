using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long UserId { get; set; } // Telegram ChatId обычно имеет тип long

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string LanguageCode { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastActivityDate { get; set; }

    [Required]
    public int RoleId { get; set; }

    // Навигационные свойства
    public Role Role { get; set; }

    public ICollection<Order> Orders { get; set; }

    public ICollection<Review> Reviews { get; set; }

    public ICollection<Notification> Notifications { get; set; }

    public ICollection<UserAction> UserActions { get; set; }
}
