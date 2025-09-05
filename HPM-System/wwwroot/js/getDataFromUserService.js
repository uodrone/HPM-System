class GetDataFromUserService {
    constructor () {
        this.userApiAddress = 'http://localhost:55680';
    }

    async GetUserById(userId) {
        try {
            const response = await fetch(`${this.userApiAddress}/api/Users/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            const data = await response.json();
            console.log(`Пользователь ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения пользователя ${userId}:`, error);
        }
    }

    async getCarsByUserId(userId) {
        try {
            const response = await fetch(`${this.userApiAddress}/api/Cars/by-user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            const data = await response.json();
            console.log(`Автомобили пользователя ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения автомобилей пользователя ${userId}:`, error);
        }
    }

    async InsertUserDataToCardOnMainPage (userId) {
        const userIdLinks = document.querySelectorAll('a.user-link[data-user-id]');
        userIdLinks.forEach(element => {
            const link = element.href;
            element.href = `/user/${userId}`;
        });

        try {
            await this.GetUserById(userId).then(user => {
                
                const fullName = document.querySelector('[data-user-fullname]');
                const phone = document.querySelector('[data-user-phone]')
                const carsCount = document.querySelector('[data-user-carslist]');

                
                fullName.textContent = `${user.firstName} ${user.lastName} ${user.patronymic}`;
                phone.textContent = user.phoneNumber;

                if (user.cars.length == 0) {
                    carsCount.remove();
                } else if (user.cars.length == 1) {
                    const car = user.cars[0];

                    carsCount.textContent = `${car.color} ${car.mark} ${car.model}, ${car.number}`;
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

    async InsertUserDataToProfile (userId) {
        try {
            await this.GetUserById(userId).then(user => {
                const setValue = (id, value) => {
                    document.getElementById(id).value = value !== null && value !== '' ? value : '';
                };
                
                setValue('firstName', user.firstName);
                setValue('lastName', user.lastName);
                setValue('patronymic', user.patronymic);
                setValue('birthday', user.age);
                setValue('phoneNumber', user.phoneNumber);
                setValue('email', user.email);  
                
                user.cars.forEach(car => {
                    this.SetUserCarInProfile(car);
                });
               
            }).catch(error => {
                console.error('Ошибка получения данных пользователя:', error);
            });
        } catch (e) {
            console.log(e);
        }
    }

    SetUserCarInProfile (car) {
        const carTemplate = `
            <div class="car" data-car-id="${car.id}">
                <div class="form-group d-none">
                    <input type="text" placeholder=" " name="car-id" id="car-id-${car.id}" value="${car.id}" />
                    <label for="car-id-${car.id}">id-машины</label>
                </div>
                <div class="form-group">
                    <input type="text" placeholder=" " name="mark" id="mark-${car.id}" value="${car.mark}" />
                    <label for="mark-${car.id}">Марка</label>
                    <div class="error invisible" data-error="mark">Неверная марка машины</div>
                </div>
                <div class="form-group">
                    <input type="text" placeholder=" " name="model" id="model-${car.id}" value="${car.model}" />
                    <label for="model-${car.id}">Модель</label>
                    <div class="error invisible" data-error="model">Неверная модель машины</div>
                </div>
                <div class="form-group">
                    <input type="text" placeholder=" " name="color" id="color-${car.id}" value="${car.color}" />
                    <label for="color-${car.id}">Модель</label>
                    <div class="error invisible" data-error="firstName">Неверная модель машины</div>
                </div>
                <div class="form-group">
                    <input type="text" placeholder=" " name="number" id="number-${car.id}" value="${car.number}" />
                    <label for="number-${car.id}">Номер</label>
                    <div class="error invisible" data-error="number">Неверный номер машины</div>
                </div>
                <div class="form-group d-none">
                    <input type="text" placeholder=" " name="car-user-id" id="car-user-id-${car.id}" value="${car.userId}" />
                    <label for="car-user-id-${car.id}">id пользователя</label>
                </div>
            </div>
        `;

        const carsList = document.querySelector('.cars-list');
        if (carsList) {
            carsList.insertAdjacentHTML('beforeend', carTemplate);
        }
    }

    collectUserDataFromProfile() {
        const cars = document.querySelectorAll('.cars-list .car');
        const carsData = [];

        
        cars.forEach(car => {
            const carId = car.dataset.carId || car.querySelector('input[name="car-id"]')?.value || null;
            const userId = car.querySelector('input[name="car-user-id"]')?.value || null;
            
            const carData = {
                id: carId ? parseInt(carId) : null,
                mark: car.querySelector('input[name="mark"]')?.value || '',
                model: car.querySelector('input[name="model"]')?.value || '',
                color: car.querySelector('input[name="color"]')?.value || '',
                number: car.querySelector('input[name="number"]')?.value || '',
                userId: userId
            };
            
            carsData.push(carData);
        });
        
        
    }
}

document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const userDataService = new GetDataFromUserService();

        if (window.location.pathname == '/') {
            userDataService.InsertUserDataToCardOnMainPage(window.authManager.userData.userId);
        }
        
        if (document.getElementById('user-profile')) {
            userDataService.InsertUserDataToProfile (window.authManager.userData.userId);

            document.querySelectorAll('#user-profile .form-group input').forEach(el => {
                el.addEventListener('change', () => {

                });
            })
        }
    }
});