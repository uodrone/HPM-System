namespace VotingService.Validation;

// Результат валидации голоса
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public ValidationErrorType ErrorType { get; set; }

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Fail(string errorMessage, ValidationErrorType errorType = ValidationErrorType.BusinessRule)
    {
        return new ValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            ErrorType = errorType
        };
    }
}

// Типы ошибок валидации для более детальной обработки
public enum ValidationErrorType
{
    NotFound,           // Сущность не найдена
    BusinessRule,       // Нарушение бизнес-правила
    Authorization,      // Проблема с авторизацией
    InvalidInput        // Некорректный ввод
}