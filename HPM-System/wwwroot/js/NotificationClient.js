export class NotificationClient {
    constructor() {
        this.baseUrl = 'https://localhost:55691';
        this.apiPath = '/api/Notifications';
    }

    /**
     * Получить полный URL для эндпоинта
     * @param {string} endpoint 
     * @returns {string}
     */
    _getUrl(endpoint) {
        return `${this.baseUrl}${this.apiPath}${endpoint}`;
    }

    /**
     * Получить все уведомления
     * @returns {Promise<Array>} Массив всех уведомлений
     * @example
     * const notifications = await client.getAllNotifications();
     * console.log('Всего уведомлений:', notifications.length);
     */
    async getAllNotifications() {
        try {
            const response = await fetch(this._getUrl(''), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка получения уведомлений: ${error}`);
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
     * @returns {Promise<Object|null>} Уведомление или null
     * @example
     * const notification = await client.getNotificationById('123e4567-e89b-12d3-a456-426614174000');
     * if (notification) {
     *     console.log('Заголовок:', notification.title);
     * }
     */
    async getNotificationById(id) {
        try {
            const response = await fetch(this._getUrl(`/${id}`), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    return null;
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
     * @returns {Promise<Array>} Массив уведомлений пользователя
     * @example
     * const userNotifications = await client.getNotificationsByUserId('123e4567-e89b-12d3-a456-426614174000');
     * console.log('Уведомлений для пользователя:', userNotifications.length);
     */
    async getNotificationsByUserId(userId) {
        try {
            const response = await fetch(this._getUrl(`/user/${userId}`), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка получения уведомлений пользователя: ${error}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении уведомлений пользователя:', error);
            throw error;
        }
    }

    /**
     * Создать новое уведомление
     * @param {Object} notificationData - Данные для создания уведомления
     * @param {string} notificationData.title - Заголовок уведомления
     * @param {string} notificationData.message - Текст уведомления
     * @param {string} notificationData.type - Тип уведомления
     * @param {string} notificationData.createdBy - GUID создателя уведомления
     * @param {string} [notificationData.imageUrl] - URL изображения (опционально)
     * @param {string[]} notificationData.userIdList - Массив GUID получателей
     * @returns {Promise<Object>} Созданное уведомление
     * @example
     * const notification = await client.createNotification({
     *     title: 'Новое сообщение',
     *     message: 'У вас новое сообщение от администратора',
     *     type: 'info',
     *     createdBy: '123e4567-e89b-12d3-a456-426614174000',
     *     imageUrl: 'https://example.com/icon.png',
     *     userIdList: [
     *         '223e4567-e89b-12d3-a456-426614174001',
     *         '323e4567-e89b-12d3-a456-426614174002'
     *     ]
     * });
     */
    async createNotification(notificationData) {
        // Валидация обязательных полей
        if (!notificationData.title) {
            throw new Error('Поле title обязательно');
        }
        if (!notificationData.message) {
            throw new Error('Поле message обязательно');
        }
        if (!notificationData.type) {
            throw new Error('Поле type обязательно');
        }
        if (!notificationData.createdBy) {
            throw new Error('Поле createdBy обязательно');
        }
        if (!Array.isArray(notificationData.userIdList) || notificationData.userIdList.length === 0) {
            throw new Error('Поле userIdList должно быть непустым массивом');
        }

        try {
            const response = await fetch(this._getUrl(''), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(notificationData)
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка создания уведомления: ${error}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при создании уведомления:', error);
            throw error;
        }
    }

    /**
     * Получить непрочитанные уведомления для пользователя
     * @param {string} userId - GUID пользователя
     * @returns {Promise<Array>} Массив непрочитанных уведомлений
     * @example
     * const unread = await client.getUnreadNotifications('123e4567-e89b-12d3-a456-426614174000');
     * console.log('Непрочитанных:', unread.length);
     */
    async getUnreadNotifications(userId) {
        try {
            const notifications = await this.getNotificationsByUserId(userId);
            
            // Фильтруем уведомления, у которых есть непрочитанные получатели
            return notifications.filter(notification => 
                notification.recipients && 
                notification.recipients.some(recipient => 
                    recipient.userId === userId && recipient.readAt === null
                )
            );
        } catch (error) {
            console.error('Ошибка при получении непрочитанных уведомлений:', error);
            throw error;
        }
    }

    /**
     * Получить прочитанные уведомления для пользователя
     * @param {string} userId - GUID пользователя
     * @returns {Promise<Array>} Массив прочитанных уведомлений
     * @example
     * const read = await client.getReadNotifications('123e4567-e89b-12d3-a456-426614174000');
     */
    async getReadNotifications(userId) {
        try {
            const notifications = await this.getNotificationsByUserId(userId);
            
            return notifications.filter(notification => 
                notification.recipients && 
                notification.recipients.some(recipient => 
                    recipient.userId === userId && recipient.readAt !== null
                )
            );
        } catch (error) {
            console.error('Ошибка при получении прочитанных уведомлений:', error);
            throw error;
        }
    }

    /**
     * Получить количество непрочитанных уведомлений для пользователя
     * @param {string} userId - GUID пользователя
     * @returns {Promise<number>} Количество непрочитанных уведомлений
     * @example
     * const count = await client.getUnreadCount('123e4567-e89b-12d3-a456-426614174000');
     * document.getElementById('badge').textContent = count;
     */
    async getUnreadCount(userId) {
        try {
            const unread = await this.getUnreadNotifications(userId);
            return unread.length;
        } catch (error) {
            console.error('Ошибка при получении количества непрочитанных:', error);
            throw error;
        }
    }

    /**
     * Установить базовый URL
     * @param {string} newBaseUrl 
     */
    setBaseUrl(newBaseUrl) {
        this.baseUrl = newBaseUrl.endsWith('/') ? newBaseUrl.slice(0, -1) : newBaseUrl;
    }

    /**
     * Получить текущий базовый URL
     * @returns {string}
     */
    getBaseUrl() {
        return this.baseUrl;
    }
}

document.addEventListener('authStateChanged', async () => {    
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const Notification = new NotificationClient();
    }
});

// ============================================
// Примеры использования
// ============================================

/*
// 1. Создание клиента
const notificationClient = new NotificationClient();

// 2. Получение всех уведомлений
const allNotifications = await notificationClient.getAllNotifications();
console.log('Всего уведомлений:', allNotifications.length);

// 3. Получение уведомлений конкретного пользователя
const userId = '123e4567-e89b-12d3-a456-426614174000';
const userNotifications = await notificationClient.getNotificationsByUserId(userId);
console.log('Уведомлений пользователя:', userNotifications.length);

// 4. Получение непрочитанных уведомлений
const unreadNotifications = await notificationClient.getUnreadNotifications(userId);
console.log('Непрочитанных:', unreadNotifications.length);

// 5. Получение количества непрочитанных (для бейджа)
const unreadCount = await notificationClient.getUnreadCount(userId);
document.getElementById('notification-badge').textContent = unreadCount;
document.getElementById('notification-badge').style.display = unreadCount > 0 ? 'block' : 'none';

// 6. Создание нового уведомления
const newNotification = await notificationClient.createNotification({
    title: 'Новое сообщение',
    message: 'Вам пришло новое сообщение от администратора',
    type: 'info', // или 'warning', 'error', 'success'
    createdBy: 'admin-user-guid',
    imageUrl: 'https://example.com/notification-icon.png',
    userIdList: [
        '123e4567-e89b-12d3-a456-426614174000',
        '223e4567-e89b-12d3-a456-426614174001'
    ]
});
console.log('Создано уведомление с ID:', newNotification.id);

// 7. Получение конкретного уведомления по ID
const notification = await notificationClient.getNotificationById(newNotification.id);
console.log('Уведомление:', notification.title, notification.message);

// 8. Пример интеграции в UI
async function updateNotificationUI(userId) {
    try {
        // Получаем количество непрочитанных
        const count = await notificationClient.getUnreadCount(userId);
        
        // Обновляем бейдж
        const badge = document.getElementById('notification-badge');
        badge.textContent = count;
        badge.style.display = count > 0 ? 'inline-block' : 'none';
        
        // Получаем последние уведомления
        const notifications = await notificationClient.getNotificationsByUserId(userId);
        
        // Отображаем в выпадающем списке
        const list = document.getElementById('notification-list');
        list.innerHTML = notifications.slice(0, 5).map(n => `
            <div class="notification-item ${n.recipients.some(r => r.userId === userId && !r.readAt) ? 'unread' : ''}">
                <h4>${n.title}</h4>
                <p>${n.message}</p>
                <small>${new Date(n.createdAt).toLocaleString()}</small>
            </div>
        `).join('');
    } catch (error) {
        console.error('Ошибка обновления UI:', error);
    }
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