using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Magazin.Models
{
    //(опционально)
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        // Навигационное свойство
        public User User { get; set; }
    } 
}
