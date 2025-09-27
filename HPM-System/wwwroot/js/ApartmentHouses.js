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
                    let houseTemplate = template(house, headOfHOuse);
                    housesListContainer.insertAdjacentHTML('beforeend', houseTemplate);
                });

            }).catch(error => {
                console.error('Ошибка получения данных квартиры:', error);
            });
        } catch (e) {
            console.log(e);
        }
    }

    MainPageHouseTemplate (house, head) {
        let houseHTML;
        if (house) {
            houseHTML = `
                <div class="house" data-house-id="${house.id}">
                    <div class="form-group">
                        <input disabled="" type="text" placeholder="" name="address" id="address-${house.id}" value="${house.city}, ${house.street}, ${house.number}">
                        <label for="address-${house.id}">Адрес дома</label>
                        <div class="error invisible" data-error="address">Неверный адрес</div>
                    </div>
                    <div class="form-group">
                        <input disabled="" type="text" placeholder="" name="headOfHouse" id="headOfHouse-${house.id}" value="${head.firstName} ${head.patronymic}, ${head.phoneNumber}">
                        <label for="headOfHouse-${house.id}">Старший по дому</label>
                        <div class="error invisible" data-error="headOfHouse">Старший по дому</div>
                    </div>
                </div>
            `;
        }        

        return houseHTML
    }

    HousesListHouseTemplate (house, head) {
        let houseHTML;
        if (house) {
            houseHTML = `
                <div class="profile-card profile-card_house" data-house-id="${house.id}">
                    <h3 class="text-center">${house.city}, улица ${house.street}, дом ${house.number}</h3>
                    <div class="d-flex flex-wrap gap-3 py-3 justify-content-between">
                        <table>                            
                            <tr>
                                <td class="p-2 fw-bold">Тип дома:</td>
                                <td class="p-2">${house.isApartmentBuilding ? "многоквартирный" : "индивидуальный"}</td>
                            </tr>
                            <tr>
                                <td class="p-2 fw-bold">Этажей:</td>
                                <td class="p-2">${house.floors}</td>
                            </tr>
                            <tr>
                                <td class="p-2 fw-bold">Подъездов:</td>
                                <td class="p-2">${house.entrances}</td>
                            </tr>
                            <tr>
                                <td class="p-2 fw-bold">Газ:</td>
                                <td class="p-2">${house.hasGas ? "есть" : "нет"}</td>
                            </tr>
                            <tr>
                                <td class="p-2 fw-bold">Электричество:</td>
                                <td class="p-2">${house.hasElectricity ? "есть" : "нет"}</td>
                            </tr>
                        </table>
                        <table>
                            <tr>
                                <td class="p-2 fw-bold">Год постройки:</td>
                                <td class="p-2">${house.builtYear}</td>
                            </tr> 
                            <tr>
                                <td class="p-2 fw-bold">Общая площадь:</td>
                                <td class="p-2">${house.totalArea} м<sup>2</sup></td>
                            </tr>
                            <tr>
                                <td class="p-2 fw-bold">Жилая площадь:</td>
                                <td class="p-2">${house.apartmentsArea} м<sup>2</sup></td>
                            </tr>
                            <tr>
                                <td class="p-2 fw-bold">Площадь территории:</td>
                                <td class="p-2">${house.landArea} м<sup>2</sup></td>
                            </tr>                                                      
                        </table>
                    </div>

                    <div class="py-3">
                        <h5 class="text-center">Старший по дому</h5>
                        <div>${head.firstName} ${head.patronymic}, <a href="tel:${head.phoneNumber}">${head.phoneNumber}</a></div>
                    </div>

                    <div class="py-3">
                        <h5 class="text-center">Управляющая компания</h5>
                         <div class="d-flex flex-wrap gap-3 justify-content-between">
                            <div>                            
                                <div class="py-3">
                                    <div class="fw-bold">Название</div>
                                    <div></div>
                                </div>                               
                                <div class="py-3">
                                    <div class="fw-bold">Режим работы</div>
                                    <div></div>
                                </div>
                                <div class="py-3">
                                    <div class="fw-bold">Адрес домоуправления</div>
                                    <div></div>
                                </div>                                
                            </div>
                            <div>                                
                               <div class="py-3">
                                    <div class="fw-bold">Аварийно-диспетчерская служба</div>
                                    <div></div>
                                </div>
                                <div class="py-3">
                                    <div class="fw-bold">Приёмная</div>
                                    <div></div>
                                </div>                          
                                <div class="py-3">
                                    <div class="fw-bold">Сайт организации</div>
                                    <div></div>
                                </div>                                                      
                            </div>
                        </div>
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
            const data = await response.json();
            if (!response.ok) throw new Error(data);
            console.log(`Старший по дому ${houseId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения старшего по дому ${houseId}:`, error);
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