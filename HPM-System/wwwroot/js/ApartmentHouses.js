export class ApartmentHouses {
    constructor () {
        this.ApartmentAPIAddress = 'https://localhost:55683';
    }

    //Вставить данные о домах пользователя в карточку на главной странице
    async InsertHouseDataToCardOnMainPage (userId) {        
        try {
            await this.GetHousesByUserId(userId).then(houses => {
                console.log(`дома пользователя:`);
                console.log(houses);
                
                const housesListContainer = document.querySelector('#houses-card .houses-list');
                housesListContainer.innerHTML = '';
                houses.forEach(async (house) => {
                    let headOfHOuse = await this.GetHead(house.id);
                    let houseTemplate = await this.SetHouseTemplate(house, headOfHOuse);
                    housesListContainer.insertAdjacentHTML('beforeend', houseTemplate);
                });

            }).catch(error => {
                console.error('Ошибка получения данных квартиры:', error);
            });
        } catch (e) {
            console.log(e);
        }
    }

    SetHouseTemplate (house, head) {
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
        

        return houseHTML;
;
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
        const hoseProfile = new ApartmentHouses();
        const userId = window.authManager.userData.userId;

       if (window.location.pathname == '/') {
            hoseProfile.InsertHouseDataToCardOnMainPage(userId);
        }
    }
});