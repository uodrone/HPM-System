import {RegularExtension} from './Regex.js';

export class ApartmentHouses {
    constructor () {
        this.ApartmentAPIAddress = 'https://localhost:55683';
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
    async InsertHouseDataByUserId (userId, housesListClass, template) {
        try {
            await this.GetHousesByUserId(userId).then(houses => {
                console.log(`дома пользователя:`);
                console.log(houses);
                
                const housesListContainer = document.querySelector(housesListClass);
                housesListContainer.innerHTML = '';
                houses.forEach(async (house) => {
                    let headOfHOuse = await this.GetHead(house.id);
                    let headTemplate = this.HeadTemplate(headOfHOuse);
                    let managementCompanyTemplate = this.ManagementCompanyTemplate();
                    let houseTemplate = template(house, headTemplate, managementCompanyTemplate, headOfHOuse);
                    housesListContainer.insertAdjacentHTML('beforeend', houseTemplate);
                });
            }).catch(error => {
                console.error('Ошибка получения данных квартиры:', error);
            });
        } catch (e) {
            console.log(e);
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
                <div><b>Квартира ${apartment.number}</b></div>
                <div>Количество комнат ${apartment.numbersOfRooms}</div>
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

                    ${headOfHouse.id == window.authManager.userData.userId ? 
                        `<div class="text-center">
                            <a href="/house/${house.id}">Редактировать дом</a>
                        </div>` : ``}

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

    CollectHouseDataAndUpdateProfile () {
        let house = {};
        const Regex = new RegularExtension();
        const houseId = Regex.isValidHouseUrl(window.location.href).id;

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
        let headId = document.getElementById('houseHead').value;
        //Переназначаем старшего по дому
        this.AssignHead(houseId, headId);
        //Обновляем данные о доме
        this.UpdateHouse(houseId, house);

        console.log(`собранные данные о доме`);
        console.log(house);
    }

    // 1. Получить все дома
    async GetHouses() {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log('Дома:', data);
            return data;
        } catch (error) {
            console.error('Ошибка получения списка домов:', error);
        }
    }

    // 2. Получить дом по ID
    async GetHouse(id) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House/${id}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);            
            return data;
        } catch (error) {
            console.error(`Ошибка получения дома ${id}:`, error);
        }
    }

    // 3. Создать новый дом
    async CreateHouse(houseData) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(houseData)
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log('Дом создан:', data);
            return data;
        } catch (error) {
            console.error('Ошибка создания дома:', error);
        }
    }

    // 4. Обновить дом
    async UpdateHouse(id, houseData) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(houseData)
            });
            if (!response.ok) {
                const error = await response.text();
                throw new Error(error);
            }
            console.log(`Дом ${id} обновлен`);
        } catch (error) {
            console.error(`Ошибка обновления дома ${id}:`, error);
        }
    }

    // 5. Удалить дом
    async DeleteHouse(id) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House/${id}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            console.log(`Дом ${id} удален`);
        } catch (error) {
            console.error(`Ошибка удаления дома ${id}:`, error);
        }
    }

    // 6. Назначить старшего по дому
    async AssignHead(houseId, userId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House/${houseId}/head/${userId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.text();
            if (!response.ok) throw new Error(data);
            console.log(`старший по дому назначен:`);
            console.log(data);
        } catch (error) {
            console.error(`Ошибка назначения старшего по дому ${houseId}:`, error);
        }
    }

    // 7. Отозвать старшего по дому
    async RevokeHead(houseId) {
        try {
                const response = await fetch(`${this.ApartmentAPIAddress}/api/House/${houseId}/head`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.text();
            if (!response.ok) throw new Error(data);
            console.log(data);
        } catch (error) {
            console.error(`Ошибка отзыва старшего по дому ${houseId}:`, error);
        }
    }

    // 8. Получить информацию о старшем по дому
    async GetHead(houseId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House/${houseId}/head`, {
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
                // Это не JSON — значит, это просто строка (например, из return NotFound("сообщение"))
                data = { message: text };
            }

            if (!response.ok) {
                const errorMessage = data.message || data.Message || (isJson ? JSON.stringify(data) : text);
                console.error(`Ошибка ${response.status}:`, errorMessage);

                if (response.status === 404) {
                    console.log(errorMessage);
                    data = errorMessage;
                }
            }

            // На случай, если успешный ответ тоже пришёл как plain text (маловероятно)
            if (!isJson) {                
                console.log(`Старший по дому отсутствует: ${data}`);
            }

            console.log(`Старший по дому ${houseId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения старшего по дому ${houseId}:`, error.message);
            throw error;
        }
    }

    // 9. Получить дома по ID пользователя
    async GetHousesByUserId(userId) {
        try {
                const response = await fetch(`${this.ApartmentAPIAddress}/api/House/user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log(`Дома пользователя ${userId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения домов для пользователя ${userId}:`, error);
        }
    }

    // 10. Получить все квартиры по ID дома
    async GetApartmentsByHouseId(houseId) {
        try {
            const response = await fetch(`${this.ApartmentAPIAddress}/api/Apartment/house/${houseId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data?.message || 'Ошибка при загрузке квартир');
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
            const response = await fetch(`${this.ApartmentAPIAddress}/api/House/${houseId}/owners`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            if (!response.ok) {
                throw new Error(data?.message || `Ошибка при загрузке владельцев дома ${houseId}`);
            }
            console.log(`Владельцы с квартирами в доме ${houseId}:`, data);
            return data; // [{ userId, fullName, phoneNumber, apartmentNumbers: [12, 15] }, ...]
        } catch (error) {
            console.error(`Ошибка получения владельцев с квартирами для дома ${houseId}:`, error);
            throw error;
        }
    }
}

document.addEventListener('authStateChanged', async () => {    
    const Regex = new RegularExtension();
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

        if (Regex.isValidHouseUrl(window.location.href).valid) {
            const houseId = Regex.isValidHouseUrl(window.location.href).id;
            await houseProfile.InsertHouseDataById(houseId);
            await houseProfile.InsertApartmentsInHouseDetails(houseId);
        }
    }
});