using Microsoft.EntityFrameworkCore;

namespace AiBackendDemo.Models
{
    public class Action
    {
        [System.ComponentModel.DataAnnotations.Key]
        [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ActionStatus { get; set; }
        public float[]? Embedding { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
