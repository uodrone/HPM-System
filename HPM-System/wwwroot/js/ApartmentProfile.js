import {ApartmentStatuses} from './ApartmentStatuses.js';
import {ApartmentHouses} from './ApartmentHouses.js';
import {UserProfile} from './UserProfile.js';
import {UserValidator} from './UserValidator.js';
import { Modal } from './Modal.js';

export class ApartmentProfile {
    constructor () {
        this.ApartmentAPIAddress = 'https://localhost:55683';
        this.House = new ApartmentHouses();
        this.userValidator = new UserValidator();
        this.userProfile = new UserProfile();
    }

    //Вставить данные о квартирах пользователя в карточку на главной странице
    async InsertApartmentDataToCardOnMainPage(userId) {
        try {
            // 1. Получаем квартиры пользователя
            const apartments = await this.GetApartmentsByUserId(userId);
            if (!apartments || apartments.length === 0) {
                document.querySelector('.apartments-card .apartments-list').innerHTML = '<p>Нет привязанных квартир</p>';
                return;
            }

            // 2. Собираем уникальные houseId
            const houseIds = [...new Set(apartments.map(a => a.houseId))];

            // 3. Параллельно загружаем все дома
            const housePromises = houseIds.map(id => this.House.GetHouse(id));
            const houses = await Promise.all(housePromises);

            // 4. Создаём мапу houseId → house для быстрого доступа
            const houseMap = new Map();
            houses.forEach(house => {
                houseMap.set(house.id, house);
            });

            // 5. Сопоставляем квартиры с домами и сортируем по номеру дома
            const apartmentWithHouse = apartments
                .map(apartment => ({
                    apartment,
                    house: houseMap.get(apartment.houseId)
                }))
                .filter(item => item.house) // на случай, если дом не найден
                .sort((a, b) => {
                    // Сравниваем по номеру дома
                    const numA = typeof a.house.number === 'string' 
                        ? parseInt(a.house.number, 10) || 0 
                        : a.house.number;
                    const numB = typeof b.house.number === 'string' 
                        ? parseInt(b.house.number, 10) || 0 
                        : b.house.number;
                    return numA - numB;
                });

            // 6. Генерируем HTML
            const apartmentsListContainer = document.querySelector('.apartments-card .apartments-list');
            apartmentsListContainer.innerHTML = '';

            for (const { apartment, house } of apartmentWithHouse) {
                const apartmentTemplate = this.SetApartmentTemplateForMainPage(apartment, house);
                apartmentsListContainer.insertAdjacentHTML('beforeend', apartmentTemplate);
            }
        } catch (error) {
            console.error('Ошибка при загрузке данных квартиры на главную страницу:', error);
            // Опционально: показать сообщение пользователю
            document.querySelector('.apartments-card .apartments-list').innerHTML = '<p>Ошибка загрузки данных</p>';
        }
    }

    SetApartmentTemplateForMainPage (apartment, house) {
        let apartmentHTML;
        if (apartment) {
            apartmentHTML = `
                <div class="apartment-item" data-apartment-id="${apartment.id}">
                    <div class="apartment-address">${house.city}, улица ${house.street}, дом ${house.number}</div>
                    <div class="apartment-details">
                        ${house.isApartmentBuilding ? `<div class="apartment-detail">
                            <div class="detail-label">Номер квартиры</div>
                            <div class="detail-value">${apartment.number}</div>
                        </div>` : ''}
                        
                        <div class="apartment-detail">
                            <div class="detail-label">Число комнат</div>
                            <div class="detail-value">${apartment.numbersOfRooms}</div>
                        </div>

                        <div class="apartment-detail">
                            <div class="detail-label">Общая площадь</div>
                            <div class="detail-value">${apartment.totalArea}</div>
                        </div>

                        <div class="apartment-detail">
                            <div class="detail-label">Жилая площадь</div>
                            <div class="detail-value">${apartment.residentialArea}</div>
                        </div>
                    </div>
                </div>
            `;
        }        

        return apartmentHTML;
    }

    SetApartmentTemplateForAllApartments (apartment, house) {
        let apartmentHTML;
        if (apartment) {
            apartmentHTML = `
                <div class="profile-group dashboard-card my-4" data-group="apartment" data-apartment-id="${apartment.id}" data-apartment-house="${house.id}">
                    <h3 class="card-header card-header_apartment w-100">Профиль квартиры</h3>

                    <div class="d-flex flex-wrap flex-lg-nowrap gap-4 mt-4 w-100">
                        <div class="form-group">
                            <input type="number" disabled placeholder="" min="1" max="10000" name="number" id="number" value="${apartment.number}">
                            <label for="number">Номер квартиры</label>
                            <div class="error invisible" data-error="number">Неверный номер</div>
                        </div>
                        <div class="form-group">
                            <input type="number" disabled placeholder="" min="1" max="100" name="numbersOfRooms" id="numbersOfRooms" value="${apartment.numbersOfRooms}">
                            <label for="numbersOfRooms">Число комнат</label>
                            <div class="error invisible" data-error="numbersOfRooms">Неверное число комнат</div>
                        </div>
                        <div class="form-group">
                            <input type="number" disabled placeholder="" min="1" max="100" name="entranceNumber" id="entranceNumber" value="${apartment.entranceNumber}">
                            <label for="entranceNumber">Номер подъезда</label>
                            <div class="error invisible" data-error="entranceNumber">Неверный номер подъезда</div>
                        </div>
                    </div>
                    <div class="d-flex flex-wrap flex-lg-nowrap gap-4 w-100">
                        <div class="form-group">
                            <input type="number" disabled placeholder="" min="1" max="200" name="floor" id="floor" value="${apartment.floor}">
                            <label for="floor">Этаж</label>
                            <div class="error invisible" data-error="floor">Неверный этаж</div>
                        </div>
                        <div class="form-group">
                            <input type="number" disabled step="0.1" min="1" max="10000" placeholder="" name="totalArea" id="totalArea" value="${apartment.floor}">
                            <label for="totalArea">Общая площадь</label>
                            <div class="error invisible" data-error="totalArea">Неверная общая площадь</div>
                        </div>
                        <div class="form-group">
                            <input type="number" disabled step="0.1" min="1" max="10000" placeholder="" name="residentialArea" id="residentialArea" value="${apartment.residentialArea}">
                            <label for="residentialArea">Жилая площадь</label>
                            <div class="error invisible" data-error="residentialArea">Неверная жилая площадь</div>
                        </div>
                    </div>
                    <div class="d-flex flex-wrap flex-lg-nowrap gap-4 w-100">
                        <div class="form-group">
                            <input type="number" disabled min="0" max="30" placeholder="" name="apartmentUsers" id="apartmentUsers" value="${apartment.users.length}">
                            <label for="apartmentUsers">Количество пользователей квартиры</label>
                            <div class="error invisible" data-error="apartmentUsers">Неверное количество пользователей</div>
                        </div>
                        <div class="form-group">
                            <input type="number" disabled min="0" max="30" placeholder="" name="apartmentOwners" id="apartmentOwners" value="${apartment.users.length}">
                            <label for="apartmentOwners">Количество владельцев квартиры</label>
                            <div class="error invisible" data-error="apartmentOwners">Неверное количество владельцев</div>
                        </div>
                        <div class="form-group">
                            <input id="houseAddress" placeholder="" disabled value="${house.city}, ул. ${house.street} ${house.number}">
                            <label for="houseAddress">Адрес дома</label>
                            <a href="/house/${house.id}">Перейти к профилю дома</a>
                        </div>
                    </div>
                    <a href="/apartment/${apartment.id}">Перейти к профилю квартиры</a>
                </div>
            `;
        }        

        return apartmentHTML;
    }

    // Редактирование профиля квартиры
    async EditApartmentProfile (apartmentId) {
        const apartment = await this.GetApartment(apartmentId);
        const apartmentsShare = await this.GetApartmentShares(apartmentId);     
        const users = apartment.users;
        const house = await this.House.GetHouse(apartment.houseId);        
        let apartmenUsertList = document.querySelector('[data-group="apartment-users"] .apartment-user-list');
        let houseContainer = document.getElementById('houseId');

        apartmenUsertList.innerHTML = '';
        houseContainer.innerHTML = '';
        

        document.getElementById('number').value = apartment.number;
        document.getElementById('numbersOfRooms').value = apartment.numbersOfRooms;
        document.getElementById('entranceNumber').value = apartment.entranceNumber;
        document.getElementById('floor').value = apartment.floor;
        document.getElementById('totalArea').value = apartment.totalArea;
        document.getElementById('residentialArea').value = apartment.residentialArea;

        const option = document.createElement('option');
        option.value = apartment.houseId;
        option.textContent = `${house.city}, ул. ${house.street} ${house.number} `;
        houseContainer.appendChild(option);

        for (const user of users) {
            const shareEntry = apartmentsShare.find(s => s.userId === user.userId);
            const share = shareEntry ? shareEntry.share : '';
            const usersTemplate = this.SetApartmentUserTemplate(user, share);
            apartmenUsertList.insertAdjacentHTML('beforeend', usersTemplate);

            const multiselect = new window.Multiselect();
            multiselect.init(`statuses-${user.userId}`);
        }
    }

    SetApartmentUserTemplate(apartmentUser, share) {
        if (!apartmentUser) return '';

        const allStatuses = [
            { id: 1, name: "Владелец" },
            { id: 2, name: "Жилец" },
            { id: 3, name: "Прописан" },
            { id: 4, name: "Временно проживающий" }
        ];

        // Получаем множество ID выбранных статусов для быстрой проверки
        const selectedStatusIds = new Set(
            (apartmentUser.statuses || []).map(s => s.id)
        );

        // Генерируем опции: все статусы, но отмечаем выбранные
        const statusOptions = allStatuses
        .map(status => {
            const isSelected = selectedStatusIds.has(status.id) ? ' selected' : '';
            return `<option value="${status.id}"${isSelected}>${status.name}</option>`;
        })
        .join('');

        const apartmentUserHTML = `
            <div class="d-flex flex-wrap flex-lg-nowrap gap-4 mt-4 w-100" data-apartment-user-id="${apartmentUser.userId}">
                <div class="form-group">
                    <input type="text" disabled placeholder="" name="fullName" id="fullName-${apartmentUser.userId}" value="${apartmentUser.userDetails.firstName} ${apartmentUser.userDetails.lastName} ${apartmentUser.userDetails.patronymic}">
                    <label for="fullName-${apartmentUser.userId}">ФИО пользователя</label>
                </div>
                <div class="form-group">
                    <input type="text" disabled placeholder="" name="phoneNumber" id="phoneNumber-${apartmentUser.userId}" value="${apartmentUser.userDetails.phoneNumber}">
                    <label for="phoneNumber-${apartmentUser.userId}">Телефон пользователя</label>
                </div>
                <div class="form-group multiselect">
                    <select id="statuses-${apartmentUser.userId}" multiple>
                        ${statusOptions}
                    </select>                        
                    <label for="statuses-${apartmentUser.userId}">Статус пользователя</label>
                </div>
                <div class="form-group">
                    <input type="number" placeholder="" name="share" min="0" id="share-${apartmentUser.userId}" value="${share}">                      
                    <label for="share-${apartmentUser.userId}">Доля владения</label>
                    <div class="error invisible" data-error="share">Доля владения только для владельцев</div>
                </div>
                <div class="save-icon icon-action" data-status="save" title="Сохранить статусы пользователя">&#128190;</div>
                <div class="remove-icon icon-action" data-status="remove" title="Удалить пользователя">&#10060;</div>                
            </div>
        `;

        return apartmentUserHTML;
    }

    //Получение домов для создания квартиры и привязки к ним
    async SetHouseIdToCreateApartment () {
        let houseId = parseInt(localStorage.getItem('house'));
        const houseProfile = new ApartmentHouses();
        const option = document.createElement('option');
        const userId = window.authManager.userData.userId;
        let houseSelector = document.getElementById('houseId');

        if (!isNaN(houseId))
        {            
            const house = await houseProfile.GetHouse(houseId);                      
            option.value = house.id;
            option.textContent = `${house.city}, ул. ${house.street}, ${house.number}`;
            houseSelector.appendChild(option);
        } else {
            const houses = await houseProfile.GetHousesByUserId(userId);

            houses.forEach(async (house) => {
                let houseHead = await this.GetHead(house.id);

                if (houseHead.id == userId) {
                    option.value = house.id;
                    option.textContent = `${house.city}, ул. ${house.street}, ${house.number}`;
                    houseSelector.appendChild(option);
                } else {
                    document.querySelector('.profile-group[data-group="apartment"]').innerHTML = `Создание квартиры недоступно`;
                }                
            });
        }
    }

    // Коллекционирование данных квартир для сохранения и создания
    async CollectApartmentDataAndSaveToCreate () {
        let apartment = {};       
        
        let number = parseInt(document.getElementById('number')?.value);
        let numbersOfRooms = parseInt(document.getElementById('numbersOfRooms')?.value);
        let entranceNumber = parseInt(document.getElementById('entranceNumber')?.value);
        let floor = parseInt(document.getElementById('floor')?.value);
        let totalArea = parseFloat(document.getElementById('totalArea')?.value);
        let residentialArea = parseFloat(document.getElementById('residentialArea')?.value);        
        let houseId = parseInt(document.getElementById('houseId')?.value);

        apartment.number = number;
        apartment.numbersOfRooms = numbersOfRooms;
        apartment.entranceNumber = entranceNumber;
        apartment.floor = floor;
        apartment.totalArea = totalArea;
        apartment.residentialArea = residentialArea;
        apartment.houseId = houseId;

        console.log(`собранные данные по квартир:`);
        console.log(apartment);

        // Вспомогательная функция: показать/скрыть ошибку
        function showError(field, message = null, show = true) {
            const errorEl = document.querySelector(`[data-error="${field}"]`);
            if (errorEl) {
                if (show) {
                    console.log(`отображаю ошибку для поля: ${field}`);
                }
                errorEl.textContent = message || errorEl.textContent;
                errorEl.classList.toggle('invisible', !show);
            }
        }

        // Основная функция валидации
        function validateApartmentForm() {
            let isValid = true;

            // Валидация номера квартиры
            if (!Number.isInteger(number) || number < 1 || number > 10000) {
                showError('number', 'Номер квартиры должен быть от 1 до 10000');
                isValid = false;
            } else {
                showError('number', null, false);                
            }

            // Валидация числа комнат
            if (!Number.isInteger(numbersOfRooms) || numbersOfRooms < 1 || numbersOfRooms > 100) {
                showError('numbersOfRooms', 'Число комнат должно быть от 1 до 100');
                isValid = false;
            } else {
                showError('numbersOfRooms', null, false);                
            }

            // Валидация номера подъезда
            if (!Number.isInteger(entranceNumber) || entranceNumber < 1 || entranceNumber > 100) {
                showError('entranceNumber', 'Номер подъезда должен быть от 1 до 100');
                isValid = false;
            } else {
                showError('entranceNumber', null, false);                
            }

            // Валидация этажа
            if (!Number.isInteger(floor) || floor < 1 || floor > 200) {
                showError('floor', 'Этаж должен быть от 1 до 200');
                isValid = false;
            } else {
                showError('floor', null, false);                
            }

            // Валидация общей площади
            if (isNaN(totalArea) || totalArea < 1 || totalArea > 10000) {
                showError('totalArea', 'Общая площадь должна быть от 1 до 10000');
                isValid = false;
            } else {
                showError('totalArea', null, false);                
            }

            // Валидация жилой площади
            if (isNaN(residentialArea) || residentialArea < 1 || residentialArea > 10000) {
                showError('residentialArea', 'Жилая площадь должна быть от 1 до 10000');
                isValid = false;
            } else if (residentialArea > totalArea) {
                showError('residentialArea', 'Жилая площадь не может превышать общую');
                isValid = false;
            } else {
                showError('residentialArea', null, false);                
            }

            // Валидация id дом
            if (!houseId) {
                const houseError = document.querySelector('[data-error="houseId"]');
                if (houseError) {
                    houseError.classList.remove('invisible');
                }
                isValid = false;
            } else {
                const houseError = document.querySelector('[data-error="houseId"]');
                if (houseError) houseError.classList.add('invisible');                
            }

            return isValid;
        }
        
        if (validateApartmentForm()) {
            let result = await this.CreateApartment(apartment);
            console.log(result);
        }        
    }

    // Удаление пользователя из квартиры
    RemoveUserFromApartmentAndSave (apartmentId) {
        document.addEventListener('click', async (e) => {
            // Проверяем, был ли клик по элементу с нужным data-атрибутом
            if (e.target.closest('[data-status="remove"]')) {
                const user = e.target.closest('[data-apartment-user-id]');
                const userId = user.dataset.apartmentUserId;
                
                try {
                    let isUserRemovedFromApartmentSuccessfull = await this.RemoveUserFromApartment(apartmentId, userId);
                    if (isUserRemovedFromApartmentSuccessfull) {                        
                        this.EditApartmentProfile(apartmentId);
                        Modal.ShowNotification('Пользователь удален успешно', 'green');                        
                    }
                        
                }
                catch (e) {
                    console.log(e);
                    Modal.ShowNotification('Ошибка удаления пользователя', 'red');
                }                
            }
        });
    }

    // Добавление пользователя к квартире
    AddNewUserToApartment (apartmentId) {
        let modalPhoneError = document.querySelector('[data-error="newPhoneNumber"]');

        document.querySelector('[data-action="add-user-to-apartment"]').addEventListener('click', async () => {
            const phoneNumber = document.getElementById('newPhoneNumber').value;
            if (this.userValidator.validatePhoneNumber(phoneNumber).isValid) {
                modalPhoneError.classList.add('invisible');
                console.log(`телефон валидный`);
                
                const user = await this.userProfile.getUserByPhone(phoneNumber);
                let isAddUserToApartmentSuccessfull = await this.AddUserToApartment(apartmentId, user.id);
                if (isAddUserToApartmentSuccessfull) {
                    Modal.CloseModalImmediately();
                    Modal.ShowNotification('Пользователь успешно добавлен', 'green');
                    this.EditApartmentProfile(apartmentId);
                }
            } else {
                modalPhoneError.classList.remove('invisible');
            }
        })
    }

    //получить квартиры пользователя по ид пользователя
    async GetApartmentsByUserId(userId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log(`Квартиры пользователя ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения квартир пользователя ${userId}:`, error);
        }
    }

    //получить квартиры пользователя по номеру телефона пользователя
    async GetApartmentsByUserPhone(phone) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/phone/${phone}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log(`Квартиры пользователя с телефоном ${phone}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения квартир по телефону ${phone}:`, error);
        }
    }

    //получить квартиру по её id
    async GetApartment(id) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/${id}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log(`Квартира ${id}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения квартиры ${id}:`, error);
        }
    }

    //создать квартиру
    async CreateApartment(apartmentData) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(apartmentData)
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log('Квартира создана:', data);
            return data;
        } catch (error) {
            console.error('Ошибка создания квартиры:', error);
        }
    }

    //удалить квартиру
    async DeleteApartment(id) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/${id}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            console.log(`Квартира ${id} удалена`);
        } catch (error) {
            console.error(`Ошибка удаления квартиры ${id}:`, error);
        }
    }

    //добавить пользователя к квартире
    async AddUserToApartment(apartmentId, userId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/${apartmentId}/users/${userId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify()
            });
            const data = await response.text();
            if (!response.ok) {
                console.log(data);
                console.log(`пользователь ${user.id} добавлен к квартире с id ${apartmentId}`);                
                return false;
                //throw new Error(data)
            } else {
                return true;
            }
            
        } catch (error) {
            console.error('Ошибка добавления пользователя к квартире:', error);
        }
    }

    //удалить пользователя из квартиры
    async RemoveUserFromApartment(apartmentId, userId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/${apartmentId}/users/${userId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.text();
            if (!response.ok) {                
                console.log(data);
                return false;
                //throw new Error(data);
            } else {
                return true;
            }
        } catch (error) {
            console.error('Ошибка удаления пользователя из квартиры:', error);
        }
    }

    //обновить долю владения пользователя
    async UpdateUserShare(apartmentId, userId, share) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/${apartmentId}/users/${userId}/share`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ share })
            });
            const data = await response.text();
            if (!response.ok) {
                console.log(data);
                return false;
            } else {
                return true;
            }
        } catch (error) {
            console.error('Ошибка обновления доли:', error);
        }
    }

    //получить доли владения для квартиры
    async GetApartmentShares(apartmentId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/${apartmentId}/shares`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log(`Доли квартиры ${apartmentId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения долей квартиры ${apartmentId}:`, error);
        }
    }

    //получить статистику по квартире
    async GetApartmentStatistics(apartmentId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/${apartmentId}/statistics`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log(`Статистика квартиры ${apartmentId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения статистики квартиры ${apartmentId}:`, error);
        }
    }
}

document.addEventListener('authStateChanged', async () => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();

    if (isAuthenticated && userData) {
        const apartmentProfile = new ApartmentProfile();
        const userId = window.authManager.userData.userId;

        if (window.location.pathname == '/') {
            apartmentProfile.InsertApartmentDataToCardOnMainPage(userId);
        }

        if (window.location.pathname == '/apartment/create') {
            apartmentProfile.SetHouseIdToCreateApartment ();

            document.querySelector('[data-action="save-apartment-data"]').addEventListener('click', () => {
                apartmentProfile.CollectApartmentDataAndSaveToCreate ();
            });            
        }

        if (Regex.getUrlPathParts(window.location.href).includes('apartment') && Regex.getUrlPathParts(window.location.href).includes(userId)) {

        }

        if (Regex.isValidEntityUrl(window.location.href).valid && Regex.getUrlPathParts(window.location.href).includes('apartment')) {
            const apartmentId = Regex.isValidEntityUrl(window.location.href).id;
            apartmentProfile.EditApartmentProfile(apartmentId);
            apartmentProfile.AddNewUserToApartment(apartmentId);
            apartmentProfile.RemoveUserFromApartmentAndSave(apartmentId);
        }
    }
});