import { UserValidator } from './UserValidator.js';
import { Modal } from './Modal.js';

export class UserProfile {
    constructor () {
        this.userApiAddress = 'http://localhost:55680';
        this.validator = new UserValidator();
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

    async getUserByPhone(phone) {
        const phoneValidation = this.validator?.validatePhoneNumber(phone);
        if (!phoneValidation?.isValid) {
            return {
                success: false,
                error: phoneValidation.error || 'Неверный формат номера телефона'
            };
        }

        // 2. Нормализуем номер для отправки (убираем всё кроме + и цифр)
        // Это важно, потому что сервер может не распознать "красивый" формат
        const cleanPhone = phone.replace(/[\s\-\(\)]/g, '');

        // 3. URL-кодируем номер (особенно важно для символа '+')
        const encodedPhone = encodeURIComponent(cleanPhone);
        const url = `${this.userApiAddress}/api/Users/by-phone/${encodedPhone}`;

        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (response.ok) {
                const user = await response.json();
                return user;
            }

            if (response.status === 404) {
                return { success: false, error: 'Пользователь с таким номером не найден' };
            }

            if (response.status === 400) {
                const errorData = await response.json().catch(() => null);
                return {
                    success: false,
                    error: errorData?.Message || 'Некорректный номер телефона'
                };
            }

            // Любая серверная ошибка
            const errorData = await response.json().catch(() => null);
            return {
                success: false,
                error: errorData?.Message || `Ошибка сервера (${response.status})`
            };

        } catch (networkError) {
            console.error('Сетевая ошибка при запросе пользователя по телефону:', networkError);
            return {
                success: false,
                error: 'Нет соединения с сервером'
            };
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

    InsertUserIdToLinks (userId) {
        const userIdLinks = document.querySelectorAll('a[data-user-id]');
        userIdLinks.forEach(element => {
            const link = element.href;
            element.href += userId;
        });
    }

    async InsertUserDataToCardOnMainPage (userId) {
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
        <div class="remove-car" data-action="remove-car-from-user" data-car-id="${car.id}" title="Удалить этот автомобиль">
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
                    <div class="error invisible" data-error="color">Неверный цвет машины</div>
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

    CollectCarsDataFromProfile() {
        let carsData = [];

        // собираем данные по машинам из профиля
        const cars = document.querySelectorAll('.profile-group[data-group="cars"] .car');
        cars.forEach(car => {
            const carData = {
                id: car.dataset.carId || '',
                mark: car.querySelector('input[name="mark"]')?.value || '',
                model: car.querySelector('input[name="model"]')?.value || '',
                color: car.querySelector('input[name="color"]')?.value || '',
                number: car.querySelector('input[name="number"]')?.value || '',
                userId: window.authManager.userData.userId
            };
            carsData.push(carData);
        });

        return carsData;
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

    ShowValidationErrors(errors) {
        // Очищаем все предыдущие ошибки
        document.querySelectorAll('.error').forEach(error => {
            error.classList.add('invisible');
        });

        // Показываем ошибки пользователя
        if (errors.user) {
            Object.keys(errors.user).forEach(field => {
                const errorElement = document.querySelector(`[data-error="${field}"]`);
                if (errorElement) {
                    errorElement.textContent = errors.user[field];
                    errorElement.classList.remove('invisible');
                }
            });
        }

        // Показываем ошибки автомобилей
        if (errors.cars && errors.cars.length > 0) {
            errors.cars.forEach((carErrors, index) => {
                Object.keys(carErrors).forEach(field => {
                    const errorElement = document.querySelector(`.profile-group[data-group="cars"] .car:nth-child(${index + 1}) [data-error="${field}"]`);
                    if (errorElement) {
                        errorElement.textContent = carErrors[field];
                        errorElement.classList.remove('invisible');
                    }
                });
            });
        }
    }

    async UpdateUserToDB (id, userData) {        
        try {
            // Валидация данных пользователя
            const validation = this.validator.validateUserData(userData);
            
            // Собираем данные автомобилей для валидации
            const carsData = this.CollectCarsDataFromProfile();
            
            // Валидация автомобилей
            const carsValidation = this.ValidateCarsData(carsData);
            
            // Объединяем ошибки
            const allErrors = {
                user: validation.errors.user,
                cars: carsValidation.errors
            };

            if (!validation.isValid || !carsValidation.isValid) {
                this.ShowValidationErrors(allErrors);
                Modal.ShowNotification('Исправьте ошибки в форме', 'red');
                return;
            }

            // Если валидация прошла успешно, очищаем ошибки
            document.querySelectorAll('.error').forEach(error => {
                error.classList.add('invisible');
            });

            const response = await fetch(`${this.userApiAddress}/api/Users/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...userData, id })
            });
            if (!response.ok) throw new Error(await response.text());
            console.log(`Пользователь ${id} обновлён`);
            Modal.ShowNotification('Данные пользователя сохранены', 'green');
        } catch (error) {
            console.error(`Ошибка обновления пользователя ${id}:`, error);
            Modal.ShowNotification('Ошибка сохранения данных', 'red');
        }
    }

    ValidateCarsData(carsData) {
        const errors = [];
        let isValid = true;

        carsData.forEach(car => {
            const carValidation = this.validator.validateCar(car);
            if (!carValidation.isValid) {
                errors.push(carValidation.errors);
                isValid = false;
            } else {
                errors.push({});
            }
        });

        // Проверка уникальности номеров
        const duplicateIndices = this.validator.validateUniqueCarNumbers(carsData);
        if (duplicateIndices.length > 0) {
            duplicateIndices.forEach(index => {
                if (!errors[index]) errors[index] = {};
                errors[index].number = 'Номер автомобиля уже существует';
                isValid = false;
            });
        }

        return { isValid, errors };
    }

    ValidateCarInModal() {
        const carData = this.CollectCarsDataFromModal();
        const validation = this.validator.validateCar(carData);
        
        // Очищаем ошибки в модальном окне
        document.querySelectorAll('.car-modal .error').forEach(error => {
            error.classList.add('invisible');
        });

        if (!validation.isValid) {
            Object.keys(validation.errors).forEach(field => {
                const errorElement = document.querySelector(`.car-modal [data-error="${field}"]`);
                if (errorElement) {
                    errorElement.textContent = validation.errors[field];
                    errorElement.classList.remove('invisible');
                }
            });
            return false;
        }
        
        return true;
    }

    async AddCarToUser (userId) {
        // Валидация данных автомобиля
        if (!this.ValidateCarInModal()) {
            Modal.ShowNotification('Исправьте ошибки в форме автомобиля', 'red');
            return;
        }

        try {
            const response = await fetch(`${this.userApiAddress}/api/Cars`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.CollectCarsDataFromModal())
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
            Modal.ShowNotification('Автомобиль успешно добавлен', 'green');

            return data;
        } catch (error) {
            console.error('Ошибка создания автомобиля:', error);
            Modal.ShowNotification('Ошибка добавления автомобиля', 'red');
        }
    }

    async RemoveCarFromUser (carId) {
        try {
            const response = await fetch(`${this.userApiAddress}/api/Cars/${carId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            console.log(`Автомобиль ${carId} удалён`);
            //удоляем строку с отображением авто из профиля
            document.querySelector(`.profile-group .car[data-car-id="${carId}"]`).remove();
            
            Modal.ShowNotification('Автомобиль успешно удалён', 'green');
        } catch (error) {
            console.error(`Ошибка удаления автомобиля ${carId}:`, error);
            Modal.ShowNotification('Ошибка удаления автомобиля', 'red');
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

            document.querySelector(`[data-action="save-user-data"]`).addEventListener('click', () => {
                userProfile.UpdateUserToDB(window.authManager.userData.userId, userProfile.CollectUserDataFromProfile());
            });

            document.querySelector(`[data-action="add-car-to-user"]`).addEventListener('click', () => {
                userProfile.AddCarToUser(window.authManager.userData.userId);
            });

            document.addEventListener('click', (e) => {            
                if (e.target.dataset.action == 'remove-car-from-user') {
                    const carId = e.target.dataset.carId;
                    userProfile.RemoveCarFromUser(carId);
                }
            });
        }

        userProfile.InsertUserIdToLinks(userId);
    }
});