class GetDataFromUserService {
    constructor () {
        this.userApiAddress = 'http://localhost:55680';
    }

    async GetUserById(id) {
        try {
            const response = await fetch(`${this.userApiAddress}/api/Users/${id}`, {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            const data = await response.json();
            console.log(`Пользователь ${id}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения пользователя ${id}:`, error);
        }
    }

    async InsertUserDataToProfile () {
        const userIdLinks = document.querySelectorAll('a[data-user-id]');
        userIdLinks.forEach(element => {
            const link = element.href;
            element.href = `/user/${window.authManager.userData.userId}`;
        });

        try {
            await this.GetUserById(window.authManager.userData.userId).then(user => {
                console.log('Данные пользователя:', user);
                
                // Здесь можно обновить DOM с полными данными пользователя
                // Например, найти элементы и заполнить их данными
                const firstNameElements = document.querySelectorAll('[data-user-firstname]');
                const lastNameElements = document.querySelectorAll('[data-user-lastname]');
                
                firstNameElements.forEach(element => {
                    element.textContent = user.firstName;
                });
                
                lastNameElements.forEach(element => {
                    element.textContent = user.lastName;
                });
            }).catch(error => {
                console.error('Ошибка получения данных пользователя:', error);
            });
        } catch (e) {
            console.log(e);
        }
    }
}

document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const userDataService = new GetDataFromUserService();
        userDataService.InsertUserDataToProfile();
    }
});