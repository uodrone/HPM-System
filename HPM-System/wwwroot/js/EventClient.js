export class EventClient {
    constructor() {
        this.gatewayUrl = 'http://localhost:55699'; // Gateway
        this.apiPath = '/api/events'; // lowercase — по соглашению Gateway
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
     * Создать новое событие
     * @param {Object} eventData
     * @returns {Promise<Object>} — созданное событие
     */
    async CreateEvent(eventData) {
        // Валидация обязательных полей
        if (!eventData.title) throw new Error('Поле title обязательно');
        if (!eventData.eventDateTime) throw new Error('Поле eventDateTime обязательно');

        const payload = {
            title: eventData.title,
            description: eventData.description || null,
            imageUrl: eventData.imageUrl || null,
            eventDateTime: eventData.eventDateTime, // ISO 8601 строка или Date
            place: eventData.place || null,
            communityId: eventData.communityId || null,
            communityType: eventData.communityType || 0 // 0 = House
        };

        try {
            const response = await window.apiCall(this._getUrl(''), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка создания события: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при создании события:', error);
            throw error;
        }
    }

    /**
     * Получить событие по ID
     * @param {number} eventId
     * @returns {Promise<Object|null>}
     */
    async GetEventById(eventId) {
        try {
            const response = await window.apiCall(this._getUrl(`/${eventId}`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) return null;
                const errorText = await response.text();
                throw new Error(`Ошибка получения события: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении события по ID:', error);
            throw error;
        }
    }

    /**
     * Получить все события текущего пользователя
     * @returns {Promise<Array>}
     */
    async GetUserEvents() {
        try {
            const response = await window.apiCall(this._getUrl(''), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения событий пользователя: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении событий пользователя:', error);
            throw error;
        }
    }

    /**
    * Получить bool является ли пользователь с id участником события с id?
    */
    async isUserParticipant(userId, eventId) {
        const response = await window.apiCall(
            this._getUrl(`/${eventId}/participants/${userId}`),
            { method: 'GET' }
        );
        if (!response.ok) throw new Error('Ошибка проверки участия');
        return await response.json(); // true или false
    }

    /**
     * Проверяет, подписан ли текущий пользователь на событие
     * @param {number} eventId
     * @returns {Promise<boolean>}
     */
    async IsCurrentUserSubscribed(eventId) {
        try {
            const response = await window.apiCall(
                this._getUrl(`/${eventId}/is-subscribed`),
                {
                    method: 'GET',
                    headers: { 'Content-Type': 'application/json' }
                    // window.apiCall должен автоматически добавить Authorization!
                }
            );

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка проверки подписки: ${errorText}`);
            }

            return await response.json(); // true или false
        } catch (error) {
            console.error('Ошибка при проверке подписки на событие:', error);
            throw error;
        }
    }

    /**
     * Подписаться на событие
     * @param {number} eventId
     * @returns {Promise<boolean>}
     */
    async SubscribeToEvent(eventId) {
        try {
            const response = await window.apiCall(this._getUrl(`/${eventId}/subscribe`), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка подписки на событие: ${errorText}`);
            }

            return true;
        } catch (error) {
            console.error('Ошибка при подписке на событие:', error);
            throw error;
        }
    }

    /**
     * Отписаться от события
     * @param {number} eventId
     * @returns {Promise<boolean>}
     */
    async UnsubscribeFromEvent(eventId) {
        try {
            const response = await window.apiCall(this._getUrl(`/${eventId}/unsubscribe`), {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка отписки от события: ${errorText}`);
            }

            return true;
        } catch (error) {
            console.error('Ошибка при отписке от события:', error);
            throw error;
        }
    }

    /**
     * Установить базовый URL Gateway (для продакшена)
     */
    SetBaseUrl(newBaseUrl) {
        this.gatewayUrl = newBaseUrl.endsWith('/') ? newBaseUrl.slice(0, -1) : newBaseUrl;
    }

    /**
     * Получить текущий базовый URL
     */
    GetBaseUrl() {
        return this.gatewayUrl;
    }
}