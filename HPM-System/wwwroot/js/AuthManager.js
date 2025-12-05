class AuthManager {
    constructor() {
        this.tokenKey = 'hpm_auth_token';
        this.userDataKey = 'hpm_user_data';
        
        // ВАЖНО: Теперь все запросы идут через Gateway!
        this.gatewayUrl = 'http://localhost:55699'; // Порт Gateway
        this.authApiUrl = `${this.gatewayUrl}/api/account`; // AccountController через Gateway
        
        this.isAuthenticated = false;
        this.userData = null;

        this.initialize();
    }

    /**
     * Инициализация менеджера аутентификации
     */
    async initialize() {
        const urlParams = new URLSearchParams(window.location.search);
        const authCode = urlParams.get('auth');

        if (authCode) {
            console.log('Найден код аутентификации в URL');
            await this.exchangeAuthCode(authCode);
            this.clearAuthCodeFromUrl();
        } else {
            await this.checkStoredToken();
        }
    }

    /**
     * Обменивает код аутентификации на токен
     */
    async exchangeAuthCode(authCode) {
        try {
            const response = await fetch(`${this.authApiUrl}/exchange-auth-code`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ authCode: authCode })
            });

            const result = await response.json();

            if (response.ok && result.token) {
                this.setAuthData(result.token, {
                    userId: result.userId,
                    email: result.email,
                    phoneNumber: result.phoneNumber
                });

                console.log('✅ Авторизация успешна через Gateway');
                this.showNotification('Добро пожаловать!', 'success');
                return true;
            } else {
                console.warn('❌ Ошибка при обмене кода:', result.message);
                this.clearAuthData();
                this.showNotification(result.message || 'Ошибка авторизации', 'error');
                return false;
            }
        } catch (error) {
            console.error('❌ Ошибка при обмене кода аутентификации:', error);
            this.clearAuthData();
            this.showNotification('Произошла ошибка при авторизации', 'error');
            return false;
        }
    }

    /**
     * Проверяет сохраненный токен
     */
    async checkStoredToken() {
        const token = localStorage.getItem(this.tokenKey);
        const userData = localStorage.getItem(this.userDataKey);

        if (!token || !userData) {
            this.clearAuthData();
            return false;
        }

        try {
            // Проверяем токен, делая тестовый запрос к защищенному эндпоинту
            const response = await this.fetchWithAuth(`${this.gatewayUrl}/api/users`, {
                method: 'GET'
            });

            if (response.ok) {
                this.setAuthData(token, JSON.parse(userData));
                console.log('✅ Токен валиден, пользователь авторизован');
                return true;
            } else {
                console.log('❌ Токен невалиден, очищаем данные');
                this.clearAuthData();
                return false;
            }
        } catch (error) {
            console.error('❌ Ошибка при проверке токена:', error);
            this.clearAuthData();
            return false;
        }
    }

    /**
     * Логин пользователя
     */
    async login(email, password) {
        try {
            const response = await fetch(`${this.authApiUrl}/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email, password })
            });

            const result = await response.json();

            if (response.ok && result.token) {
                this.setAuthData(result.token, {
                    userId: result.userId,
                    email: result.email,
                    phoneNumber: result.phoneNumber
                });

                console.log('✅ Логин успешен');
                this.showNotification('Вход выполнен успешно!', 'success');
                return { success: true };
            } else {
                console.warn('❌ Ошибка логина:', result.message);
                return { success: false, message: result.message || 'Неверные учетные данные' };
            }
        } catch (error) {
            console.error('❌ Ошибка при логине:', error);
            return { success: false, message: 'Произошла ошибка при входе' };
        }
    }

    /**
     * Регистрация пользователя
     */
    async register(registerData) {
        try {
            const response = await fetch(`${this.authApiUrl}/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(registerData)
            });

            const result = await response.json();

            if (response.ok) {
                console.log('✅ Регистрация успешна');
                this.showNotification('Регистрация успешна! Теперь можете войти.', 'success');
                return { success: true, data: result };
            } else {
                console.warn('❌ Ошибка регистрации:', result.message);
                return { success: false, message: result.message, errors: result.errors };
            }
        } catch (error) {
            console.error('❌ Ошибка при регистрации:', error);
            return { success: false, message: 'Произошла ошибка при регистрации' };
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

        this.updateUI();
    }

    /**
     * Выполняет выход из системы
     */
    logout() {
        this.clearAuthData();
        this.showNotification('Вы вышли из системы', 'info');
        
        // Можно перенаправить на страницу входа
        // window.location.href = '/login';
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
     * Fetch с автоматической авторизацией
     * ИСПОЛЬЗУЙТЕ ЭТОТ МЕТОД ДЛЯ ВСЕХ API ЗАПРОСОВ!
     */
    async fetchWithAuth(url, options = {}) {
        // Добавляем токен к запросу
        const headers = {
            ...options.headers,
            ...this.getAuthHeaders()
        };

        const response = await fetch(url, {
            ...options,
            headers
        });

        // Если получили 401 - токен невалиден, выходим
        if (response.status === 401) {
            console.warn('❌ Получен 401, токен невалиден');
            this.clearAuthData();
            this.showNotification('Сессия истекла. Пожалуйста, войдите снова.', 'error');
        }

        return response;
    }

    /**
     * Обновляет UI в зависимости от состояния авторизации
     */
    updateUI() {
        const authElements = document.querySelectorAll('[data-auth-required]');
        const guestElements = document.querySelectorAll('[data-guest-only]');

        authElements.forEach(element => {
            element.style.display = this.isAuthenticated ? 'block' : 'none';
        });

        guestElements.forEach(element => {
            element.style.display = this.isAuthenticated ? 'none' : 'block';
        });

        if (this.isAuthenticated && this.userData) {
            const userEmailElements = document.querySelectorAll('[data-user-email]');
            userEmailElements.forEach(element => {
                element.textContent = this.userData.email;
            });
        }

        const loginButtons = document.querySelectorAll('[data-login-btn]');
        const logoutButtons = document.querySelectorAll('[data-logout-btn]');

        loginButtons.forEach(btn => {
            btn.style.display = this.isAuthenticated ? 'none' : 'inline-block';
        });

        logoutButtons.forEach(btn => {
            btn.style.display = this.isAuthenticated ? 'inline-block' : 'none';
            btn.onclick = () => this.logout();
        });

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

        setTimeout(() => {
            notification.remove();
        }, 5000);

        notification.onclick = () => notification.remove();
    }
}

// Глобальный экземпляр менеджера аутентификации
window.authManager = new AuthManager();

// Глобальные функции для удобства
window.isAuthenticated = () => window.authManager.isAuthenticated;
window.getCurrentUser = () => window.authManager.userData;
window.logout = () => window.authManager.logout();

// ВАЖНАЯ ФУНКЦИЯ! Используйте её для всех API запросов
window.apiCall = (url, options) => window.authManager.fetchWithAuth(url, options);