using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Magazin.Models
{
    //(опционально)
    public class UserAction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActionId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public string ActionType { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        public string Details { get; set; }

        // Навигационное свойство
        public User User { get; set; }
    }
}