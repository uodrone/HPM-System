import { ApartmentStatuses } from './ApartmentStatuses.js';
import { ApartmentHouses } from './ApartmentHouses.js';
import { UserProfile } from './UserProfile.js';
import { UserValidator } from './UserValidator.js';
import { Modal } from './Modal.js';

export class ApartmentProfile {
    constructor() {
        // ИЗМЕНЕНИЕ 1: Убираем прямой адрес микросервиса, используем Gateway через window.apiCall
        this.gatewayUrl = 'http://localhost:55699';
        this.House = new ApartmentHouses();
        this.userValidator = new UserValidator();
        this.userProfile = new UserProfile();
    }

    // Вставить данные о квартирах пользователя в карточку на главной странице
    async InsertApartmentDataToCardOnMainPage(userId) {
        try {
            const apartments = await this.GetApartmentsByUserId(userId);
            if (!apartments || apartments.length === 0) {
                document.querySelector('.apartments-card .apartments-list').innerHTML = '<p>Нет привязанных квартир</p>';
                return;
            }

            const houseIds = [...new Set(apartments.map(a => a.houseId))];
            const housePromises = houseIds.map(id => this.House.GetHouse(id));
            const houses = await Promise.all(housePromises);

            const houseMap = new Map(houses.map(house => [house.id, house]));

            const apartmentWithHouse = apartments
                .map(apartment => ({
                    apartment,
                    house: houseMap.get(apartment.houseId)
                }))
                .filter(item => item.house)
                .sort((a, b) => {
                    const numA = typeof a.house.number === 'string'
                        ? parseInt(a.house.number, 10) || 0
                        : a.house.number;
                    const numB = typeof b.house.number === 'string'
                        ? parseInt(b.house.number, 10) || 0
                        : b.house.number;
                    return numA - numB;
                });

            const apartmentsListContainer = document.querySelector('.apartments-card .apartments-list');
            apartmentsListContainer.innerHTML = '';

            for (const { apartment, house } of apartmentWithHouse) {
                const apartmentTemplate = this.SetApartmentTemplateForMainPage(apartment, house);
                apartmentsListContainer.insertAdjacentHTML('beforeend', apartmentTemplate);
            }
        } catch (error) {
            console.error('Ошибка при загрузке данных квартиры на главную страницу:', error);
            document.querySelector('.apartments-card .apartments-list').innerHTML = '<p>Ошибка загрузки данных</p>';
        }
    }

    SetApartmentTemplateForMainPage(apartment, house) {
        if (!apartment) return '';
        return `
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

    async InsertApartmentProfileToAllApartments(apartment, house) {
        const container = document.querySelector('.apartments-by-user-list');
        const apartmentTemplate = this.SetApartmentTemplateForAllApartments(apartment, house);
        container.insertAdjacentHTML('beforeend', apartmentTemplate);
    }

    SetApartmentTemplateForAllApartments(apartment, house) {
        if (!apartment) return '';
        return `
            <div class="profile-group dashboard-card my-4" data-group="apartment" data-apartment-id="${apartment.id}" data-apartment-house="${house.id}">
                <h3 class="card-header card-header_apartment w-100"><a href="/apartment/${apartment.id}">${house.city}, ул. ${house.street} ${house.number}, квартира ${apartment.number}</a></h3>
                <div class="d-flex flex-wrap flex-lg-nowrap gap-4 mt-4 w-100">
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
                    <div class="form-group">
                        <input type="number" disabled placeholder="" min="1" max="200" name="floor" id="floor" value="${apartment.floor}">
                        <label for="floor">Этаж</label>
                        <div class="error invisible" data-error="floor">Неверный этаж</div>
                    </div>
                </div>
                <div class="d-flex flex-wrap flex-lg-nowrap gap-4 w-100">
                    <div class="form-group">
                        <input type="number" disabled step="0.1" min="1" max="10000" placeholder="" name="totalArea" id="totalArea" value="${apartment.totalArea}">
                        <label for="totalArea">Общая площадь</label>
                        <div class="error invisible" data-error="totalArea">Неверная общая площадь</div>
                    </div>
                    <div class="form-group">
                        <input type="number" disabled step="0.1" min="1" max="10000" placeholder="" name="residentialArea" id="residentialArea" value="${apartment.residentialArea}">
                        <label for="residentialArea">Жилая площадь</label>
                        <div class="error invisible" data-error="residentialArea">Неверная жилая площадь</div>
                    </div>
                    <div class="form-group" style="max-width: 407px">
                        <input type="number" disabled min="0" max="30" placeholder="" name="apartmentUsers" id="apartmentUsers" value="${apartment.users.length}">
                        <label for="apartmentUsers">Количество пользователей квартиры</label>
                        <div class="error invisible" data-error="apartmentUsers">Неверное количество пользователей</div>
                    </div>
                </div>
                <a href="/apartment/${apartment.id}">Перейти к профилю квартиры</a>
            </div>
        `;
    }

    async EditApartmentProfile(apartmentId) {
        try {
            const apartment = await this.GetApartment(apartmentId);
            const apartmentsShare = await this.GetApartmentShares(apartmentId);
            const users = apartment.users;
            const house = await this.House.GetHouse(apartment.houseId);
            const apartmenUsertList = document.querySelector('[data-group="apartment-users"] .apartment-user-list');
            const houseContainer = document.getElementById('houseId');

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
        } catch (error) {
            console.error('Ошибка при редактировании профиля квартиры:', error);
            Modal.ShowNotification('Ошибка загрузки данных квартиры', 'error');
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

        const selectedStatusIds = new Set(
            (apartmentUser.statuses || []).map(s => s.id)
        );

        const statusOptions = allStatuses
            .map(status => {
                const isSelected = selectedStatusIds.has(status.id) ? ' selected' : '';
                return `<option value="${status.id}"${isSelected}>${status.name}</option>`;
            })
            .join('');

        return `
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
                    <input type="number" placeholder="" name="share" min="0" step="0.1" id="share-${apartmentUser.userId}" value="${share}">
                    <label for="share-${apartmentUser.userId}">Доля владения</label>
                    <div class="error invisible" data-error="share">Доля владения только для владельцев</div>
                </div>
                <div class="save-icon icon-action" data-status="save" title="Сохранить статусы пользователя">&#128190;</div>
                <div class="remove-icon icon-action" data-status="remove" title="Удалить пользователя">&#10060;</div>
            </div>
        `;
    }

    async SetHouseIdToCreateApartment() {
        const userId = window.authManager.userData.userId;
        const houseSelector = document.getElementById('houseId');
        const houseIdFromStorage = parseInt(localStorage.getItem('house'), 10);

        if (!isNaN(houseIdFromStorage)) {
            const house = await this.House.GetHouse(houseIdFromStorage);
            if (house) {
                const option = document.createElement('option');
                option.value = house.id;
                option.textContent = `${house.city}, ул. ${house.street}, ${house.number}`;
                houseSelector.appendChild(option);
                return;
            }
        }

        const houses = await this.House.GetHousesByUserId(userId);
        let hasEditableHouse = false;
        const option = document.createElement('option');

        for (const house of houses) {
            const houseHead = await this.House.GetHead(house.id);
            if (houseHead?.id === userId) {
                option.value = house.id;
                option.textContent = `${house.city}, ул. ${house.street}, ${house.number}`;
                houseSelector.appendChild(option.cloneNode(true));
                hasEditableHouse = true;
            }
        }

        if (!hasEditableHouse) {
            document.querySelector('.profile-group[data-group="apartment"]').innerHTML = `Создание квартиры недоступно`;
        }
    }

    async CollectApartmentDataAndSaveToCreate() {
        const fields = {
            number: parseInt(document.getElementById('number')?.value, 10),
            numbersOfRooms: parseInt(document.getElementById('numbersOfRooms')?.value, 10),
            entranceNumber: parseInt(document.getElementById('entranceNumber')?.value, 10),
            floor: parseInt(document.getElementById('floor')?.value, 10),
            totalArea: parseFloat(document.getElementById('totalArea')?.value),
            residentialArea: parseFloat(document.getElementById('residentialArea')?.value),
            houseId: parseInt(document.getElementById('houseId')?.value, 10)
        };

        const apartment = { ...fields };

        console.log(`Собранные данные по квартире:`, apartment);

        function showError(field, message = null, show = true) {
            const errorEl = document.querySelector(`[data-error="${field}"]`);
            if (errorEl) {
                errorEl.textContent = message || errorEl.textContent;
                errorEl.classList.toggle('invisible', !show);
            }
        }

        function validateApartmentForm() {
            let isValid = true;

            // Валидация номера квартиры
            if (!Number.isInteger(fields.number) || fields.number < 1 || fields.number > 10000) {
                showError('number', 'Номер квартиры должен быть от 1 до 10000');
                isValid = false;
            } else showError('number', null, false);

            // Валидация числа комнат
            if (!Number.isInteger(fields.numbersOfRooms) || fields.numbersOfRooms < 1 || fields.numbersOfRooms > 100) {
                showError('numbersOfRooms', 'Число комнат должно быть от 1 до 100');
                isValid = false;
            } else showError('numbersOfRooms', null, false);

            // Валидация подъезда
            if (!Number.isInteger(fields.entranceNumber) || fields.entranceNumber < 1 || fields.entranceNumber > 100) {
                showError('entranceNumber', 'Номер подъезда должен быть от 1 до 100');
                isValid = false;
            } else showError('entranceNumber', null, false);

            // Валидация этажа
            if (!Number.isInteger(fields.floor) || fields.floor < 1 || fields.floor > 200) {
                showError('floor', 'Этаж должен быть от 1 до 200');
                isValid = false;
            } else showError('floor', null, false);

            // Общая площадь
            if (isNaN(fields.totalArea) || fields.totalArea < 1 || fields.totalArea > 10000) {
                showError('totalArea', 'Общая площадь должна быть от 1 до 10000');
                isValid = false;
            } else showError('totalArea', null, false);

            // Жилая площадь
            if (isNaN(fields.residentialArea) || fields.residentialArea < 1 || fields.residentialArea > 10000) {
                showError('residentialArea', 'Жилая площадь должна быть от 1 до 10000');
                isValid = false;
            } else if (fields.residentialArea > fields.totalArea) {
                showError('residentialArea', 'Жилая площадь не может превышать общую');
                isValid = false;
            } else showError('residentialArea', null, false);

            // Дом
            if (!fields.houseId) {
                const houseError = document.querySelector('[data-error="houseId"]');
                if (houseError) houseError.classList.remove('invisible');
                isValid = false;
            } else {
                const houseError = document.querySelector('[data-error="houseId"]');
                if (houseError) houseError.classList.add('invisible');
            }

            return isValid;
        }

        if (validateApartmentForm()) {
            const result = await this.CreateApartment(apartment);
            if (result) {
                Modal.ShowNotification('Квартира успешно создана', 'success');
                window.location.href = `/apartment/${result.id}`;
            } else {
                Modal.ShowNotification('Ошибка создания квартиры', 'error');
            }
        }
    }

    RemoveUserFromApartmentAndSave(apartmentId) {
        document.addEventListener('click', async (e) => {
            if (e.target.closest('[data-status="remove"]')) {
                const user = e.target.closest('[data-apartment-user-id]');
                const userId = user.dataset.apartmentUserId;

                try {
                    const success = await this.RemoveUserFromApartment(apartmentId, userId);
                    if (success) {
                        await this.EditApartmentProfile(apartmentId);
                        Modal.ShowNotification('Пользователь удален успешно', 'success');
                    } else {
                        Modal.ShowNotification('Не удалось удалить пользователя', 'error');
                    }
                } catch (error) {
                    console.error('Ошибка удаления пользователя:', error);
                    Modal.ShowNotification('Ошибка удаления пользователя', 'error');
                }
            }
        });
    }

    AddNewUserToApartment(apartmentId) {
        const modalPhoneError = document.querySelector('[data-error="newPhoneNumber"]');
        document.querySelector('[data-action="add-user-to-apartment"]').addEventListener('click', async () => {
            const phoneNumber = document.getElementById('newPhoneNumber').value;
            if (this.userValidator.validatePhoneNumber(phoneNumber).isValid) {
                modalPhoneError.classList.add('invisible');
                try {
                    const user = await this.userProfile.getUserByPhone(phoneNumber);
                    const success = await this.AddUserToApartment(apartmentId, user.id);
                    if (success) {
                        Modal.CloseModalImmediately();
                        Modal.ShowNotification('Пользователь успешно добавлен', 'success');
                        await this.EditApartmentProfile(apartmentId);
                    } else {
                        Modal.ShowNotification('Не удалось добавить пользователя', 'error');
                    }
                } catch (error) {
                    console.error('Ошибка при добавлении пользователя:', error);
                    Modal.ShowNotification('Ошибка при добавлении пользователя', 'error');
                }
            } else {
                modalPhoneError.classList.remove('invisible');
            }
        });
    }

    // ========================================
    // API МЕТОДЫ — ИСПОЛЬЗУЕМ window.apiCall
    // ========================================

    async GetApartmentsByUserId(userId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error?.message || `Ошибка получения квартир пользователя ${userId}`);
            }
            const data = await response.json();
            console.log(`Квартиры пользователя ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения квартир пользователя ${userId}:`, error);
            throw error;
        }
    }

    async GetApartmentsByUserPhone(phone) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/phone/${phone}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error?.message || `Ошибка получения квартир по телефону ${phone}`);
            }
            const data = await response.json();
            console.log(`Квартиры пользователя с телефоном ${phone}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения квартир по телефону ${phone}:`, error);
            throw error;
        }
    }

    async GetApartment(id) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/${id}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error?.message || `Ошибка получения квартиры ${id}`);
            }
            const data = await response.json();
            console.log(`Квартира ${id}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения квартиры ${id}:`, error);
            throw error;
        }
    }

    async CreateApartment(apartmentData) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(apartmentData)
            });
            if (response.ok) {
                const data = await response.json();
                console.log('Квартира создана:', data);
                return data;
            } else {
                const error = await response.json();
                console.error('Ошибка создания квартиры:', error);
                return null;
            }
        } catch (error) {
            console.error('Ошибка создания квартиры:', error);
            return null;
        }
    }

    async DeleteApartment(id) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/${id}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) {
                const error = await response.text();
                throw new Error(error);
            }
            console.log(`Квартира ${id} удалена`);
            return true;
        } catch (error) {
            console.error(`Ошибка удаления квартиры ${id}:`, error);
            throw error;
        }
    }

    async AddUserToApartment(apartmentId, userId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/${apartmentId}/users/${userId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            });
            if (response.ok) {
                console.log(`Пользователь ${userId} добавлен к квартире ${apartmentId}`);
                return true;
            } else {
                const error = await response.text();
                console.error('Ошибка добавления пользователя:', error);
                return false;
            }
        } catch (error) {
            console.error('Ошибка добавления пользователя к квартире:', error);
            return false;
        }
    }

    async RemoveUserFromApartment(apartmentId, userId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/${apartmentId}/users/${userId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            if (response.ok) {
                console.log(`Пользователь ${userId} удален из квартиры ${apartmentId}`);
                return true;
            } else {
                const error = await response.text();
                console.error('Ошибка удаления пользователя:', error);
                return false;
            }
        } catch (error) {
            console.error('Ошибка удаления пользователя из квартиры:', error);
            return false;
        }
    }

    async UpdateUserShare(apartmentId, userId, share) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/${apartmentId}/users/${userId}/share`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ share })
            });
            if (response.ok) {
                return true;
            } else {
                const error = await response.text();
                console.error('Ошибка обновления доли:', error);
                return false;
            }
        } catch (error) {
            console.error('Ошибка обновления доли:', error);
            return false;
        }
    }

    async GetApartmentShares(apartmentId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/${apartmentId}/shares`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error?.message || `Ошибка получения долей квартиры ${apartmentId}`);
            }
            const data = await response.json();
            console.log(`Доли квартиры ${apartmentId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения долей квартиры ${apartmentId}:`, error);
            throw error;
        }
    }

    async GetApartmentStatistics(apartmentId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/${apartmentId}/statistics`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error?.message || `Ошибка получения статистики квартиры ${apartmentId}`);
            }
            const data = await response.json();
            console.log(`Статистика квартиры ${apartmentId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения статистики квартиры ${apartmentId}:`, error);
            throw error;
        }
    }
}

// Обработчик события авторизации (остаётся без изменений, кроме уточнения обработки ошибок)
document.addEventListener('authStateChanged', async (event) => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();

    if (isAuthenticated && userData) {
        const apartmentProfile = new ApartmentProfile();
        const userId = window.authManager.userData.userId;

        if (window.location.pathname === '/') {
            await apartmentProfile.InsertApartmentDataToCardOnMainPage(userId);
        }

        if (window.location.pathname === '/apartment/create') {
            await apartmentProfile.SetHouseIdToCreateApartment();
            document.querySelector('[data-action="save-apartment-data"]').addEventListener('click', () => {
                apartmentProfile.CollectApartmentDataAndSaveToCreate();
            });
        }

        if (Regex.getUrlPathParts(window.location.href).includes('apartment') && Regex.getUrlPathParts(window.location.href).includes(String(userId))) {
            try {
                const apartments = await apartmentProfile.GetApartmentsByUserId(userId);
                if (!apartments || apartments.length === 0) {
                    document.querySelector('.apartments-by-user-list').innerHTML = '<p>Нет привязанных квартир</p>';
                    return;
                }

                const houseIds = [...new Set(apartments.map(a => a.houseId))];
                const housePromises = houseIds.map(id => apartmentProfile.House.GetHouse(id));
                const houses = await Promise.all(housePromises);
                const houseMap = new Map(houses.map(h => [h.id, h]));

                const sorted = apartments
                    .map(a => ({ apartment: a, house: houseMap.get(a.houseId) }))
                    .filter(item => item.house)
                    .sort((a, b) => {
                        const numA = parseInt(a.house.number) || 0;
                        const numB = parseInt(b.house.number) || 0;
                        return numA - numB;
                    });

                const container = document.querySelector('.apartments-by-user-list');
                container.innerHTML = '';
                for (const { apartment, house } of sorted) {
                    apartmentProfile.InsertApartmentProfileToAllApartments(apartment, house);
                }
            } catch (e) {
                console.error('Ошибка при отображении списка квартир:', e);
                document.querySelector('.apartments-by-user-list').innerHTML = '<p>Ошибка загрузки списка квартир</p>';
            }
        }

        if (Regex.isValidEntityUrl(window.location.href).valid && Regex.getUrlPathParts(window.location.href).includes('apartment')) {
            const apartmentId = Regex.isValidEntityUrl(window.location.href).id;
            apartmentProfile.EditApartmentProfile(apartmentId);
            apartmentProfile.AddNewUserToApartment(apartmentId);
            apartmentProfile.RemoveUserFromApartmentAndSave(apartmentId);
        }
    }
});