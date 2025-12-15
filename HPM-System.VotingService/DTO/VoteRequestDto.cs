using System.ComponentModel.DataAnnotations;

namespace DTO;

public class VoteRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "ID квартиры должен быть положительным числом.")]
    public long ApartmentId { get; set; }

    [Required(ErrorMessage = "Ответ обязателен.")]
    [MinLength(1, ErrorMessage = "Ответ не может быть пустой строкой.")]
    public string Response { get; set; } = string.Empty;
}