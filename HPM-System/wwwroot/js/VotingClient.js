export class VotingClient {
    constructor() {
        this.gatewayUrl = 'http://localhost:55699';
        this.apiPath = '/api/votings';
    }

    _getUrl(endpoint) {
        return `${this.gatewayUrl}${this.apiPath}${endpoint}`;
    }

    /**
     * Получить все голосования (для админа)
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
     * Получить детальную информацию о голосовании
     */
    async GetVotingById(votingId) {
        try {
            const response = await window.apiCall(this._getUrl(`/${votingId}`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error('Голосование не найдено');
                }
                const errorText = await response.text();
                throw new Error(`Ошибка получения голосования: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении голосования по ID:', error);
            throw error;
        }
    }

    /**
     * Создать новое голосование
     */
    async CreateVoting(votingData) {
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
            durationInHours: votingData.durationInHours || 168
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
     * Проголосовать (userId берется из JWT автоматически)
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

            return await response.text();
        } catch (error) {
            console.error('Ошибка при отправке голоса:', error);
            throw error;
        }
    }

    /**
     * Получить результаты голосования
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
     * Установить решение комиссии
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

            return await response.text();
        } catch (error) {
            console.error('Ошибка при установке решения:', error);
            throw error;
        }
    }

    /**
     * Удалить голосование
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
     * Получить все голосования текущего пользователя
     */
    async GetMyVotings() {
        try {
            const response = await window.apiCall(this._getUrl('/my'), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения голосований: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении голосований пользователя:', error);
            throw error;
        }
    }

    /**
     * Получить активные голосования текущего пользователя
     */
    async GetMyActiveVotings() {
        try {
            const response = await window.apiCall(this._getUrl('/my/active'), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения активных голосований: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении активных голосований:', error);
            throw error;
        }
    }

    /**
     * Получить завершённые голосования текущего пользователя
     */
    async GetMyCompletedVotings() {
        try {
            const response = await window.apiCall(this._getUrl('/my/completed'), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения завершённых голосований: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении завершённых голосований:', error);
            throw error;
        }
    }

    /**
     * Получить завершённые голосования без решения
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

    SetBaseUrl(newBaseUrl) {
        this.gatewayUrl = newBaseUrl.endsWith('/') ? newBaseUrl.slice(0, -1) : newBaseUrl;
    }

    GetBaseUrl() {
        return this.gatewayUrl;
    }
}