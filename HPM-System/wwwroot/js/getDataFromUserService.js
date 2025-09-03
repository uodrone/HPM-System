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

    async getCarsByUserId(id) {
        try {
            const response = await fetch(`${this.userApiAddress}/api/Cars/by-user/${id}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            const data = await response.json();
            console.log(`Автомобили пользователя ${id}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения автомобилей пользователя ${id}:`, error);
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
                
                const fullName = document.querySelector('[data-user-fullname]');
                const phone = document.querySelector('[data-user-phone]')
                const carsCount = document.querySelector('[data-user-carslist]');

                
                fullName.textContent = `${user.firstName} ${user.lastName} ${user.patronymic}`;
                phone.textContent = user.phoneNumber;

                if (user.cars.length == 0) {
                    carsCount.remove();
                } else if (user.cars.length == 1) {
                    const car = user.cars[0];

                    carsCount.textContent = `${car.color} ${car.mark} ${car.model}, №${car.number}`;
                } else {
                    carsCount.textContent = `${user.cars.length} машины`;
                }
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