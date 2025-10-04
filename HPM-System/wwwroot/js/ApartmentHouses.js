export class ApartmentHouses {
    constructor () {
        this.ApartmentAPIAddress = 'https://localhost:55683';
    }

    //Вставить данные о домах пользователя в карточку на главной странице
    async InsertHouseData (userId, housesListClass, template) {        
        try {
            await this.GetHousesByUserId(userId).then(houses => {
                console.log(`дома пользователя:`);
                console.log(houses);
                
                const housesListContainer = document.querySelector(housesListClass);
                housesListContainer.innerHTML = '';
                houses.forEach(async (house) => {
                    let headOfHOuse = await this.GetHead(house.id);
                    let headTemplate = this.headTemplate(headOfHOuse);
                    let managementCompanyTemplate = this.managementCompanyTemplate();
                    let houseTemplate = template(house, headTemplate, managementCompanyTemplate);
                    housesListContainer.insertAdjacentHTML('beforeend', houseTemplate);
                });

            }).catch(error => {
                console.error('Ошибка получения данных квартиры:', error);
            });
        } catch (e) {
            console.log(e);
        }
    }

    managementCompanyTemplate (company) {
        let companyHTML;
        companyHTML = `
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

    headTemplate (head) {
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

    HousesListHouseTemplate (house, headTemplate, managementCompanyTemplate) {
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
            console.log(`Дом ${id}:`, data);
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
}

document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const houseProfile = new ApartmentHouses();
        const userId = window.authManager.userData.userId;

        if (window.location.pathname == '/') {
            houseProfile.InsertHouseData(userId, '.houses-list', houseProfile.MainPageHouseTemplate);
        }

        if (window.location.pathname.includes(`/house/by-user/${userId}`)) {
            houseProfile.InsertHouseData(userId, '.houses-list', houseProfile.HousesListHouseTemplate);
        }
    }
});