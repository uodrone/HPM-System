using System.ComponentModel.DataAnnotations;

namespace DTO;

public class VoteRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Ответ обязателен.")]
    [MinLength(1, ErrorMessage = "Ответ не может быть пустой строкой.")]
    public string Response { get; set; } = string.Empty;
}