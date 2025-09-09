class UserValidator {
    constructor() {
        // Разрешенные буквы в российских номерах (совпадают с латинскими)
        this.allowedLetters = 'АВЕКМНОРСТУХ';

        // Коды регионов России
        this.validRegionCodes = [
            // Основные регионы
            '01', '02', '03', '04', '05', '06', '07', '08', '09', '10',
            '11', '12', '13', '14', '15', '16', '17', '18', '19', '20',
            '21', '22', '23', '24', '25', '26', '27', '28', '29', '30',
            '31', '32', '33', '34', '35', '36', '37', '38', '39', '40',
            '41', '42', '43', '44', '45', '46', '47', '48', '49', '50',
            '51', '52', '53', '54', '55', '56', '57', '58', '59', '60',
            '61', '62', '63', '64', '65', '66', '67', '68', '69', '70',
            '71', '72', '73', '74', '75', '76', '77', '78', '79', '80',
            '81', '82', '83', '84', '85', '86', '87', '88', '89', '90',
            '91', '92', '93', '94', '95', '96', '97', '98', '99',
            // Трёхзначные коды для крупных регионов
            '102', '113', '116', '117', '118', '119', '121', '122', '123',
            '124', '125', '126', '134', '136', '138', '142', '150', '152',
            '154', '159', '161', '163', '164', '173', '174', '177', '178',
            '186', '190', '196', '197', '198', '199', '702', '716', '750',
            '761', '763', '774', '777', '790', '799'
        ];

        // Запрещенные комбинации
        this.forbiddenCombinations = [
            /.*ХУ[ЙИ].*/,
            /.*БЛ[ЯА].*/,
            /.*П[ИИ]З.*/,
            /.*МУД.*/,
            /.*ГОВ.*/
        ];
    }

    /**
     * Валидация имени/фамилии/отчества
     */
    validateName(name, fieldName, required = true) {
        if (!name || name.trim() === '') {
            if (required) {
                return { isValid: false, error: `${fieldName} обязательно для заполнения` };
            }
            return { isValid: true };
        }

        if (name.length > 50) {
            return { isValid: false, error: `${fieldName} не может быть длиннее 50 символов` };
        }

        if (!/^[а-яё\s\-']+$/i.test(name)) {
            return { isValid: false, error: `${fieldName} должно содержать только русские буквы, пробелы, дефисы и апострофы` };
        }

        // Проверка на слишком много пробелов подряд
        if (/\s{2,}/.test(name)) {
            return { isValid: false, error: `${fieldName} не должно содержать несколько пробелов подряд` };
        }

        // Проверка на пробелы в начале/конце
        if (name !== name.trim()) {
            return { isValid: false, error: `${fieldName} не должно начинаться или заканчиваться пробелом` };
        }

        return { isValid: true };
    }

    /**
     * Валидация даты рождения
     */
    validateBirthday(birthday) {
        if (!birthday) {
            return { isValid: true }; // Дата рождения необязательна
        }

        const birthDate = new Date(birthday);
        const today = new Date();
        const minDate = new Date(today.getFullYear() - 120, today.getMonth(), today.getDate());
        const maxDate = new Date(today.getFullYear() - 14, today.getMonth(), today.getDate()); // Минимальный возраст 14 лет

        if (isNaN(birthDate.getTime())) {
            return { isValid: false, error: 'Неверный формат даты' };
        }

        if (birthDate > today) {
            return { isValid: false, error: 'Дата рождения не может быть в будущем' };
        }

        if (birthDate < minDate) {
            return { isValid: false, error: 'Дата рождения не может быть более 120 лет назад' };
        }

        if (birthDate > maxDate) {
            return { isValid: false, error: 'Минимальный возраст должен быть 14 лет' };
        }

        return { isValid: true };
    }

    /**
     * Валидация номера телефона
     */
    validatePhoneNumber(phone) {
        if (!phone || phone.trim() === '') {
            return { isValid: false, error: 'Номер телефона обязателен для заполнения' };
        }

        // Убираем все символы кроме цифр и +
        const cleanPhone = phone.replace(/[\s\-\(\)]/g, '');

        // Различные форматы российских номеров
        const phonePatterns = [
            /^(\+7|8)\d{10}$/,           // +7XXXXXXXXXX или 8XXXXXXXXXX
            /^\+7\d{10}$/,               // +7XXXXXXXXXX
            /^8\d{10}$/,                 // 8XXXXXXXXXX
            /^7\d{10}$/                  // 7XXXXXXXXXX
        ];

        let isValidFormat = false;
        for (const pattern of phonePatterns) {
            if (pattern.test(cleanPhone)) {
                isValidFormat = true;
                break;
            }
        }

        if (!isValidFormat) {
            return { isValid: false, error: 'Неверный формат номера телефона. Используйте российский формат' };
        }

        return { isValid: true };
    }

    /**
     * Валидация email
     */
    validateEmail(email) {
        if (!email || email.trim() === '') {
            return { isValid: false, error: 'Email обязателен для заполнения' };
        }

        if (email.length > 100) {
            return { isValid: false, error: 'Email не может быть длиннее 100 символов' };
        }

        const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        if (!emailRegex.test(email)) {
            return { isValid: false, error: 'Неверный формат email' };
        }

        // Проверка на запрещенные символы в локальной части
        const localPart = email.split('@')[0];
        if (localPart.startsWith('.') || localPart.endsWith('.') || localPart.includes('..')) {
            return { isValid: false, error: 'Неверный формат email' };
        }

        return { isValid: true };
    }

    /**
     * Валидация номера автомобиля
     */
    validateCarNumber(number) {
        if (!number || number.trim() === '') {
            return { isValid: false, error: 'Номер автомобиля обязателен' };
        }

        const cleanNumber = number.replace(/[\s\-]/g, '').toUpperCase();

        // Проверка длины
        if (cleanNumber.length < 8 || cleanNumber.length > 9) {
            return { isValid: false, error: 'Неверная длина номера (должно быть 8-9 символов)' };
        }

        // Различные форматы российских номеров
        const patterns = [
            { pattern: /^[АВЕКМНОРСТУХ]\d{3}[АВЕКМНОРСТУХ]{2}\d{2,3}$/, type: 'стандартный' },
            { pattern: /^[АВЕКМНОРСТУХ]{2}\d{3}[АВЕКМНОРСТУХ]\d{2,3}$/, type: 'такси' },
            { pattern: /^[АВЕКМНОРСТУХ]{2}\d{4}\d{2,3}$/, type: 'прицеп' },
            { pattern: /^\d{4}[АВЕКМНОРСТУХ]{2}\d{2,3}$/, type: 'мотоцикл' },
            { pattern: /^Т\d{3}[АВЕКМНОРСТУХ]{2}\d{2,3}$/, type: 'транзит' }
        ];

        let isValidFormat = false;
        for (const { pattern } of patterns) {
            if (pattern.test(cleanNumber)) {
                isValidFormat = true;
                break;
            }
        }

        if (!isValidFormat) {
            return { isValid: false, error: 'Неверный формат российского номера' };
        }

        // Проверка кода региона
        const regionCode = cleanNumber.slice(-3);
        const twoDigitCode = regionCode.slice(-2);
        const threeDigitCode = regionCode;

        if (!this.validRegionCodes.includes(twoDigitCode) && !this.validRegionCodes.includes(threeDigitCode)) {
            return { isValid: false, error: 'Неверный код региона' };
        }

        // Проверка на запрещенные комбинации
        for (const forbidden of this.forbiddenCombinations) {
            if (forbidden.test(cleanNumber)) {
                return { isValid: false, error: 'Недопустимая комбинация символов в номере' };
            }
        }

        return { isValid: true };
    }

    /**
     * Валидация марки/модели автомобиля
     */
    validateCarBrand(value, fieldName) {
        if (!value || value.trim() === '') {
            return { isValid: true }; // Необязательное поле
        }

        if (value.length > 100) {
            return { isValid: false, error: `${fieldName} не может быть длиннее 100 символов` };
        }

        if (!/^[а-яёa-z0-9\s\-._]+$/i.test(value)) {
            return { isValid: false, error: `${fieldName} должна содержать только буквы, цифры, пробелы, дефисы, точки и подчеркивания` };
        }

        return { isValid: true };
    }

    /**
     * Валидация цвета автомобиля
     */
    validateCarColor(color) {
        if (!color || color.trim() === '') {
            return { isValid: true }; // Необязательное поле
        }

        if (color.length > 50) {
            return { isValid: false, error: 'Цвет не может быть длиннее 50 символов' };
        }

        if (!/^[а-яёa-z\s\-]+$/i.test(color)) {
            return { isValid: false, error: 'Цвет должен содержать только буквы, пробелы и дефисы' };
        }

        return { isValid: true };
    }

    /**
     * Валидация одного автомобиля
     */
    validateCar(car) {
        const errors = {};
        let isValid = true;

        // Валидация марки
        const markValidation = this.validateCarBrand(car.mark, 'Марка');
        if (!markValidation.isValid) {
            errors.mark = markValidation.error;
            isValid = false;
        }

        // Валидация модели
        const modelValidation = this.validateCarBrand(car.model, 'Модель');
        if (!modelValidation.isValid) {
            errors.model = modelValidation.error;
            isValid = false;
        }

        // Валидация цвета
        const colorValidation = this.validateCarColor(car.color);
        if (!colorValidation.isValid) {
            errors.color = colorValidation.error;
            isValid = false;
        }

        // Валидация номера
        const numberValidation = this.validateCarNumber(car.number);
        if (!numberValidation.isValid) {
            errors.number = numberValidation.error;
            isValid = false;
        }

        return { isValid, errors };
    }

    /**
     * Проверка уникальности номеров автомобилей
     */
    validateUniqueCarNumbers(cars) {
        const numbers = cars
            .map(car => car.number?.replace(/[\s\-]/g, '').toUpperCase())
            .filter(Boolean);

        const duplicates = [];
        const seen = new Set();

        numbers.forEach((number, index) => {
            if (seen.has(number)) {
                duplicates.push(index);
            } else {
                seen.add(number);
                // Также добавляем индекс первого вхождения дубликата
                const firstIndex = numbers.indexOf(number);
                if (firstIndex !== index && !duplicates.includes(firstIndex)) {
                    duplicates.push(firstIndex);
                }
            }
        });

        return duplicates;
    }

    /**
     * Полная валидация данных пользователя
     */
    validateUserData(userData) {
        const errors = {
            user: {},
            cars: []
        };
        let isValid = true;

        // Валидация данных пользователя
        const firstNameValidation = this.validateName(userData.firstName, 'Имя', true);
        if (!firstNameValidation.isValid) {
            errors.user.firstName = firstNameValidation.error;
            isValid = false;
        }

        const lastNameValidation = this.validateName(userData.lastName, 'Фамилия', true);
        if (!lastNameValidation.isValid) {
            errors.user.lastName = lastNameValidation.error;
            isValid = false;
        }

        const patronymicValidation = this.validateName(userData.patronymic, 'Отчество', false);
        if (!patronymicValidation.isValid) {
            errors.user.patronymic = patronymicValidation.error;
            isValid = false;
        }

        const birthdayValidation = this.validateBirthday(userData.birthday);
        if (!birthdayValidation.isValid) {
            errors.user.birthday = birthdayValidation.error;
            isValid = false;
        }

        const phoneValidation = this.validatePhoneNumber(userData.phoneNumber);
        if (!phoneValidation.isValid) {
            errors.user.phoneNumber = phoneValidation.error;
            isValid = false;
        }

        const emailValidation = this.validateEmail(userData.email);
        if (!emailValidation.isValid) {
            errors.user.email = emailValidation.error;
            isValid = false;
        }

        // Валидация автомобилей
        if (userData.cars && userData.cars.length > 0) {
            userData.cars.forEach((car, index) => {
                const carValidation = this.validateCar(car);
                if (!carValidation.isValid) {
                    errors.cars[index] = carValidation.errors;
                    isValid = false;
                }
            });

            // Проверка уникальности номеров
            const duplicateIndexes = this.validateUniqueCarNumbers(userData.cars);
            duplicateIndexes.forEach(index => {
                if (!errors.cars[index]) errors.cars[index] = {};
                errors.cars[index].number = 'Номер автомобиля должен быть уникальным';
                isValid = false;
            });
        }

        return {
            isValid,
            errors
        };
    }

    /**
     * Форматирование номера автомобиля
     */
    formatCarNumber(input) {
        let value = input.value.replace(/[^а-яёА-ЯЁ0-9ТтTt]/g, '').toUpperCase();

        // Заменяем русские буквы на допустимые
        const letterMap = {
            'А': 'А', 'В': 'В', 'Е': 'Е', 'К': 'К', 'М': 'М', 'Н': 'Н',
            'О': 'О', 'Р': 'Р', 'С': 'С', 'Т': 'Т', 'У': 'У', 'Х': 'Х',
            'T': 'Т' // Латинская T заменяется на русскую Т
        };

        value = value.split('').map(char => letterMap[char] || char).join('');

        // Ограничиваем длину
        if (value.length > 9) {
            value = value.slice(0, 9);
        }

        input.value = value;
    }

    /**
     * Форматирование номера телефона
     */
    formatPhoneNumber(input) {
        let value = input.value.replace(/\D/g, '');

        if (value.startsWith('8') && value.length > 1) {
            value = '7' + value.slice(1);
        }

        if (value.startsWith('7') && value.length <= 11) {
            const formatted = value.replace(/^7(\d{3})(\d{3})(\d{2})(\d{2})/, '+7 ($1) $2-$3-$4');
            input.value = formatted;
        } else if (value.length <= 10) {
            const formatted = value.replace(/^(\d{3})(\d{3})(\d{2})(\d{2})/, '+7 ($1) $2-$3-$4');
            input.value = formatted;
        }
    }
}