// wwwroot/js/auth.js
class AuthManager {
    constructor() {
        this.tokenKey = 'hpm_auth_token';
        this.userDataKey = 'hpm_user_data';
        this.authApiUrl = '/api/auth';
        this.isAuthenticated = false;
        this.userData = null;

        // Автоматически инициализируем при загрузке
        this.initialize();
    }

    /**
     * Инициализация менеджера аутентификации
     */
    async initialize() {
        // Проверяем наличие кода аутентификации в URL
        const urlParams = new URLSearchParams(window.location.search);
        const authCode = urlParams.get('auth');

        if (authCode) {
            console.log('Найден код аутентификации в URL');
            await this.exchangeAuthCode(authCode);

            // Удаляем код из URL после обработки
            this.clearAuthCodeFromUrl();
        } else {
            // Проверяем сохраненный токен
            await this.checkStoredToken();
        }

        // Обновляем UI в зависимости от состояния авторизации
        this.updateUI();
    }

    /**
     * Обменивает код аутентификации на токен
     */
    async exchangeAuthCode(authCode) {
        try {
            const response = await fetch(`${this.authApiUrl}/exchange-code`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ authCode: authCode })
            });

            const result = await response.json();

            if (response.ok && result.isAuthenticated) {
                this.setAuthData(result.token, {
                    userId: result.userId,
                    email: result.email,
                    phoneNumber: result.phoneNumber
                });

                console.log('✅ Авторизация успешна');
                this.showNotification('Добро пожаловать!', 'success');
            } else {
                console.warn('❌ Ошибка при обмене кода аутентификации:', result.message);
                this.clearAuthData();
                this.showNotification(result.message || 'Ошибка авторизации', 'error');
            }
        } catch (error) {
            console.error('❌ Ошибка при обмене кода аутентификации:', error);
            this.clearAuthData();
            this.showNotification('Произошла ошибка при авторизации', 'error');
        }
    }

    /**
     * Проверяет сохраненный токен
     */
    async checkStoredToken() {
        const token = localStorage.getItem(this.tokenKey);

        if (!token) {
            this.clearAuthData();
            return;
        }

        try {
            const response = await fetch(`${this.authApiUrl}/validate-token`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ token: token })
            });

            const result = await response.json();

            if (response.ok && result.isAuthenticated) {
                this.setAuthData(token, {
                    userId: result.userId,
                    email: result.email,
                    phoneNumber: result.phoneNumber
                });
                console.log('✅ Токен валиден, пользователь авторизован');
            } else {
                console.log('❌ Токен невалиден, очищаем данные');
                this.clearAuthData();
            }
        } catch (error) {
            console.error('❌ Ошибка при проверке токена:', error);
            this.clearAuthData();
        }
    }

    /**
     * Устанавливает данные аутентификации
     */
    setAuthData(token, userData) {
        this.isAuthenticated = true;
        this.userData = userData;

        localStorage.setItem(this.tokenKey, token);
        localStorage.setItem(this.userDataKey, JSON.stringify(userData));

        // Устанавливаем токен в cookie для серверных запросов
        document.cookie = `auth_token=${token}; path=/; max-age=3600; samesite=strict`;

        this.updateUI();
    }

    /**
     * Очищает данные аутентификации
     */
    clearAuthData() {
        this.isAuthenticated = false;
        this.userData = null;

        localStorage.removeItem(this.tokenKey);
        localStorage.removeItem(this.userDataKey);

        // Удаляем cookie
        document.cookie = 'auth_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';

        this.updateUI();
    }

    /**
     * Выполняет выход из системы
     */
    async logout() {
        this.clearAuthData();
        this.showNotification('Вы вышли из системы', 'info');

        // Перенаправляем на страницу входа IdentityServer
        const identityServerUrl = window.location.protocol + '//' + window.location.hostname + ':55674';
        window.location.href = `${identityServerUrl}/Auth/Login`;
    }

    /**
     * Получает токен для API запросов
     */
    getAuthToken() {
        return localStorage.getItem(this.tokenKey);
    }

    /**
     * Создает заголовки для авторизованных запросов
     */
    getAuthHeaders() {
        const token = this.getAuthToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }

    /**
     * Обновляет UI в зависимости от состояния авторизации
     */
    updateUI() {
        // Показываем/скрываем элементы для авторизованных пользователей
        const authElements = document.querySelectorAll('[data-auth-required]');
        const guestElements = document.querySelectorAll('[data-guest-only]');

        authElements.forEach(element => {
            element.style.display = this.isAuthenticated ? 'block' : 'none';
        });

        guestElements.forEach(element => {
            element.style.display = this.isAuthenticated ? 'none' : 'block';
        });

        // Обновляем информацию о пользователе
        if (this.isAuthenticated && this.userData) {
            const userEmailElements = document.querySelectorAll('[data-user-email]');
            const userIdElements = document.querySelectorAll('[data-user-id]');

            userEmailElements.forEach(element => {
                element.textContent = this.userData.email;
            });

            userIdElements.forEach(element => {
                element.textContent = this.userData.userId;
            });
        }

        // Обновляем состояние кнопок
        const loginButtons = document.querySelectorAll('[data-login-btn]');
        const logoutButtons = document.querySelectorAll('[data-logout-btn]');

        loginButtons.forEach(btn => {
            btn.style.display = this.isAuthenticated ? 'none' : 'inline-block';
        });

        logoutButtons.forEach(btn => {
            btn.style.display = this.isAuthenticated ? 'inline-block' : 'none';
            btn.onclick = () => this.logout();
        });

        // Генерируем кастомное событие для других скриптов
        const authEvent = new CustomEvent('authStateChanged', {
            detail: {
                isAuthenticated: this.isAuthenticated,
                userData: this.userData
            }
        });
        document.dispatchEvent(authEvent);
    }

    /**
     * Удаляет код аутентификации из URL
     */
    clearAuthCodeFromUrl() {
        const url = new URL(window.location);
        url.searchParams.delete('auth');
        window.history.replaceState(null, '', url);
    }

    /**
     * Показывает уведомление пользователю
     */
    showNotification(message, type = 'info') {
        // Создаем простое уведомление
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;

        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 12px 20px;
            border-radius: 4px;
            color: white;
            z-index: 10000;
            font-weight: 500;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            ${type === 'success' ? 'background-color: #10B981;' : ''}
            ${type === 'error' ? 'background-color: #EF4444;' : ''}
            ${type === 'info' ? 'background-color: #3B82F6;' : ''}
        `;

        document.body.appendChild(notification);

        // Автоматически удаляем через 5 секунд
        setTimeout(() => {
            notification.remove();
        }, 5000);

        // Добавляем возможность закрытия по клику
        notification.onclick = () => notification.remove();
    }
}

// Глобальный экземпляр менеджера аутентификации
window.authManager = new AuthManager();

// Полезные глобальные функции
window.isAuthenticated = () => window.authManager.isAuthenticated;
window.getCurrentUser = () => window.authManager.userData;
window.logout = () => window.authManager.logout();