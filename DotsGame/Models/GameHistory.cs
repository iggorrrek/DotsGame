using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotsGame.Models
{
    public class GameHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string OpponentName { get; set; }

        [Required]
        public bool IsWinner { get; set; }

        [Required]
        public int PlayerScore { get; set; }

        [Required]
        public int OpponentScore { get; set; }
    }
}