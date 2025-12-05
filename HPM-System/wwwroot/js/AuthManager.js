class AuthManager {
    constructor() {
        this.tokenKey = 'hpm_auth_token';
        this.userDataKey = 'hpm_user_data';
        // OIDC UI — напрямую в IdentityServer
        this.identityServerUrl = 'https://localhost:55676';
        // API — через Gateway
        this.gatewayUrl = 'http://localhost:55699';
        this.authApiUrl = `${this.gatewayUrl}/api/account`;
        this.isAuthenticated = false;
        this.userData = null;
        this.initialize();
    }

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

    async exchangeAuthCode(authCode) {
        try {
            const response = await fetch(`${this.authApiUrl}/exchange-auth-code`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ authCode })
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

    async checkStoredToken() {
        const token = localStorage.getItem(this.tokenKey);
        const userData = localStorage.getItem(this.userDataKey);
        if (!token || !userData) {
            this.clearAuthData();
            return false;
        }
        try {
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

    // НЕ ИСПОЛЬЗУЕТСЯ — логин происходит через редирект на IdentityServer
    async login(email, password) {
        console.warn('Метод login() не должен вызываться — используется OIDC-редирект');
        window.location.href = `${this.identityServerUrl}/Auth/Login?returnUrl=${encodeURIComponent(window.location.href)}`;
    }

    // НЕ ИСПОЛЬЗУЕТСЯ — регистрация через редирект
    async register() {
        window.location.href = `${this.identityServerUrl}/Auth/Register?returnUrl=${encodeURIComponent(window.location.href)}`;
    }

    setAuthData(token, userData) {
        this.isAuthenticated = true;
        this.userData = userData;
        localStorage.setItem(this.tokenKey, token);
        localStorage.setItem(this.userDataKey, JSON.stringify(userData));
        this.updateUI();
    }

    clearAuthData() {
        this.isAuthenticated = false;
        this.userData = null;
        localStorage.removeItem(this.tokenKey);
        localStorage.removeItem(this.userDataKey);
        this.updateUI();
    }

    logout() {
        this.clearAuthData();
        this.showNotification('Вы вышли из системы', 'info');
    }

    getAuthToken() {
        return localStorage.getItem(this.tokenKey);
    }

    getAuthHeaders() {
        const token = this.getAuthToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }

    async fetchWithAuth(url, options = {}) {
        const headers = { ...options.headers, ...this.getAuthHeaders() };
        const response = await fetch(url, { ...options, headers });
        if (response.status === 401) {
            console.warn('❌ Получен 401, токен невалиден');
            this.clearAuthData();
            this.showNotification('Сессия истекла. Пожалуйста, войдите снова.', 'error');
        }
        return response;
    }

    updateUI() {
        const authElements = document.querySelectorAll('[data-auth-required]');
        const guestElements = document.querySelectorAll('[data-guest-only]');
        authElements.forEach(el => el.style.display = this.isAuthenticated ? 'block' : 'none');
        guestElements.forEach(el => el.style.display = this.isAuthenticated ? 'none' : 'block');

        if (this.isAuthenticated && this.userData) {
            document.querySelectorAll('[data-user-email]').forEach(el => {
                el.textContent = this.userData.email;
            });
        }

        document.querySelectorAll('[data-login-btn]').forEach(btn => {
            btn.style.display = this.isAuthenticated ? 'none' : 'inline-block';
        });
        document.querySelectorAll('[data-logout-btn]').forEach(btn => {
            btn.style.display = this.isAuthenticated ? 'inline-block' : 'none';
            btn.onclick = () => this.logout();
        });

        document.dispatchEvent(new CustomEvent('authStateChanged', {
            detail: { isAuthenticated: this.isAuthenticated, userData: this.userData }
        }));
    }

    clearAuthCodeFromUrl() {
        const url = new URL(window.location);
        url.searchParams.delete('auth');
        window.history.replaceState(null, '', url);
    }

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
        setTimeout(() => notification.remove(), 5000);
        notification.onclick = () => notification.remove();
    }
}

// Создаём глобальный экземпляр
window.authManager = new AuthManager();

// Удобные глобальные функции
window.isAuthenticated = () => window.authManager.isAuthenticated;
window.getCurrentUser = () => window.authManager.userData;
window.logout = () => window.authManager.logout();
window.apiCall = (url, options) => window.authManager.fetchWithAuth(url, options);