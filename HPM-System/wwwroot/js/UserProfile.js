import { UserValidator } from './UserValidator.js';
import { Modal } from './Modal.js';

export class UserProfile {
    constructor() {
        // ИЗМЕНЕНИЕ 1: Используем Gateway вместо прямого адреса микросервиса
        this.gatewayUrl = 'http://localhost:55699';
        this.validator = new UserValidator();
    }

    async GetUserById(userId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/users/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            const data = await response.json();
            console.log(`Пользователь ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения пользователя ${userId}:`, error);
            throw error;
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

        // Нормализуем и кодируем номер
        const cleanPhone = phone.replace(/[\s\-\(\)]/g, '');
        const encodedPhone = encodeURIComponent(cleanPhone);
        const url = `${this.gatewayUrl}/api/users/by-phone/${encodedPhone}`;

        try {
            const response = await window.apiCall(url, {
                method: 'GET',
                headers: { 'Accept': 'application/json' }
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
                    error: errorData?.message || errorData?.Message || 'Некорректный номер телефона'
                };
            }

            const errorData = await response.json().catch(() => null);
            return {
                success: false,
                error: errorData?.message || errorData?.Message || `Ошибка сервера (${response.status})`
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
            const response = await window.apiCall(`${this.gatewayUrl}/api/cars/by-user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            const data = await response.json();
            console.log(`Автомобили пользователя ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения автомобилей пользователя ${userId}:`, error);
            throw error;
        }
    }

    InsertUserIdToLinks(userId) {
        const userIdLinks = document.querySelectorAll('a[data-user-id]');
        userIdLinks.forEach(element => {
            // Предполагается, что в href есть плейсхолдер вроде "/profile/" → добавляем ID в конец
            if (!element.href.endsWith(userId)) {
                element.href = element.href.endsWith('/') ? `${element.href}${userId}` : `${element.href}/${userId}`;
            }
        });
    }

    async InsertUserDataToCardOnMainPage(userId) {
        try {
            const user = await this.GetUserById(userId);

            const fullName = document.querySelector('[data-user-fullname]');
            const phone = document.querySelector('[data-user-phone]');
            const carsCount = document.querySelector('[data-user-carslist]');

            if (fullName) fullName.textContent = `${user.firstName} ${user.lastName} ${user.patronymic}`;
            if (phone) phone.textContent = user.phoneNumber;

            if (carsCount) {
                if (user.cars.length === 0) {
                    carsCount.remove();
                } else if (user.cars.length === 1) {
                    const car = user.cars[0];
                    carsCount.textContent = `${car.color} ${car.mark} ${car.model}, ${car.number}`;
                } else {
                    carsCount.textContent = `${user.cars.length} машины`;
                }
            }
        } catch (error) {
            console.error('Ошибка получения данных пользователя для главной страницы:', error);
            Modal.ShowNotification('Ошибка загрузки профиля', 'error');
        }
    }

    async InsertCarsToUserProfile(userId) {
        try {
            const cars = await this.GetCarsByUserId(userId);
            const carsContainer = document.querySelector('.profile-group[data-group="cars"] .cars-list');
            if (!carsContainer) return;

            carsContainer.innerHTML = '';
            cars.forEach(car => {
                this.SetUserCar(car, carsContainer);
            });
        } catch (error) {
            console.error('Ошибка загрузки автомобилей:', error);
        }
    }

    async InsertUserDataToProfile(userId) {
        try {
            const user = await this.GetUserById(userId);

            const setValue = (id, value) => {
                const element = document.getElementById(id);
                if (!element) return;

                if (id === 'birthday' && value) {
                    const date = new Date(value);
                    const year = date.getFullYear();
                    const month = String(date.getMonth() + 1).padStart(2, '0');
                    const day = String(date.getDate()).padStart(2, '0');
                    element.value = `${year}-${month}-${day}`;
                } else {
                    element.value = value ?? '';
                }
            };

            setValue('firstName', user.firstName);
            setValue('lastName', user.lastName);
            setValue('patronymic', user.patronymic);
            setValue('birthday', user.birthday);
            setValue('phoneNumber', user.phoneNumber);
            setValue('email', user.email);

            await this.InsertCarsToUserProfile(userId);
        } catch (error) {
            console.error('Ошибка загрузки данных профиля:', error);
            Modal.ShowNotification('Ошибка загрузки профиля', 'error');
        }
    }

    SetCarTemplate(car) {
        let buttonDelCar = '';
        let disabledOrNot = 'disabled';

        if (car && Object.keys(car).length > 0) {
            buttonDelCar = `
                <div class="remove-car" data-action="remove-car-from-user" data-car-id="${car.id}" title="Удалить этот автомобиль">
                    &#10060;
                </div>`;
        } else {
            // Пустой шаблон для модального окна
            car = {
                id: '',
                mark: '',
                model: '',
                color: '',
                number: '',
                userId: window.authManager.userData.userId
            };
            disabledOrNot = '';
        }

        return `
            <div class="car" data-car-id="${car.id}">
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="mark" id="mark-${car.id || 'new'}" value="${car.mark}" />
                    <label for="mark-${car.id || 'new'}">Марка</label>
                    <div class="error invisible" data-error="mark">Неверная марка машины</div>
                </div>
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="model" id="model-${car.id || 'new'}" value="${car.model}" />
                    <label for="model-${car.id || 'new'}">Модель</label>
                    <div class="error invisible" data-error="model">Неверная модель машины</div>
                </div>
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="color" id="color-${car.id || 'new'}" value="${car.color}" />
                    <label for="color-${car.id || 'new'}">Цвет</label>
                    <div class="error invisible" data-error="color">Неверный цвет машины</div>
                </div>
                <div class="form-group">
                    <input ${disabledOrNot} type="text" placeholder=" " name="number" id="number-${car.id || 'new'}" value="${car.number}" />
                    <label for="number-${car.id || 'new'}">Номер</label>
                    <div class="error invisible" data-error="number">Неверный номер машины</div>
                </div>
                ${buttonDelCar}
            </div>
        `;
    }

    SetUserCar(car, carsList) {
        if (carsList) {
            const carTemplate = this.SetCarTemplate(car);
            carsList.insertAdjacentHTML('beforeend', carTemplate);
        }
    }

    CollectUserDataFromProfile() {
        const userData = {};
        const inputs = document.querySelectorAll('.profile-group[data-group="user"] input');

        inputs.forEach(input => {
            let value = input.value;
            if (input.id === 'birthday' && value) {
                const date = new Date(value);
                date.setHours(0, 0, 0, 0);
                value = date.toISOString();
            }
            userData[input.id] = value;
        });

        return userData;
    }

    CollectCarsDataFromProfile() {
        const carsData = [];
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

    CollectCarsDataFromModal() {
        const car = {};
        const modal = document.querySelector('.car-modal');
        if (!modal) return car;

        car.mark = modal.querySelector('input[name="mark"]')?.value || '';
        car.model = modal.querySelector('input[name="model"]')?.value || '';
        car.color = modal.querySelector('input[name="color"]')?.value || '';
        car.number = modal.querySelector('input[name="number"]')?.value || '';
        car.userId = window.authManager.userData.userId;

        console.log('Добавляемый автомобиль:', car);
        return car;
    }

    ShowValidationErrors(errors) {
        document.querySelectorAll('.error').forEach(el => el.classList.add('invisible'));

        if (errors.user) {
            Object.entries(errors.user).forEach(([field, message]) => {
                const el = document.querySelector(`[data-error="${field}"]`);
                if (el) {
                    el.textContent = message;
                    el.classList.remove('invisible');
                }
            });
        }

        if (errors.cars?.length) {
            errors.cars.forEach((carErrors, index) => {
                Object.entries(carErrors).forEach(([field, message]) => {
                    const el = document.querySelector(
                        `.profile-group[data-group="cars"] .car:nth-child(${index + 1}) [data-error="${field}"]`
                    );
                    if (el) {
                        el.textContent = message;
                        el.classList.remove('invisible');
                    }
                });
            });
        }
    }

    async UpdateUserToDB(id, userData) {
        try {
            const userValidation = this.validator.validateUserData(userData);
            const carsData = this.CollectCarsDataFromProfile();
            const carsValidation = this.ValidateCarsData(carsData);

            const allErrors = {
                user: userValidation.errors.user,
                cars: carsValidation.errors
            };

            if (!userValidation.isValid || !carsValidation.isValid) {
                this.ShowValidationErrors(allErrors);
                Modal.ShowNotification('Исправьте ошибки в форме', 'error');
                return;
            }

            // Очищаем ошибки перед сохранением
            document.querySelectorAll('.error').forEach(el => el.classList.add('invisible'));

            const response = await window.apiCall(`${this.gatewayUrl}/api/users/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...userData, id })
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            console.log(`Пользователь ${id} обновлён`);
            Modal.ShowNotification('Данные пользователя сохранены', 'success');
        } catch (error) {
            console.error(`Ошибка обновления пользователя ${id}:`, error);
            Modal.ShowNotification('Ошибка сохранения данных', 'error');
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

        document.querySelectorAll('.car-modal .error').forEach(el => el.classList.add('invisible'));

        if (!validation.isValid) {
            Object.entries(validation.errors).forEach(([field, message]) => {
                const el = document.querySelector(`.car-modal [data-error="${field}"]`);
                if (el) {
                    el.textContent = message;
                    el.classList.remove('invisible');
                }
            });
            return false;
        }

        return true;
    }

    async AddCarToUser(userId) {
        if (!this.ValidateCarInModal()) {
            Modal.ShowNotification('Исправьте ошибки в форме автомобиля', 'error');
            return;
        }

        try {
            const carData = this.CollectCarsDataFromModal();
            const response = await window.apiCall(`${this.gatewayUrl}/api/cars`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(carData)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            const data = await response.json();
            console.log('Автомобиль создан:', data);

            await this.InsertCarsToUserProfile(userId);

            // Очистка модального окна
            document.querySelectorAll('.car-modal input').forEach(input => input.value = '');
            const modal = document.querySelector('.car-modal')?.closest('.modal-overview');
            if (modal) modal.classList.remove('active');

            Modal.ShowNotification('Автомобиль успешно добавлен', 'success');
            return data;
        } catch (error) {
            console.error('Ошибка создания автомобиля:', error);
            Modal.ShowNotification('Ошибка добавления автомобиля', 'error');
        }
    }

    async RemoveCarFromUser(carId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/cars/${carId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            console.log(`Автомобиль ${carId} удалён`);
            const carElement = document.querySelector(`.profile-group .car[data-car-id="${carId}"]`);
            if (carElement) carElement.remove();

            Modal.ShowNotification('Автомобиль успешно удалён', 'success');
        } catch (error) {
            console.error(`Ошибка удаления автомобиля ${carId}:`, error);
            Modal.ShowNotification('Ошибка удаления автомобиля', 'error');
        }
    }
}

// Инициализация при авторизации
document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const userProfile = new UserProfile();
        const userId = window.authManager.userData.userId;

        if (window.location.pathname === '/') {
            userProfile.InsertUserDataToCardOnMainPage(userId);
        }

        if (document.getElementById('user-profile')) {
            userProfile.InsertUserDataToProfile(userId);

            document.querySelector('[data-action="save-user-data"]')?.addEventListener('click', () => {
                userProfile.UpdateUserToDB(userId, userProfile.CollectUserDataFromProfile());
            });

            document.querySelector('[data-action="add-car-to-user"]')?.addEventListener('click', () => {
                userProfile.AddCarToUser(userId);
            });

            document.addEventListener('click', (e) => {
                if (e.target.dataset.action === 'remove-car-from-user') {
                    const carId = e.target.dataset.carId;
                    userProfile.RemoveCarFromUser(carId);
                }
            });
        }

        userProfile.InsertUserIdToLinks(userId);
    }
});