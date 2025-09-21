export class ApartmentHouses {
    constructor () {
        this.ApartmentAPIAddress = 'https://localhost:55683';
    }

    // 1. Получить все дома
    async GetHouses() {
        try {
            const response = await fetch(`${API_BASE}/api/House`, {
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
            const response = await fetch(`${API_BASE}/api/House/${id}`, {
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
            const response = await fetch(`${API_BASE}/api/House`, {
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
        const response = await fetch(`${API_BASE}/api/House/${id}`, {
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
            const response = await fetch(`${API_BASE}/api/House/${id}`, {
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
            const response = await fetch(`${API_BASE}/api/House/${houseId}/head/${userId}`, {
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
                const response = await fetch(`${API_BASE}/api/House/${houseId}/head`, {
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
                const response = await fetch(`${API_BASE}/api/House/${houseId}/head`, {
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
                const response = await fetch(`${API_BASE}/api/House/user/${userId}`, {
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