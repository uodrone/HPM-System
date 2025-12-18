export class VotingClient {
    constructor() {
        this.gatewayUrl = 'http://localhost:55699'; // Gateway
        this.apiPath = '/api/Votings'; // lowercase — по соглашению Gateway
    }

    /**
     * Получить полный URL для эндпоинта (через Gateway)
     * @param {string} endpoint 
     * @returns {string}
     */
    _getUrl(endpoint) {
        return `${this.gatewayUrl}${this.apiPath}${endpoint}`;
    }

    /**
     * Получить все голосования
     * @returns {Promise<Array>}
     */
    async GetAllVotings() {
        try {
            const response = await window.apiCall(this._getUrl(''), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения голосований: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении голосований:', error);
            throw error;
        }
    }

    /**
     * Создать новое голосование
     * @param {Object} votingData
     * @param {string} votingData.questionPut - Вопрос для голосования
     * @param {Array<string>} votingData.responseOptions - Варианты ответа (минимум 2)
     * @param {Array<number>} votingData.houseIds - ID домов для голосования
     * @param {number} votingData.durationInHours - Длительность в часах (по умолчанию 168 = 7 дней)
     * @returns {Promise<Object>} - созданное голосование
     */
    async CreateVoting(votingData) {
        // Валидация обязательных полей
        if (!votingData.questionPut) {
            throw new Error('Поле questionPut обязательно');
        }
        if (!votingData.responseOptions || votingData.responseOptions.length < 2) {
            throw new Error('Необходимо указать минимум 2 варианта ответа');
        }
        if (!votingData.houseIds || votingData.houseIds.length === 0) {
            throw new Error('Необходимо указать хотя бы один дом');
        }

        const payload = {
            questionPut: votingData.questionPut,
            responseOptions: votingData.responseOptions,
            houseIds: votingData.houseIds,
            durationInHours: votingData.durationInHours || 168 // 7 дней по умолчанию
        };

        try {
            const response = await window.apiCall(this._getUrl(''), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка создания голосования: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при создании голосования:', error);
            throw error;
        }
    }

    /**
     * Проголосовать в голосовании
     * @param {string} votingId - GUID голосования
     * @param {Object} voteData
     * @param {string} voteData.userId - GUID пользователя
     * @param {number} voteData.apartmentId - ID квартиры
     * @param {string} voteData.response - Выбранный вариант ответа
     * @returns {Promise<string>} - сообщение о результате
     */
    async SubmitVote(votingId, voteData) {
        if (!voteData.userId) {
            throw new Error('Поле userId обязательно');
        }
        if (!voteData.apartmentId) {
            throw new Error('Поле apartmentId обязательно');
        }
        if (!voteData.response) {
            throw new Error('Поле response обязательно');
        }

        const payload = {
            userId: voteData.userId,
            apartmentId: voteData.apartmentId,
            response: voteData.response
        };

        try {
            const response = await window.apiCall(this._getUrl(`/${votingId}/vote`), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка при голосовании: ${errorText}`);
            }

            return await response.text(); // Возвращает строку с сообщением
        } catch (error) {
            console.error('Ошибка при отправке голоса:', error);
            throw error;
        }
    }

    /**
     * Получить результаты голосования
     * @param {string} votingId - GUID голосования
     * @returns {Promise<Object>} - результаты голосования
     */
    async GetVotingResults(votingId) {
        try {
            const response = await window.apiCall(this._getUrl(`/${votingId}/results`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error('Голосование не найдено');
                }
                const errorText = await response.text();
                throw new Error(`Ошибка получения результатов: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении результатов голосования:', error);
            throw error;
        }
    }

    /**
     * Установить решение комиссии по голосованию
     * @param {string} votingId - GUID голосования
     * @param {string} decision - Текст решения
     * @returns {Promise<string>} - сообщение о результате
     */
    async SetVotingDecision(votingId, decision) {
        if (!decision || decision.trim() === '') {
            throw new Error('Решение не может быть пустым');
        }

        try {
            const response = await window.apiCall(this._getUrl(`/${votingId}/decision`), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(decision)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка при вынесении решения: ${errorText}`);
            }

            return await response.text(); // "Решение вынесено"
        } catch (error) {
            console.error('Ошибка при установке решения:', error);
            throw error;
        }
    }

    /**
     * Удалить голосование
     * @param {string} votingId - GUID голосования
     * @returns {Promise<boolean>}
     */
    async DeleteVoting(votingId) {
        try {
            const response = await window.apiCall(this._getUrl(`/${votingId}`), {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка при удалении голосования: ${errorText}`);
            }

            console.log('Голосование успешно удалено');
            return true;
        } catch (error) {
            console.error('Ошибка при удалении голосования:', error);
            throw error;
        }
    }

    /**
     * Получить активные голосования пользователя (где он ещё не проголосовал)
     * @param {string} userId - GUID пользователя
     * @returns {Promise<Array>}
     */
    async GetUserActiveVotings(userId) {
        try {
            const response = await window.apiCall(this._getUrl(`/user/${userId}/active`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения активных голосований: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении активных голосований пользователя:', error);
            throw error;
        }
    }

    /**
     * Получить завершённые голосования пользователя (где он уже проголосовал)
     * @param {string} userId - GUID пользователя
     * @returns {Promise<Array>}
     */
    async GetUserCompletedVotings(userId) {
        try {
            const response = await window.apiCall(this._getUrl(`/user/${userId}/completed`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения завершённых голосований: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении завершённых голосований пользователя:', error);
            throw error;
        }
    }

    /**
     * Получить завершённые голосования без решения комиссии
     * @returns {Promise<Array>}
     */
    async GetUnresolvedVotings() {
        try {
            const response = await window.apiCall(this._getUrl('/completed-without-decision'), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения нерешённых голосований: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении нерешённых голосований:', error);
            throw error;
        }
    }

    /**
     * Установить базовый URL Gateway (для продакшена)
     * @param {string} newBaseUrl
     */
    SetBaseUrl(newBaseUrl) {
        this.gatewayUrl = newBaseUrl.endsWith('/') ? newBaseUrl.slice(0, -1) : newBaseUrl;
    }

    /**
     * Получить текущий базовый URL
     * @returns {string}
     */
    GetBaseUrl() {
        return this.gatewayUrl;
    }
}