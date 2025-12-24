export class NotificationClient {
    constructor() {
        this.gatewayUrl = 'http://localhost:55699'; // Gateway
        this.apiPath = '/api/notifications'; // lowercase по соглашению Gateway
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
     * Получить все уведомления
     * @returns {Promise<Array>}
     */
    async GetAllNotifications() {
        try {
            const response = await window.apiCall(this._getUrl(''), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения уведомлений: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении всех уведомлений:', error);
            throw error;
        }
    }

    /**
     * Получить уведомление по ID
     * @param {string} id - GUID уведомления
     * @returns {Promise<Object|null>}
     */
    async GetNotificationById(id) {
        try {
            const response = await window.apiCall(this._getUrl(`/${id}`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) return null;
                const errorText = await response.text();
                throw new Error(`Ошибка получения уведомления: ${errorText}`);
            }
                const error = await response.text();
                throw new Error(`Ошибка получения уведомления: ${error}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении уведомления по ID:', error);
            throw error;
        }
    }

    /**
     * Получить уведомления для конкретного пользователя
     * @param {string} userId - GUID пользователя
     * @returns {Promise<Array>}
     */
    async GetNotificationsByUserId(userId) {
        try {
            const response = await window.apiCall(this._getUrl(`/user/${userId}`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения уведомлений пользователя: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении уведомлений пользователя:', error);
            throw error;
        }
    }

    /**
     * Получить непрочитанные уведомления для пользователя
     * @param {string} userId
     * @returns {Promise<Array>}
     */
    async GetUnreadNotificationsByUserId(userId) {
        try {
            const response = await window.apiCall(this._getUrl(`/user/${userId}/unread`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения непрочитанных уведомлений: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении непрочитанных уведомлений:', error);
            throw error;
        }
    }

    /**
     * Получить количество непрочитанных уведомлений
     * @param {string} userId
     * @returns {Promise<number>}
     */
    async GetUnreadCount(userId) {
        try {
            const response = await window.apiCall(this._getUrl(`/user/${userId}/unread/count`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения количества непрочитанных: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении количества непрочитанных уведомлений:', error);
            throw error;
        }
    }

    /**
     * Отметить уведомление как прочитанное (по ID записи NotificationUsers)
     * @param {string} recipientId
     * @returns {Promise<boolean>}
     */
    async MarkAsReadByRecipientId(recipientId) {
        try {
            const response = await window.apiCall(this._getUrl(`/recipient/${recipientId}/mark-read`), {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) return false;
                const errorText = await response.text();
                throw new Error(`Ошибка отметки уведомления как прочитанного: ${errorText}`);
            }
                const error = await response.text();
                throw new Error(`Ошибка отметки уведомления как прочитанного: ${error}`);
            }

            return true;
        } catch (error) {
            console.error('Ошибка при отметке уведомления как прочитанного:', error);
            throw error;
        }
    }

    /**
     * Отметить уведомление как прочитанное (по ID уведомления и пользователя)
     * @param {string} notificationId
     * @param {string} userId
     * @returns {Promise<boolean>}
     */
    async MarkAsReadByIds(notificationId, userId) {
        try {
            const response = await window.apiCall(this._getUrl(`/notification/${notificationId}/user/${userId}/mark-read`), {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) return false;
                const errorText = await response.text();
                throw new Error(`Ошибка отметки уведомления как прочитанного: ${errorText}`);
            }
                const error = await response.text();
                throw new Error(`Ошибка отметки уведомления как прочитанного: ${error}`);
            }

            return true;
        } catch (error) {
            console.error('Ошибка при отметке уведомления как прочитанного (по ID):', error);
            throw error;
        }
    }

    /**
     * Отметить все уведомления пользователя как прочитанные
     * @param {string} userId
     * @returns {Promise<number>}
     */
    async MarkAllAsRead(userId) {
        try {
            const response = await window.apiCall(this._getUrl(`/user/${userId}/mark-all-read`), {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка отметки всех уведомлений как прочитанных: ${errorText}`);
            }

            const result = await response.json();
            // Ожидаем ответ вида { count: N } или строку с числом — сделаем обобщённую обработку
            if (typeof result === 'object' && result.count !== undefined) {
                return result.count;
            } else if (typeof result === 'string') {
                const match = result.match(/\d+/);
                return match ? parseInt(match[0], 10) : 0;
            }
            return 0;
        } catch (error) {
            console.error('Ошибка при отметке всех уведомлений как прочитанных:', error);
            throw error;
        }
    }

    /**
     * Создать новое уведомление
     * @param {Object} notificationData
     * @returns {Promise<boolean>}
     */
    async CreateNotification(notificationData) {
        // Валидация
        if (!notificationData.title) throw new Error('Поле title обязательно');
        if (!notificationData.message) throw new Error('Поле message обязательно');
        if (!notificationData.createdBy) throw new Error('Поле createdBy обязательно');
        if (!Array.isArray(notificationData.userIdList) || notificationData.userIdList.length === 0) {
            throw new Error('Поле userIdList должно быть непустым массивом');
        }

        // Подготовка данных с дефолтными значениями
        const payload = {
            title: notificationData.title,
            message: notificationData.message,
            imageUrl: notificationData.imageUrl || null,
            createdBy: notificationData.createdBy,
            type: notificationData.type === 0 ? 0 : 1,
            isReadable: notificationData.isReadable !== false, // true по умолчанию
            userIdList: notificationData.userIdList
        };

        try {
            const response = await window.apiCall(this._getUrl(''), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Ошибка создания уведомления:', errorText);
                return false;
            } else {
                return true;
            }

            return true;
        } catch (error) {
            console.error('Ошибка при создании уведомления:', error);
            throw error;
        }
    }

    /**
     * Установить базовый URL (редко нужно, но оставлено для совместимости)
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

// Инициализация при авторизации
document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        // Экземпляр можно создать по месту использования
        // window.notificationClient = new NotificationClient(); // опционально
    }

// 9. Автоматическое обновление каждые 30 секунд
setInterval(() => {
    const currentUserId = getCurrentUserId(); // ваша функция получения ID текущего пользователя
    updateNotificationUI(currentUserId);
}, 30000);

// 10. Отправка уведомления всем пользователям квартиры
async function notifyApartmentUsers(apartmentUserIds, title, message) {
    try {
        const notification = await notificationClient.createNotification({
            title: title,
            message: message,
            type: 'info',
            createdBy: getCurrentUserId(),
            userIdList: apartmentUserIds
});
        console.log('Уведомление отправлено:', notification.id);
        return notification;
    } catch (error) {
        console.error('Ошибка отправки уведомления:', error);
        throw error;
    }
}

// 11. Фильтрация уведомлений по типу
async function getNotificationsByType(userId, type) {
    const notifications = await notificationClient.getNotificationsByUserId(userId);
    return notifications.filter(n => n.type === type);
}

// Примеры типов: 'info', 'warning', 'error', 'success'
const warningNotifications = await getNotificationsByType(userId, 'warning');
*/