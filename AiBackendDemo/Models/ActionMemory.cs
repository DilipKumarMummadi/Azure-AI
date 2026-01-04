namespace AiBackendDemo.Models;

public record ActionMemory
{
    [System.ComponentModel.DataAnnotations.Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    public int ActionId { get; init; }
    public string ActionTitle { get; init; } = string.Empty;
    public string ProblemSummary { get; init; } = string.Empty;
    public string Resolution { get; init; } = string.Empty;
    public float[]? Embedding { get; set; }
    public DateTime CreatedAt { get; init; }
}