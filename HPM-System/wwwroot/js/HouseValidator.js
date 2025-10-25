export class HouseValidator {
    static validate(house) {
        const errors = {};

        // Обязательные текстовые поля
        const requiredTextFields = ['city', 'street', 'number'];
        for (const field of requiredTextFields) {
            if (!house[field] || typeof house[field] !== 'string' || house[field].trim() === '') {
                errors[field] = 'Обязательное поле';
            }
        }

        // Числовые поля: entrances, floors, builtYear — должны быть >= 1
        const positiveIntFields = ['entrances', 'floors', 'builtYear'];
        for (const field of positiveIntFields) {
            const val = house[field];
            if (typeof val !== 'number' || !Number.isInteger(val) || val < 1) {
                errors[field] = 'Должно быть целым числом ≥ 1';
            }
        }

        // Год постройки: разумный диапазон (например, 1800–текущий год + 1)
        const currentYear = new Date().getFullYear();
        if (house.builtYear && (house.builtYear < 1800 || house.builtYear > currentYear + 1)) {
            errors.builtYear = `Год должен быть от 1800 до ${currentYear + 1}`;
        }

        // Площади: apartmentsArea, totalArea, landArea — должны быть >= 0 или null
        const areaFields = ['apartmentsArea', 'totalArea', 'landArea'];
        for (const field of areaFields) {
            const val = house[field];
            if (val !== null && (typeof val !== 'number' || val < 0)) {
                errors[field] = 'Площадь должна быть ≥ 0';
            }
        }

        // Почтовый индекс: может быть null или строка (опционально можно добавить формат)
        if (house.postIndex !== null && (typeof house.postIndex !== 'string' || house.postIndex.trim() === '')) {
            errors.postIndex = 'Некорректный почтовый индекс';
        }

        // Булевы поля: должны быть boolean
        const boolFields = ['hasGas', 'hasElectricity', 'hasElevator', 'isApartmentBuilding'];
        for (const field of boolFields) {
            if (typeof house[field] !== 'boolean') {
                errors[field] = 'Должно быть логическим значением';
            }
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors
        };
    }

    // Опционально: метод для отображения ошибок в UI
    static displayErrors(errors) {
        // Скрываем все ошибки
        document.querySelectorAll('[data-error]').forEach(el => {
            el.classList.add('invisible');
            el.textContent = '';
        });

        // Показываем ошибки по ключам
        for (const [field, message] of Object.entries(errors)) {
            const errorEl = document.querySelector(`[data-error="${field}"]`);
            if (errorEl) {
                errorEl.textContent = message;
                errorEl.classList.remove('invisible');
            }
        }
    }
}