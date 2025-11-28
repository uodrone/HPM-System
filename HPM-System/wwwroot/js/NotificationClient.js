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
    _GetUrl(endpoint) {
        return `${this.baseUrl}${this.apiPath}${endpoint}`;
    }

    /**
     * Получить все уведомления
     * @returns {Promise<Array>} Массив всех уведомлений
     * @example
     * const notifications = await client.getAllNotifications();
     * console.log('Всего уведомлений:', notifications.length);
     */
    async GetAllNotifications() {
        try {
            const response = await fetch(this._GetUrl(''), {
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
    async GetNotificationById(id) {
        try {
            const response = await fetch(this._GetUrl(`/${id}`), {
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
    async GetNotificationsByUserId(userId) {
        try {
            const response = await fetch(this._GetUrl(`/user/${userId}`), {
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
     * Получить непрочитанные уведомления для конкретного пользователя
     * @param {string} userId - GUID пользователя
     * @returns {Promise<Array>} Массив непрочитанных уведомлений
     * @example
     * const unread = await client.getUnreadNotificationsByUserId('123e4567-e89b-12d3-a456-426614174000');
     * console.log('Непрочитанных:', unread.length);
     */
    async GetUnreadNotificationsByUserId(userId) {
        try {
            const response = await fetch(this._GetUrl(`/user/${userId}/unread`), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка получения непрочитанных уведомлений: ${error}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении непрочитанных уведомлений пользователя:', error);
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
    async GetUnreadCount(userId) {
        try {
            const response = await fetch(this._GetUrl(`/user/${userId}/unread/count`), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка получения количества непрочитанных: ${error}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении количества непрочитанных уведомлений:', error);
            throw error;
        }
    }

    /**
     * Отметить уведомление как прочитанное (по ID получателя)
     * @param {string} recipientId - GUID записи NotificationUsers
     * @returns {Promise<boolean>} true, если успешно, иначе false
     * @example
     * const result = await client.markAsReadByRecipientId('123e4567-e89b-12d3-a456-426614174000');
     * if (result) console.log('Уведомление отмечено как прочитанное');
     */
    async MarkAsReadByRecipientId(recipientId) {
        try {
            const response = await fetch(this._GetUrl(`/recipient/${recipientId}/mark-read`), {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    return false;
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
     * Отметить уведомление как прочитанное (по ID уведомления и ID пользователя)
     * @param {string} notificationId - GUID уведомления
     * @param {string} userId - GUID пользователя
     * @returns {Promise<boolean>} true, если успешно, иначе false
     * @example
     * const result = await client.markAsReadByIds('123e4567-e89b-12d3-a456-426614174000', '223e4567-e89b-12d3-a456-426614174001');
     * if (result) console.log('Уведомление отмечено как прочитанное');
     */
    async MarkAsReadByIds(notificationId, userId) {
        try {
            const response = await fetch(this._GetUrl(`/notification/${notificationId}/user/${userId}/mark-read`), {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    return false;
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
     * @param {string} userId - GUID пользователя
     * @returns {Promise<number>} Количество отмеченных уведомлений
     * @example
     * const count = await client.markAllAsRead('123e4567-e89b-12d3-a456-426614174000');
     * console.log(`Отмечено: ${count} уведомлений`);
     */
    async MarkAllAsRead(userId) {
        try {
            const response = await fetch(this._GetUrl(`/user/${userId}/mark-all-read`), {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка отметки всех уведомлений как прочитанных: ${error}`);
            }

            const result = await response.json();
            return result.message ? parseInt(result.message.match(/\d+/)[0]) : 0;
        } catch (error) {
            console.error('Ошибка при отметке всех уведомлений как прочитанных:', error);
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
     * @returns {Promise<boolean>} true, если успешно создано, иначе false
     * @example
     * const success = await client.createNotification({
     *     title: 'Новое сообщение',
     *     message: 'У вас новое сообщение от администратора',
     *     type: 'info',
     *     createdBy: '123e4567-e89b-12d3-a456-426614174000',
     *     userIdList: [
     *         '223e4567-e89b-12d3-a456-426614174001',
     *         '323e4567-e89b-12d3-a456-426614174002'
     *     ]
     * });
     */
    async CreateNotification(notificationData) {
        // Валидация обязательных полей
        if (!notificationData.title) {
            throw new Error('Поле title обязательно');
        }
        if (!notificationData.message) {
            throw new Error('Поле message обязательно');
        }
        if (!notificationData.createdBy) {
            throw new Error('Поле createdBy обязательно');
        }
        if (!Array.isArray(notificationData.userIdList) || notificationData.userIdList.length === 0) {
            throw new Error('Поле userIdList должно быть непустым массивом');
        }

        // Подготовка данных с дефолтными значениями
        const payload = {
            title: notificationData.title,
            message: notificationData.message,
            imageUrl: notificationData.imageUrl || null,
            createdBy: notificationData.createdBy,
            type: notificationData.type === 0 ? 0 : 1, // 0 = User, иначе 1 = System
            isReadable: notificationData.isReadable === false ? false : true, // false или true (по умолчанию)
            userIdList: notificationData.userIdList
        };

        try {
            const response = await fetch(this._GetUrl(''), {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const error = await response.text();
                console.error(error);
                return false;
            } else {
                return true;
            }
        } catch (error) {
            console.error('Ошибка при создании уведомления:', error);
            throw error;
        }
    }

    /**
     * Установить базовый URL
     * @param {string} newBaseUrl 
     */
    SetBaseUrl(newBaseUrl) {
        this.baseUrl = newBaseUrl.endsWith('/') ? newBaseUrl.slice(0, -1) : newBaseUrl;
    }

    /**
     * Получить текущий базовый URL
     * @returns {string}
     */
    GetBaseUrl() {
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