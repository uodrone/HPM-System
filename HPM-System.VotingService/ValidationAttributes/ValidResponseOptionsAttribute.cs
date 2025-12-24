using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace VotingService.ValidationAttributes;

public class ValidResponseOptionsAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is not List<string> options)
        {
            return false; // или true, если null разрешён — зависит от Required
        }

        if (options.Count < 2)
        {
            ErrorMessage = "Должно быть указано не менее двух вариантов ответа.";
            return false;
        }

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];

            // Проверить на null, пустую строку или только пробелы
            if (string.IsNullOrWhiteSpace(option))
            {
                ErrorMessage = $"Вариант ответа #{i + 1} не может быть пустым или содержать только пробелы.";
                return false;
            }

            // Проверить, состоит ли строка только из знаков препинания
            if (IsOnlyPunctuation(option))
            {
                ErrorMessage = $"Вариант ответа #{i + 1} не может состоять только из знаков препинания: '{option}'.";
                return false;
            }
        }

        // Проверить на дубликаты (с учётом Trim и IgnoreCase)
        var uniqueOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < options.Count; i++)
        {
            var trimmed = options[i].Trim();
            if (uniqueOptions.Contains(trimmed))
            {
                ErrorMessage = $"Вариант ответа '{trimmed}' встречается более одного раза.";
                return false;
            }
            uniqueOptions.Add(trimmed);
        }

        return true;
    }

    private static bool IsOnlyPunctuation(string s)
    {
        // Проверяет, состоит ли строка только из знаков препинания и пробельных символов
        return s.All(c => char.IsPunctuation(c) || char.IsWhiteSpace(c));
    }
}