using System.ComponentModel.DataAnnotations;
using VotingService.ValidationAttributes;

namespace DTO;

public class CreateVotingRequestDto
{
    [Required(ErrorMessage = "Вопрос для голосования обязателен.")]
    public string QuestionPut { get; set; } = string.Empty;

    [Required(ErrorMessage = "Необходимо указать варианты ответа.")]
    [ValidResponseOptions]
    public List<string> ResponseOptions { get; set; } = new();

    [Required(ErrorMessage = "Необходимо указать ID домов.")]
    [MinLength(1, ErrorMessage = "Список домов не может быть пустым.")]
    public List<long> HouseIds { get; set; } = new();

    [Range(1, 8760, ErrorMessage = "Длительность голосования должна быть от 1 часа до 8760 (1 год).")]
    public int DurationInHours { get; set; } = 24 * 7; // по умолчанию 7 дней
}