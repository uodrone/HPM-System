import { Modal } from './Modal.js';
import { HouseValidator } from './HouseValidator.js';

export class ApartmentHouses {
    constructor () {
        // ИЗМЕНЕНИЕ 1: Используем Gateway вместо прямого обращения к сервису
        this.gatewayUrl = 'http://localhost:55699';
    }

    async InsertHouseDataById(id) {
        try {
            const house = await this.GetHouse(id);
            console.log(`дом пользователя`);
            console.log(house);

            const headOfHouse = await this.GetHead(house.id);            
            const houseUsers = await this.GetHouseOwnersWithApartments(id);

            document.getElementById('city').value = house.city;
            document.getElementById('street').value = house.street;
            document.getElementById('number').value = house.number;            
            document.getElementById('postIndex').value = house.postIndex;
            document.getElementById('floors').value = house.floors;
            document.getElementById('entrances').value = house.entrances;
            document.getElementById('totalArea').value = house.totalArea;
            document.getElementById('apartmentsArea').value = house.apartmentsArea;
            document.getElementById('landArea').value = house.landArea;
            document.getElementById('isApartmentBuilding').checked = house.isApartmentBuilding;
            document.getElementById('hasGas').checked = house.hasGas;
            document.getElementById('hasElectricity').checked = house.hasElectricity;
            document.getElementById('hasElevator').checked = house.hasElevator;
            document.getElementById('builtYear').value = house.builtYear;

            houseUsers.forEach(user => {
                const option = document.createElement('option');
                option.value = user.userId;
                option.textContent = `${user.fullName}, кв. ${user.apartmentNumbers[0]} `;
                document.getElementById('houseHead').appendChild(option);
            });
            
            //сохранение данных профиля дома
            document.querySelector('[data-action="save-house-data"]').addEventListener('click', () => {
                console.log(`клик по кнопке сохранения дома`);
                this.CollectHouseDataAndUpdateProfile ();
            });    

        } catch (e) {
            console.error('Ошибка при загрузке данных дома:', e);
        }
    }

    async InsertApartmentsInHouseDetails(houseId) {
        const apartments = await this.GetApartmentsByHouseId(houseId);
        const apartmentsContainerList = document.querySelector('[data-group="AllApartmentsInHouse"] .apartments-list');

        apartments.forEach(apartment => {
            let apartmentTemplate = this.ApartmentToHouseTemplate(apartment);
            apartmentsContainerList.insertAdjacentHTML('beforeend', apartmentTemplate);
        });
    }

    //Вставить данные о домах пользователя в карточку
    async InsertHouseDataByUserId(userId, housesListClass, template) {
        try {
            const houses = await this.GetHousesByUserId(userId);
            console.log(`дома пользователя:`, houses);

            const housesListContainer = document.querySelector(housesListClass);
            housesListContainer.innerHTML = '';

            for (const house of houses) {
                const headOfHouse = await this.GetHead(house.id);
                const headTemplate = this.HeadTemplate(headOfHouse);
                const managementCompanyTemplate = this.ManagementCompanyTemplate();
                const houseTemplate = template(house, headTemplate, managementCompanyTemplate, headOfHouse);
                housesListContainer.insertAdjacentHTML('beforeend', houseTemplate);
            }
        } catch (error) {
            console.error('Ошибка получения данных домов:', error);
        }
    }

    ApartmentToHouseTemplate (apartment) {
        let apartmentHTML;

        if (apartment && typeof(apartment) == 'object') {
            let apartmentsOwnersCount = apartment.users.filter(user =>
                                            user.statuses.some(status => status.name === 'Владелец')
                                        ).length;

            apartmentHTML = `
            <div class="card card_apartment">
                <div class="area-value text-center">Квартира ${apartment.number}</div>
                <div>Количество комнат: ${apartment.numbersOfRooms}</div>
                <div>Общая площадь: ${apartment.totalArea}</div>
                <div>Количество владельцев: ${apartmentsOwnersCount}</div>
                <div class="text-center">
                    <a href="/apartment/${apartment.id}">К профилю квартиры</a>
                </div>
            </div>
            `;
        } else {
            apartmentHTML = `
            <div class="apartment-card-into-house">
                <div>В доме нет квартир</div>
            </div>
            `;
        }

        return apartmentHTML;
    }

    ManagementCompanyTemplate (company) {
        let companyHTML = `
            <div class="company-grid">
                <div class="company-item">
                    <div class="company-label">Название</div>
                    <div class="company-value empty">Не указано</div>
                </div>
                <div class="company-item">
                    <div class="company-label">Аварийно-диспетчерская служба</div>
                    <div class="company-value empty">Не указано</div>
                </div>
                <div class="company-item">
                    <div class="company-label">Режим работы</div>
                    <div class="company-value empty">Не указано</div>
                </div>
                <div class="company-item">
                    <div class="company-label">Приёмная</div>
                    <div class="company-value empty">Не указано</div>
                </div>
                <div class="company-item">
                    <div class="company-label">Адрес домоуправления</div>
                    <div class="company-value empty">Не указано</div>
                </div>
                <div class="company-item">
                    <div class="company-label">Сайт организации</div>
                    <div class="company-value empty">Не указано</div>
                </div>
            </div>
        `;
        return companyHTML;
    }

    HeadTemplate (head) {
        let headHTML;
        if (head && typeof(head) == 'object') {
            headHTML = `
                <div class="senior-info">
                    <div class="senior-name">${head.firstName} ${head.patronymic}</div>
                    <div class="senior-phone"><a href="tel:${head.phoneNumber}">${head.phoneNumber}</a></div>
                </div>
            `;
            return headHTML;
        }
        else {
            return `
                <div class="senior-info">
                    <div class="no-senior">Здесь нет старшего по дому</div>
                </div>
            `;
        }
    }

    MainPageHouseTemplate (house, headTemplate) {
        let houseHTML;
        if (house) {
            houseHTML = `
                <div class="house-item">
                    <div class="house-address" data-house-id="${house.id}">${house.city}, ${house.street}, ${house.number}</div>
                    <div class="senior-section">
                        <div class="senior-title">Старший по дому</div>
                        ${headTemplate}                        
                    </div>
                </div>
            `;            
        }

        return houseHTML
    }

    HousesListHouseTemplate (house, headTemplate, managementCompanyTemplate, headOfHouse) {
        let houseHTML;

        console.log(`дом:`);
        console.log(house);

        if (house) {
             houseHTML = `
                <div class="card card_house" data-house-id="${house.id}">
                    <h3 class="card-header card-header_house">${house.city}, улица ${house.street}, дом ${house.number}</h3>

                    <!-- Основные характеристики -->
                    <div class="basic-details">
                        <div class="detail-item">
                            <span class="detail-label">Тип дома</span>
                            <span class="detail-value">${house.isApartmentBuilding ? "многоквартирный" : "индивидуальный"}</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Этажей</span>
                            <span class="detail-value">${house.floors}</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Подъездов</span>
                            <span class="detail-value">${house.entrances}</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Газ</span>
                            <span class="detail-value ${house.hasGas ? 'utility-yes' : 'utility-no'}">${house.hasGas ? 'Есть' : 'Нет'}</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Электричество</span>
                            <span class="detail-value ${house.hasElectricity ? 'utility-yes' : 'utility-no'}">${house.hasElectricity ? 'Есть' : 'Нет'}</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Лифты</span>
                            <span class="detail-value ${house.hasElevator ? 'utility-yes' : 'utility-no'}">${house.hasElevator ? 'Есть' : 'Нет'}</span>
                        </div>                        
                        <div class="detail-item">
                            <span class="detail-label">Год постройки</span>
                            <span class="detail-value">${house.builtYear}</span>
                        </div>
                    </div>

                    <!-- Площади -->
                    <div class="areas-section">
                        <div class="card card_area mb-0">
                            <div class="area-value">${house.totalArea} м²</div>
                            <div class="area-label">Общая площадь</div>
                        </div>
                        <div class="card card_area mb-0">
                            <div class="area-value">${house.apartmentsArea} м²</div>
                            <div class="area-label">Жилая площадь</div>
                        </div>
                        <div class="card card_area mb-0">
                            <div class="area-value">${house.landArea} м²</div>
                            <div class="area-label">Площадь территории</div>
                        </div>
                    </div>

                    ${headOfHouse && headOfHouse.id === window.authManager.userData.userId 
                    ? `<div class="text-center">
                        <a href="/house/${house.id}">Редактировать дом</a>
                    </div>` 
                    : ''}

                    <!-- Старший по дому -->
                    <div class="senior-section">
                        <h6 class="section-title">Старший по дому</h6>
                        ${headTemplate}                        
                    </div>
                    

                    <!-- Управляющая компания -->
                    <div class="card card_management mb-0">
                        <h6 class="section-title">Управляющая компания</h6>
                        ${managementCompanyTemplate}
                    </div>
                </div>
            `;
        }        

        return houseHTML
    }

    async CollectHouseDataAndCreate () {
        let house = {};

        document.querySelectorAll('[data-group="house"] input').forEach(input => {
            const key = input.id;

            let value;
            if (input.type === 'checkbox') {
                value = input.checked;
            } else if (input.type === 'number') {
                // Пустое поле → 0
                value = input.value === '' ? 0 : Number(input.value);                
            } else if (input.tagName === 'SELECT') {
                value = input.value === '' ? null : el.value;
            } else {               
                value = input.value || null;
            }

            house[key] = value;
        });

        // Валидация
        const validation = HouseValidator.validate(house);
        if (!validation.isValid) {
            HouseValidator.displayErrors(validation.errors);
            Modal.ShowNotification('Исправьте ошибки в форме', 'red');
            return;
        }

        // Убираем ошибки перед попыткой отправки
        HouseValidator.displayErrors({});

        let isCreateHouseSuccessfull = await this.CreateHouse(house);

        if (isCreateHouseSuccessfull) {
            Modal.ShowNotification('Данные о доме успешно сохранены', 'green');
            console.log(`собранные данные о доме`);
            console.log(house);
        } else {
            Modal.ShowNotification('Ошибка сохранения данных', 'red');
        }
    }

    async CollectHouseDataAndCreate () {
        let house = {};

        document.querySelectorAll('[data-group="house"] input').forEach(input => {
            const key = input.id;

            let value;
            if (input.type === 'checkbox') {
                value = input.checked;
            } else if (input.type === 'number') {
                // Пустое поле → 0
                value = input.value === '' ? 0 : Number(input.value);                
            } else if (input.tagName === 'SELECT') {
                value = input.value === '' ? null : el.value;
            } else {               
                value = input.value || null;
            }

            house[key] = value;
        });

        // Валидация
        const validation = HouseValidator.validate(house);
        if (!validation.isValid) {
            HouseValidator.displayErrors(validation.errors);
            Modal.ShowNotification('Исправьте ошибки в форме', 'red');
            return;
        }

        // Убираем ошибки перед попыткой отправки
        HouseValidator.displayErrors({});

        let isCreateHouseSuccessfull = await this.CreateHouse(house);

        if (isCreateHouseSuccessfull) {
            Modal.ShowNotification('Данные о доме успешно сохранены', 'green');
            console.log(`собранные данные о доме`);
            console.log(house);
        } else {
            Modal.ShowNotification('Ошибка сохранения данных', 'red');
        }
    }

    async CollectHouseDataAndUpdateProfile () {
        let house = {};
        const Regex = new RegularExtension();
        const houseId = Regex.isValidEntityUrl(window.location.href).id;

        document.querySelectorAll('[data-group="house"] input').forEach(input => {
            const key = input.id;

            let value;
            if (input.type === 'checkbox') {
                value = input.checked;
            } else if (input.type === 'number') {
                // Пустое поле → 0
                value = input.value === '' ? 0 : Number(input.value);                
            } else if (input.tagName === 'SELECT') {
                value = input.value === '' ? null : el.value;
            } else {               
                value = input.value || null;
            }

            house[key] = value;
        });

        // Валидация
        const validation = HouseValidator.validate(house);
        if (!validation.isValid) {
            HouseValidator.displayErrors(validation.errors);
            Modal.ShowNotification('Исправьте ошибки в форме', 'red');
            return;
        }

        // Убираем ошибки перед попыткой отправки
        HouseValidator.displayErrors({});

        let headId = document.getElementById('houseHead').value;
        //Переназначаем старшего по дому
        let isAssignHeadSuccessfull = await this.AssignHead(houseId, headId);        
        //Обновляем данные о доме
        let isUpdateHouseSuccessfull = await this.UpdateHouse(houseId, house);

        if (isAssignHeadSuccessfull && isUpdateHouseSuccessfull) {
            Modal.ShowNotification('Данные о доме успешно сохранены', 'green');
            console.log(`собранные данные о доме`);
            console.log(house);
        } else {
            Modal.ShowNotification('Ошибка сохранения данных', 'red');
        }
    }

    // ========================================
    // API МЕТОДЫ - ИЗМЕНЕНИЕ 3: Используем window.apiCall для авторизации
    // ========================================

    // 1. Получить все дома
    async GetHouses() {
        try {
            // ИЗМЕНЕНИЕ: Используем window.apiCall вместо fetch
            const response = await window.apiCall(`${this.gatewayUrl}/api/house`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Ошибка получения домов');
            }
            
            const data = await response.json();
            console.log('Дома:', data);
            return data;
        } catch (error) {
            console.error('Ошибка получения списка домов:', error);
            throw error;
        }
    }

    // 2. Получить дом по ID
    async GetHouse(id) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/${id}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || `Ошибка получения дома ${id}`);
            }
            
            const data = await response.json();
            return data;
        } catch (error) {
            console.error(`Ошибка получения дома ${id}:`, error);
            throw error;
        }
    }

    // 3. Создать новый дом
    async CreateHouse(houseData) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(houseData)
            });
            
            if (response.ok) {
                const data = await response.json();
                console.log('Дом создан:', data);
                return true;
            } else {
                const error = await response.json();
                console.error('Ошибка создания дома:', error);
                return false;
            }
        } catch (error) {
            console.error('Ошибка создания дома:', error);
            return false;
        }
    }

    // 4. Обновить дом
    async UpdateHouse(id, houseData) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(houseData)
            });
            
            if (response.ok) {
                console.log(`Дом ${id} обновлен`);
                return true;
            } else {
                const error = await response.text();
                console.error('Ошибка обновления дома:', error);
                return false;
            }
        } catch (error) {
            console.error(`Ошибка обновления дома ${id}:`, error);
            return false;
        }
    }

    // 5. Удалить дом
    async DeleteHouse(id) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/${id}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (!response.ok) {
                const error = await response.text();
                throw new Error(error);
            }
            
            console.log(`Дом ${id} удален`);
            return true;
        } catch (error) {
            console.error(`Ошибка удаления дома ${id}:`, error);
            throw error;
        }
    }

    // 6. Назначить старшего по дому
    async AssignHead(houseId, userId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/${houseId}/head/${userId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (response.ok) {
                const data = await response.text();
                console.log(`Старший по дому назначен:`, data);
                return true;
            } else {
                const error = await response.text();
                console.error('Ошибка назначения старшего:', error);
                return false;
            }
        } catch (error) {
            console.error(`Ошибка назначения старшего по дому ${houseId}:`, error);
            return false;
        }
    }

    // 7. Отозвать старшего по дому
    async RevokeHead(houseId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/${houseId}/head`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (!response.ok) {
                const error = await response.text();
                throw new Error(error);
            }
            
            const data = await response.text();
            console.log(data);
            return true;
        } catch (error) {
            console.error(`Ошибка отзыва старшего по дому ${houseId}:`, error);
            throw error;
        }
    }

    // 8. Получить информацию о старшем по дому
    async GetHead(houseId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/${houseId}/head`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            // Читаем тело ОДИН раз как текст
            const text = await response.text();

            let data;
            let isJson = false;

            // Пытаемся распарсить как JSON
            try {
                data = JSON.parse(text);
                isJson = true;
            } catch (e) {
                // Это не JSON — значит, это просто строка
                data = { message: text };
            }

            if (!response.ok) {
                const errorMessage = data.message || data.Message || (isJson ? JSON.stringify(data) : text);
                console.error(`Ошибка ${response.status}:`, errorMessage);

                if (response.status === 404) {
                    console.log(errorMessage);
                    return null; // Старшего нет
                }
                throw new Error(errorMessage);
            }

            console.log(`Старший по дому ${houseId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения старшего по дому ${houseId}:`, error.message);
            return null;
        }
    }

    // Получить информацию о старшем по дому по id квартиры
    async GetHeadByApartmentId(apartmentId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/apartment/${apartmentId}/head`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            // Читаем тело ОДИН раз как текст
            const text = await response.text();

            let data;
            let isJson = false;

            // Пытаемся распарсить как JSON
            try {
                data = JSON.parse(text);
                isJson = true;
            } catch (e) {
                // Это не JSON — значит, это просто строка
                data = { message: text };
            }

            if (!response.ok) {
                const errorMessage = data.message || data.Message || (isJson ? JSON.stringify(data) : text);
                console.error(`Ошибка ${response.status}:`, errorMessage);

                if (response.status === 404) {
                    console.log(errorMessage);
                    return null; // Старшего нет
                }
                throw new Error(errorMessage);
            }

            console.log(`Старший по дому:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения старшего по дому:`, error.message);
            return null;
        }
    }

    // 9. Получить дома по ID пользователя
    async GetHousesByUserId(userId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || `Ошибка получения домов пользователя ${userId}`);
            }
            
            const data = await response.json();
            console.log(`Дома пользователя ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения домов для пользователя ${userId}:`, error);
            throw error;
        }
    }

    // 10. Получить все квартиры по ID дома
    async GetApartmentsByHouseId(houseId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/apartment/house/${houseId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error?.message || 'Ошибка при загрузке квартир');
            }
            
            const data = await response.json();
            console.log(`Квартиры в доме ${houseId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения квартир для дома ${houseId}:`, error);
            throw error;
        }
    }

    // 11. Получить владельцев квартир в доме с их номерами (массивами)
    async GetHouseOwnersWithApartments(houseId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/house/${houseId}/owners`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error?.message || `Ошибка при загрузке владельцев дома ${houseId}`);
            }
            
            const data = await response.json();
            console.log(`Владельцы с квартирами в доме ${houseId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения владельцев с квартирами для дома ${houseId}:`, error);
            throw error;
        }
    }
}

document.addEventListener('authStateChanged', async () => {    
    const Regex = new window.RegularExtension();
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const houseProfile = new ApartmentHouses();
        const userId = window.authManager.userData.userId;

        if (window.location.pathname == '/') {
            await houseProfile.InsertHouseDataByUserId(userId, '.houses-list', houseProfile.MainPageHouseTemplate);
        }

        if (window.location.pathname.includes(`/house/by-user/${userId}`)) {
            await houseProfile.InsertHouseDataByUserId(userId, '.houses-list', houseProfile.HousesListHouseTemplate);
        }

        if (Regex.isValidEntityUrl(window.location.href).valid && Regex.getUrlPathParts(window.location.href).includes('house')) {
            const houseId = Regex.isValidEntityUrl(window.location.href).id;
            localStorage.setItem('house', houseId);
            await houseProfile.InsertHouseDataById(houseId);
            await houseProfile.InsertApartmentsInHouseDetails(houseId);
        }

        //где-то тут и на бэке надо бы сделать проверку на суперадминистратора :)
        if (window.location.pathname.includes('/house/create')) {            
            document.querySelector('[data-action="save-house-data"]').addEventListener('click', () => {
                console.log(`клик по кнопке сохранения дома`);
                houseProfile.CollectHouseDataAndCreate ();
            }); 
        }
    }
});