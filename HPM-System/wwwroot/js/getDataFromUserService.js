import { UserValidator } from './UserValidator.js';

class UserProfile {
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

    async GetCarsByUserId(userId) {
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

    async InsertCarsToUserProfile (userId) {
        const cars = await this.GetCarsByUserId(userId);
        const carsContainer = document.querySelector('.profile-group[data-group="cars"] .cars-list');
        carsContainer.innerHTML = '';     
        cars.forEach(car => {
            this.SetUserCar(car, carsContainer);
        });
    }

    async InsertUserDataToProfile (userId) {
        try {
            await this.GetUserById(userId).then(user => {
                const setValue = (id, value) => {
                    const element = document.getElementById(id);
                    if (element) {
                        // Специальная обработка для даты рождения
                        if (id === 'birthday' && value) {
                            // Преобразуем ISO строку в формат YYYY-MM-DD
                            const date = new Date(value);
                            const year = date.getFullYear();
                            const month = String(date.getMonth() + 1).padStart(2, '0');
                            const day = String(date.getDate()).padStart(2, '0');
                            element.value = `${year}-${month}-${day}`;
                        } else {
                            element.value = value !== null && value !== '' ? value : '';
                        }
                    }
                };
                
                setValue('firstName', user.firstName);
                setValue('lastName', user.lastName);
                setValue('patronymic', user.patronymic);
                setValue('birthday', user.birthday);
                setValue('phoneNumber', user.phoneNumber);
                setValue('email', user.email);
            }).catch(error => {
                console.error('Ошибка получения данных пользователя:', error);
            });

            await this.InsertCarsToUserProfile(userId);
        } catch (e) {
            console.log(e);
        }
    }

    SetCarTemplate (car) {
        let buttonDelCar = `
        <div class="remove-car" title="Удалить этот автомобиль">
            &#10060;
        </div>`;
        let disabledOrNot = 'disabled';

        if (!Object.keys(car).length) {
            car.id = '';
            car.mark = '';
            car.model = '';
            car.color = '';
            car.number = '';
            car.userId = window.authManager.userData.userId;

            buttonDelCar = '';
            disabledOrNot = '';
        }

        

        const carTemplate = `
            <div class="car" data-car-id="${car.id}">
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="mark" id="mark-${car.id}" value="${car.mark}" />
                    <label for="mark-${car.id}">Марка</label>
                    <div class="error invisible" data-error="mark">Неверная марка машины</div>
                </div>
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="model" id="model-${car.id}" value="${car.model}" />
                    <label for="model-${car.id}">Модель</label>
                    <div class="error invisible" data-error="model">Неверная модель машины</div>
                </div>
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="color" id="color-${car.id}" value="${car.color}" />
                    <label for="color-${car.id}">Цвет</label>
                    <div class="error invisible" data-error="firstName">Неверный цвет машины</div>
                </div>
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="number" id="number-${car.id}" value="${car.number}" />
                    <label for="number-${car.id}">Номер</label>
                    <div class="error invisible" data-error="number">Неверный номер машины</div>
                </div>
                ${buttonDelCar}
            </div>
        `;

        return carTemplate;
    }

    SetUserCar (car, carsList) {
        
        let carTemplate = this.SetCarTemplate(car);        
       
        if (carsList) {
            carsList.insertAdjacentHTML('beforeend', carTemplate);
        }
    }

    CollectUserDataFromProfile() {
        let userData = {};

        //Собираем данные по пользователю
        const userProfileInputs = document.querySelectorAll('.profile-group[data-group="user"] input');
        userProfileInputs.forEach(input => {
            const inputKey = input.id;
            let inputValue = input.value;

            // Обработка даты рождения
            if (inputKey === 'birthday' && inputValue) {
                // Преобразуем дату в формат ISO с UTC
                let date = new Date(inputValue);
                // Устанавливаем время в 00:00:00 и конвертируем в UTC
                date.setHours(0, 0, 0, 0);
                inputValue = date.toISOString();
            }

            userData[inputKey] = inputValue;
        });

        return userData;
    }

    CollectCarsDataFromModal () {
        let carData = {};

        // собираем данные по машинам
        let cars = document.querySelectorAll('.car-modal .car');
        cars.forEach(car => {            
            carData = {
                mark: car.querySelector('input[name="mark"]')?.value || '',
                model: car.querySelector('input[name="model"]')?.value || '',
                color: car.querySelector('input[name="color"]')?.value || '',
                number: car.querySelector('input[name="number"]')?.value || '',
                userId: window.authManager.userData.userId
            };
        });

        console.log(`Добавляемые машины:`);
        console.log(carData);
        return carData;
    }

    async UpdateUserToDB (id, userData) {        
        try {
            const response = await fetch(`${this.userApiAddress}/api/Users/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...userData, id })
            });
            if (!response.ok) throw new Error(await response.text());
            console.log(`Пользователь ${id} обновлён`);
        } catch (error) {
            console.error(`Ошибка обновления пользователя ${id}:`, error);
        }
    }

    async AddCarToUser (userId) {
        try {
            const response = await fetch(`${this.userApiAddress}/api/Cars`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.CollectCarsDataFromModal ())
            });
            if (!response.ok) throw new Error(await response.text());
            const data = await response.json();
            console.log('Автомобиль создан:', data);
            //инсертим автомобиль в список
            await this.InsertCarsToUserProfile(userId);
            //зачищаем модалку и закрываем её
            document.querySelectorAll('.car-modal input').forEach(input => {
                input.value = '';
            });
            document.querySelector('.car-modal').closest('.modal-overview').classList.remove('active');
            return data;
        } catch (error) {
            console.error('Ошибка создания автомобиля:', error);
        }
    }
}

document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const userProfile = new UserProfile();
        const userId = window.authManager.userData.userId;

        if (window.location.pathname == '/') {
            userProfile.InsertUserDataToCardOnMainPage(userId);
        }
        
        if (document.getElementById('user-profile')) {
            userProfile.InsertUserDataToProfile (userId);

            document.querySelector(`.btn[data-action="save-user-data"]`).addEventListener('click', () => {
                userProfile.UpdateUserToDB(window.authManager.userData.userId, userProfile.CollectUserDataFromProfile());
            });

            document.querySelector(`.btn[data-action="add-car-to-user"]`).addEventListener('click', () => {
                userProfile.AddCarToUser(window.authManager.userData.userId);
            });
        }
    }
});