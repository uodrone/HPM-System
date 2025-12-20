import { Modal } from './Modal.js';
import { ApartmentHouses } from './ApartmentHouses.js';
import { VotingClient } from './VotingClient.js';
import { FileStorageClient } from './FileStorageClient.js';
import { DateFormat } from './DateFormat.js';

export class VotingProfileManager {
    constructor() {
        this.houseProfile = new ApartmentHouses();
        this.userId = window.authManager.userData.userId;
        this.votingClient = new VotingClient();
        this.currentVoting = null;
        this.fullVotingData = null;
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

    InsertDataToMainPage (data) {
        const voteContainer = document.querySelector('.vote-list');        
        if (data.length) {
            data.forEach((vote) => {
                const voteMainPageTemplate = this.VoteMainPageTemplate(vote);
                voteContainer.insertAdjacentHTML('beforeend', voteMainPageTemplate);
            });
        } else {
            voteContainer.innerHTML = `Нет новых голосований`;
        }
    }

    VoteMainPageTemplate (vote) {
        let voteHTML;
        if (vote) {
            voteHTML = `
                <a class="card-item card-item_vote" href="/vote/${vote.votingId}">
                    <div class="font-size-12 color-gray">${DateFormat.DateFormatToRuString(vote.endTime)}</div>
                    <div class="font-weight-600">${vote.questionPut}</div>
                </a>
            `;            
        }

        return voteHTML;
    }

    VotingsListByUserId (votings) {
        const votingsContainer = document.querySelector('.votings-by-user-list');        
        if (votings.length) {
            for (const vote of votings) {
                console.log(`голосование`);
                console.log(vote);
                const voteToListByUserId = this.VoteTemplateByUserId(vote);
                votingsContainer.insertAdjacentHTML('beforeend', voteToListByUserId);
            }
        } else {
            votingsContainer.innerHTML = `Нет новых голосований`;
        }
    }

    VoteTemplateByUserId(vote) {
        let voteHTML;
        if (vote) {
            const votingAction = vote.hasVoted ? `Подробности` : `Проголосовать`;
            const isVoted = vote.hasVoted ? `Вы уже проголосовали` : ``;
            const decision = vote.hasDecision ? '<div><b>Решение вынесено</b></div>' : '<div><b>Решение еще не вынесено</b></div>';
            const isVoteComplete = vote.isCompleted 
                ? `<span style="font-size: 14px;">Завершено: ${DateFormat.DateFormatToRuString(endTime)}</span>`
                : `<span style="font-size: 14px;">Завершится: ${DateFormat.DateFormatToRuString(vote.endTime)}</span>`;

            voteHTML = `
                <div class="profile-group dashboard-card my-4" data-group="vote" data-vote-id="${vote.votingId}">
                    <h3 class="card-header card-header_vote w-100 d-flex justify-content-between align-items-center">
                        <a href="/vote/${vote.votingId}">${vote.questionPut}</a> ${isVoteComplete}
                    </h3>
                    <div class="card-content w-100">
                        <div class="d-flex flex-wrap gap-4 w-100 justify-content-between">
                            <div>Всего участников: <b>${vote.totalParticipants}</b></div>
                            <div>Всего проголосовало: <b>${vote.votedCount}</b></div>                            
                            ${decision}
                            ${isVoted}                        
                        </div>
                        <div class="text-center mt-4"><a href="/vote/${vote.votingId}">${votingAction}</a></div>
                    </div>
                </div>
            `;            
        }

        return voteHTML;
    }

    /**
     * Загрузить и отобразить данные голосования
     * @param {string} votingId - GUID голосования
     */
    async LoadVotingProfile(votingId) {
        try {
            // Получаем детальную информацию о голосовании
            const voting = await this.votingClient.GetVotingById(votingId);
            console.log(`голосование:`);
            console.log(voting);
            
            if (!voting) {
                Modal.ShowNotification('Голосование не найдено', 'red');
                return;
            }

            if (!voting.isParticipant) {
                Modal.ShowNotification('Вы не являетесь участником этого голосования', 'red');
                return;
            }

            this.currentVoting = voting;
            
            this.RenderVotingProfile();
            this.InitializeVotingProfileHandlers();            
        } catch (error) {
            console.error('Ошибка при загрузке профиля голосования:', error);
            Modal.ShowNotification('Ошибка при загрузке голосования', 'red');
        }
    }

    /**
     * Отобразить данные голосования
     */
    RenderVotingProfile() {
        // Заполняем вопрос
        const questionElement = document.getElementById('question-put');
        if (questionElement) {
            questionElement.textContent = this.currentVoting.questionPut;
        }

        // Заполняем статус и время
        const votingEndDiv = document.getElementById('voting-end');
        const votingEndTimeSpan = document.getElementById('voting-end-time');
        
        if (votingEndDiv && votingEndTimeSpan) {
            const formattedDate = DateFormat.DateFormatToRuString(this.currentVoting.endTime);
            console.log(`формат даты: ${formattedDate}`);

            votingEndDiv.innerHTML = this.currentVoting.isCompleted 
                ? '<strong>Голосование завершено:</strong> '
                : '<strong>Голосование завершится:</strong> ';
            
            votingEndTimeSpan.textContent = formattedDate;
        }

        

        // Варианты или результаты
        if (this.currentVoting.hasVoted || this.currentVoting.isCompleted) {
            this.RenderVotingResults();
        } else {
            this.RenderVotingOptions();
        }

        // Кнопка
        this.UpdateVoteButton(this.currentVoting.userApartmentId);
    }

    /**
     * Отобразить статистику
     */
    RenderVotingStats() {
        const votingStatsContainer = document.querySelector(`[data-group="voting-stats"]`);
        if (!votingStatsContainer) return;

        votingStatsContainer.innerHTML = '';

        const progressPercent = this.currentVoting.totalParticipants > 0 
            ? Math.round((this.currentVoting.votedCount / this.currentVoting.totalParticipants) * 100) 
            : 0;
        
        let progressClass = 'bg-danger';
        if (progressPercent >= 75) progressClass = 'bg-success';
        else if (progressPercent >= 50) progressClass = 'bg-info';
        else if (progressPercent >= 25) progressClass = 'bg-warning';

        const statsHtml = `
            <div class="voting-stats mt-3 p-3 bg-light rounded">
                <p class="mb-2">
                    <strong>Проголосовало:</strong> 
                    ${this.currentVoting.votedCount} из ${this.currentVoting.totalParticipants} участников
                </p>
                <div class="progress" style="height: 25px;">
                    <div class="progress-bar ${progressClass}" style="width: ${progressPercent}%">
                        ${progressPercent}%
                    </div>
                </div>
                ${this.currentVoting.hasVoted ? 
                    `<p class="mt-2 mb-0 text-success"><strong>✓ Вы уже проголосовали: ${this.currentVoting.userResponse}</strong></p>` : 
                    `<p class="mt-2 mb-0 text-warning"><strong>⚠ Вы ещё не проголосовали</strong></p>`
                }
                ${this.currentVoting.isCompleted && this.currentVoting.hasDecision ? 
                    '<p class="mt-2 mb-0 text-info"><strong>ℹ Решение по голосованию вынесено</strong></p>' : ''
                }
            </div>
        `;

        votingStatsContainer.insertAdjacentHTML('afterend', statsHtml);
    }

    /**
     * Отобразить варианты ответа
     */
    RenderVotingOptions() {
        const optionsContainer = document.querySelector('[data-group="voting-options"]');
        if (!optionsContainer) return;

        optionsContainer.innerHTML = '';

        this.currentVoting.responseOptions.forEach((option, index) => {
            const optionId = `voting-option-${index}`;
            const optionHtml = `
                <div class="form-check my-3 d-flex align-items-center">
                    <input 
                        class="form-check-input" 
                        type="radio" 
                        name="votingOption" 
                        id="${optionId}" 
                        value="${option}"
                        style="width: 20px; height: 20px; margin-right: 10px;"
                    >
                    <label class="form-check-label fs-5" for="${optionId}" style="cursor: pointer;">
                        ${option}
                    </label>
                </div>
            `;
            optionsContainer.insertAdjacentHTML('beforeend', optionHtml);
        });
    }

    /**
     * Отобразить результаты голосования
     */
    async RenderVotingResults() {
        const optionsContainer = document.querySelector('[data-group="voting-options"]');
        if (!optionsContainer) return;

        if (!this.currentVoting.isCompleted && this.currentVoting.hasVoted) {
            optionsContainer.innerHTML = `
                <div class="alert alert-info">
                    <p><strong>Вы проголосовали: ${this.currentVoting.userResponse}</strong></p>
                    <p>Результаты будут доступны после завершения голосования.</p>
                </div>
            `;
            return;
        }

        try {
            const results = await this.votingClient.GetVotingResults(this.currentVoting.id);
            optionsContainer.innerHTML = '<h4 class="mt-4 mb-3">Результаты голосования:</h4>';

            // Создаем Map всех вариантов с результатами (0% для тех, за которые не голосовали)
            const allOptions = this.currentVoting.responseOptions.map(option => {
                return {
                    option: option,
                    percent: results.responses[option] || 0
                };
            });

            // Сортируем по проценту (от большего к меньшему)
            allOptions.sort((a, b) => b.percent - a.percent);

            // Отображаем все варианты
            allOptions.forEach(({ option, percent }) => {
                const isUserChoice = this.currentVoting.userResponse === option;
                const resultHtml = `
                    <div class="mb-3">
                        <div class="d-flex justify-content-between align-items-center mb-1">
                            <strong>${option} ${isUserChoice ? '(ваш выбор)' : ''}</strong>
                            <span class="badge bg-secondary">${percent}%</span>
                        </div>
                        <div class="progress" style="height: 25px;">
                            <div class="progress-bar ${isUserChoice ? 'bg-primary' : 'bg-secondary'}" 
                                style="width: ${percent}%">
                            </div>
                        </div>
                    </div>
                `;
                optionsContainer.insertAdjacentHTML('beforeend', resultHtml);
            });

            // Показываем решение, если оно есть
            if (results.decision && results.decision !== 'Решение не опубликовано') {
                optionsContainer.insertAdjacentHTML('beforeend', `
                    <div class="alert alert-success mt-4">
                        <h5>Решение комиссии:</h5>
                        <p class="mb-0">${results.decision}</p>
                    </div>
                `);
            }

            // Статистика
            this.RenderVotingStats();
        } catch (error) {
            console.error('Ошибка при загрузке результатов:', error);
            Modal.ShowNotification('Ошибка при загрузке результатов', 'red');
        }
    }

    /**
     * Управление кнопкой
     */
    async UpdateVoteButton(apartmentId) {
        const voteButton = document.querySelector('[data-action="send-vote"]');
        const decisionButton = document.querySelector('[data-modal="open"]');        

        if (!voteButton) return;

        if (this.currentVoting.hasVoted || this.currentVoting.isCompleted) {
            voteButton.classList.add('d-none');

            const houseHead = await this.houseProfile.GetHeadByApartmentId(apartmentId);

            if (houseHead.id == this.userId) {
                decisionButton.classList.remove('d-none');
            }
        }
    }

    /**
     * Собрать данные голоса
     */
    CollectVoteData() {
        const selectedOption = document.querySelector('input[name="votingOption"]:checked');
        
        if (!selectedOption) {
            Modal.ShowNotification('Пожалуйста, выберите вариант ответа', 'orange');
            return null;
        }

        return {
            userId: this.userId,
            apartmentId: this.currentVoting.userApartmentId,
            response: selectedOption.value
        };
    }

    /**
     * Отправить голос
     */
    async SubmitVote() {
        try {
            if (this.currentVoting.isCompleted || this.currentVoting.hasVoted) {
                Modal.ShowNotification('Вы уже проголосовали или голосование завершено', 'orange');
                return;
            }

            const voteData = this.CollectVoteData();
            if (!voteData) return;

            const voteButton = document.querySelector('[data-action="send-vote"]');
            if (voteButton) {
                voteButton.textContent = 'Отправка...';
                voteButton.style.pointerEvents = 'none';
            }

            await this.votingClient.SubmitVote(this.currentVoting.id, voteData);
            Modal.ShowNotification('Ваш голос успешно принят!', 'green');

            setTimeout(() => this.LoadVotingProfile(this.currentVoting.id), 1500);
        } catch (error) {
            console.error('Ошибка:', error);
            Modal.ShowNotification(`Ошибка: ${error.message}`, 'red');
            
            const voteButton = document.querySelector('[data-action="send-vote"]');
            if (voteButton) {
                voteButton.textContent = 'Проголосовать';
                voteButton.style.pointerEvents = 'auto';
            }
        }
    }

    /**
     * Инициализация обработчиков
     */
    InitializeVotingProfileHandlers() {
        const voteButton = document.querySelector('[data-action="send-vote"]');
        if (voteButton) {
            const newButton = voteButton.cloneNode(true);
            voteButton.parentNode.replaceChild(newButton, voteButton);
            newButton.addEventListener('click', () => this.SubmitVote());
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

        if (window.location.pathname.includes('/vote/create')) {
            votingProfile.InsertDataToCreateVote();
            votingProfile.InitializeEventHandlersForCreateVoting();
        }

        if (window.location.pathname == '/') {
            const votingsByUser = await votingClient.GetMyActiveVotings();
            console.log(`голосования для пользователя`);
            console.log(votingsByUser);
            votingProfile.InsertDataToMainPage(votingsByUser);
        }

        if (UrlParts.includes(`vote`)) {
            if (UrlParts.includes('by-user') && UrlParts.includes(userId)) {
                const votingsByUser = await votingClient.GetMyVotings();
                console.log(`Все голосования пользователя:`);
                console.log(votingsByUser);
                votingProfile.VotingsListByUserId(votingsByUser);
            } else if (Regex.isGuid(UrlParts[1])) {     
                const votingId = UrlParts[1];
                console.log(`Загрузка профиля голосования: ${votingId}`);
                await votingProfile.LoadVotingProfile(votingId);
            }
        }
    }
});