import { Modal } from './Modal.js';
import { ApartmentHouses } from './ApartmentHouses.js';
import { VotingClient } from './VotingClient.js';
import { FileStorageClient } from './FileStorageClient.js';
import { DateFormat } from './DateFormat.js';

export class VotingProfileManager {
    constructor() {
        this.houseProfile = new ApartmentHouses();
        this.userId = window.authManager.userData.userId;
        this.votingClient = new VotingClient(); // Добавляем клиент для работы с API
    }

    async InsertDataToCreateVote() {
        const houseSelector = document.getElementById('houseId');
        const eventGroup = document.querySelector('.profile-group[data-group="vote"]');

        // Очистим селектор перед заполнением
        houseSelector.innerHTML = '';

        const houses = await this.houseProfile.GetHousesByUserId(this.userId);

        const houseChecks = await Promise.all(
            // Массив промисов для параллельного выполнения
            houses.map(async (house) => {
                const houseHead = await this.houseProfile.GetHead(house.id);
                if (houseHead == null) return null;
                return { house, isHead: houseHead.id === this.userId };
            })
        );

        // Фильтруем только ненулевые значения, т.е. там где есть старший по дому
        const results = houseChecks.filter(item => item !== null);

        // Фильтруем дома, где пользователь — глава
        const eligibleHouses = results
            .filter(({ isHead }) => isHead)
            .map(({ house }) => house);

        if (eligibleHouses.length) {
            // Пользователь — глава хотя бы в одном доме => показываем селект
            eligibleHouses.forEach(house => {
                const option = document.createElement('option');
                option.value = house.id;
                option.textContent = `${house.city}, ул. ${house.street}, ${house.number}`;
                houseSelector.appendChild(option);
            });
        } else {
            // Ни в одном доме не глава => событие недоступно
            if (eventGroup) {
                eventGroup.innerHTML = `Создание голосования недоступно`;
            }
        }
    }

    AddAnswerOption() {
        const listContainer = document.querySelector('[data-list="AnswerOptions"]');
        if (!listContainer) return;

        const inputs = listContainer.querySelectorAll('input[id^="answerOption-"]');
        if (inputs.length >= 10) return;

        const newIndex = inputs.length + 1;
        const newId = `answerOption-${newIndex}`;

        const newGroup = document.createElement('div');
        newGroup.className = 'form-group';
        newGroup.innerHTML = `
            <input type="text" placeholder="" name="${newId}" id="${newId}" value="">
            <label for="${newId}">Вариант ответа</label>
            <div class="error invisible" data-error="${newId}">Неверный вариант ответа</div>
        `;

        const buttonsWrapper = listContainer.querySelector('.d-flex.gap-3');
        if (buttonsWrapper) {
            listContainer.insertBefore(newGroup, buttonsWrapper);
        }

        const btnRemove = listContainer.querySelector('[data-action="remove-answer-option"]');
        if (btnRemove && newIndex > 2) {
            btnRemove.classList.remove('d-none');
        }
    }

    RemoveAnswerOption() {
        const listContainer = document.querySelector('[data-list="AnswerOptions"]');
        if (!listContainer) return;

        const inputs = listContainer.querySelectorAll('input[id^="answerOption-"]');
        if (inputs.length <= 2) return;

        const lastGroup = inputs[inputs.length - 1].closest('.form-group');
        if (lastGroup) lastGroup.remove();

        const updatedInputs = listContainer.querySelectorAll('input[id^="answerOption-"]');
        const btnRemove = listContainer.querySelector('[data-action="remove-answer-option"]');
        if (btnRemove && updatedInputs.length <= 2) {
            btnRemove.classList.add('d-none');
        }
    }

    ValidateAnswerOptions(options) {
        const fieldErrors = {};
        const errorsToShow = {};
        const punctuationRegex = /^[^\p{L}\p{N}]+$/u; // только пунктуация/пробелы, без букв/цифр

        // Проверка каждого варианта
        const trimmedOptions = [];
        for (let i = 0; i < options.length; i++) {
            const raw = options[i];
            const trimmed = raw.trim();
            const inputId = `answerOption-${i + 1}`;

            let error = null;

            if (!trimmed) {
                error = 'Вариант ответа не может быть пустым или содержать только пробелы.';
            } else if (punctuationRegex.test(trimmed)) {
                error = 'Вариант ответа не может состоять только из знаков препинания.';
            }

            if (error) {
                fieldErrors[inputId] = error;
                errorsToShow[inputId] = error;
            }

            trimmedOptions.push(trimmed);
        }

        // Проверка дубликатов (case-insensitive, с trim)
        const seen = new Set();
        for (let i = 0; i < trimmedOptions.length; i++) {
            const opt = trimmedOptions[i].toLowerCase();
            const inputId = `answerOption-${i + 1}`;

            if (opt && seen.has(opt)) {
                const error = 'Такой вариант ответа уже существует.';
                fieldErrors[inputId] = error;
                errorsToShow[inputId] = error;
            }
            if (opt) seen.add(opt);
        }

        // Обновляем DOM: показываем/скрываем ошибки
        const inputs = document.querySelectorAll('input[id^="answerOption-"]');
        inputs.forEach((_, idx) => {
            const inputId = `answerOption-${idx + 1}`;
            const errorEl = document.querySelector(`[data-error="${inputId}"]`);
            if (errorEl) {
                const hasError = !!errorsToShow[inputId];
                errorEl.textContent = errorsToShow[inputId] || '';
                errorEl.classList.toggle('invisible', !hasError);
            }
        });

        const isValid = Object.keys(fieldErrors).length === 0 && options.length >= 2;
        return { isValid, fieldErrors, trimmedOptions };
    }
    
    /**
     * Собрать варианты ответов с валидацией
     * @returns {Array<string>|null} - массив валидных вариантов или null если невалидно
     */
    CollectAnswerOptions() {
        const inputs = document.querySelectorAll('input[id^="answerOption-"]');
        const options = Array.from(inputs).map(input => input.value);

        console.log('Опции голосования:', options);

        const validation = this.ValidateAnswerOptions(options);

        if (!validation.isValid) {
            console.error('Ошибки валидации вариантов ответа:', validation.fieldErrors);
            return null;
        }

        // Возвращаем trimmed варианты
        return validation.trimmedOptions.filter(opt => opt.length > 0);
    }

    /**
     * Валидация даты и времени завершения голосования
     * @param {string} dateTimeValue - значение из input datetime-local
     * @returns {Object} - {isValid, error, hoursFromNow}
     */
    ValidateVotingDateTime(dateTimeValue) {
        const errorEl = document.querySelector('[data-error="votingDateTime"]');
        
        if (!dateTimeValue) {
            if (errorEl) {
                errorEl.textContent = 'Необходимо указать дату и время завершения голосования';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Необходимо указать дату и время' };
        }

        const endDate = new Date(dateTimeValue);
        const now = new Date();

        if (isNaN(endDate.getTime())) {
            if (errorEl) {
                errorEl.textContent = 'Неверный формат даты и времени';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Неверный формат даты' };
        }

        if (endDate <= now) {
            if (errorEl) {
                errorEl.textContent = 'Дата завершения должна быть в будущем';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Дата должна быть в будущем' };
        }

        // Рассчитываем количество часов до завершения
        const diffMs = endDate.getTime() - now.getTime();
        const hoursFromNow = Math.ceil(diffMs / (1000 * 60 * 60));

        if (hoursFromNow < 1) {
            if (errorEl) {
                errorEl.textContent = 'Минимальная длительность голосования - 1 час';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Минимальная длительность - 1 час' };
        }

        if (hoursFromNow > 8760) { // 365 дней
            if (errorEl) {
                errorEl.textContent = 'Максимальная длительность голосования - 1 год (8760 часов)';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Максимальная длительность - 1 год' };
        }

        // Скрываем ошибку, если валидация прошла
        if (errorEl) {
            errorEl.classList.add('invisible');
        }

        return { isValid: true, hoursFromNow };
    }

    /**
     * Валидация вопроса для голосования
     * @param {string} question
     * @returns {Object} - {isValid, error}
     */
    ValidateQuestionPut(question) {
        const errorEl = document.querySelector('[data-error="questionPut"]');
        const trimmed = question.trim();

        if (!trimmed) {
            if (errorEl) {
                errorEl.textContent = 'Вопрос для голосования обязателен';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Вопрос обязателен' };
        }

        if (trimmed.length < 10) {
            if (errorEl) {
                errorEl.textContent = 'Вопрос должен содержать минимум 10 символов';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Минимум 10 символов' };
        }

        if (trimmed.length > 500) {
            if (errorEl) {
                errorEl.textContent = 'Вопрос не должен превышать 500 символов';
                errorEl.classList.remove('invisible');
            }
            return { isValid: false, error: 'Максимум 500 символов' };
        }

        // Скрываем ошибку
        if (errorEl) {
            errorEl.classList.add('invisible');
        }

        return { isValid: true };
    }

    /**
     * Собрать все данные для создания голосования
     * @returns {Object|null} - объект с данными или null если есть ошибки
     */
    CollectVotingData() {
        // Собираем значения полей
        const votingDateTimeInput = document.getElementById('votingDateTime');
        const questionPutInput = document.getElementById('questionPut');
        const houseIdSelect = document.getElementById('houseId');

        if (!votingDateTimeInput || !questionPutInput || !houseIdSelect) {
            console.error('Не найдены обязательные поля формы');
            return null;
        }

        const votingDateTime = votingDateTimeInput.value;
        const questionPut = questionPutInput.value;
        const houseId = parseInt(houseIdSelect.value);

        // Валидация даты и времени
        const dateValidation = this.ValidateVotingDateTime(votingDateTime);
        if (!dateValidation.isValid) {
            return null;
        }

        // Валидация вопроса
        const questionValidation = this.ValidateQuestionPut(questionPut);
        if (!questionValidation.isValid) {
            return null;
        }

        // Валидация и сбор вариантов ответа
        const responseOptions = this.CollectAnswerOptions();
        if (!responseOptions) {
            return null;
        }

        // Валидация выбранного дома
        if (isNaN(houseId) || houseId <= 0) {
            console.error('Не выбран дом для голосования');
            return null;
        }

        // Формируем объект данных
        const votingData = {
            questionPut: questionPut.trim(),
            responseOptions: responseOptions,
            houseIds: [houseId], // API принимает массив домов
            durationInHours: dateValidation.hoursFromNow
        };

        console.log('Собранные данные для создания голосования:', votingData);
        return votingData;
    }

    /**
     * Создать голосование (вызывается при клике на кнопку сохранения)
     */
    async CreateVoting() {
        try {
            // Собираем данные
            const votingData = this.CollectVotingData();
            
            if (!votingData) {
                console.error('Данные формы невалидны');
                return false;
            }

            // Отправляем на сервер
            const result = await this.votingClient.CreateVoting(votingData);
            
            console.log('Голосование успешно создано:', result);
            
            // Можно показать уведомление об успехе
            // this.showSuccessNotification('Голосование создано успешно');
            
            // Очистить форму
            this.ClearForm();
            Modal.ShowNotification('Создание голосования прошло успешно успешно!', 'green');
            
            return true;
        } catch (error) {
            console.error('Ошибка при создании голосования:', error);
            // Можно показать уведомление об ошибке
            // this.showErrorNotification(error.message);
            return false;
        }
    }

    /**
     * Очистить форму после успешного создания
     */
    ClearForm() {
        document.getElementById('votingDateTime').value = '';
        document.getElementById('questionPut').value = '';
        
        // Очистить варианты ответов
        const inputs = document.querySelectorAll('input[id^="answerOption-"]');
        inputs.forEach(input => input.value = '');
        
        // Скрыть все ошибки
        const errors = document.querySelectorAll('.error');
        errors.forEach(error => error.classList.add('invisible'));
    }

    /**
     * Инициализация обработчиков событий
     */
    InitializeEventHandlersForCreateVoting() {
        // Кнопка добавления варианта ответа
        const btnAdd = document.querySelector('[data-action="add-answer-option"]');
        if (btnAdd) {
            btnAdd.addEventListener('click', () => this.AddAnswerOption());
        }

        // Кнопка удаления варианта ответа
        const btnRemove = document.querySelector('[data-action="remove-answer-option"]');
        if (btnRemove) {
            btnRemove.addEventListener('click', () => this.RemoveAnswerOption());
        }

        // Кнопка сохранения
        const btnSave = document.querySelector('[data-action="save-event-data"]');
        if (btnSave) {
            btnSave.addEventListener('click', async () => {
                await this.CreateVoting();
            });
        }
    }
}

document.addEventListener('authStateChanged', async () => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();
    const UrlParts = Regex.getUrlPathParts(window.location.href);

    if (isAuthenticated && userData) {
        const userId = window.authManager.userData.userId;
        const votingProfile = new VotingProfileManager();
        const votingClient = new VotingClient();

        console.log('Аутентификация пройдена');

        if (window.location.pathname.includes('/vote/create')) {
            votingProfile.InsertDataToCreateVote();
            votingProfile.InitializeEventHandlersForCreateVoting();
        }

        if (window.location.pathname == '/') {            
            
        }

        if (UrlParts.includes(`vote`)) {
            if (UrlParts.includes('by-user') && UrlParts.includes(userId)) {
                
            } else if (!isNaN(Number(UrlParts[1]))) {     
                
            }
        }
    }
});