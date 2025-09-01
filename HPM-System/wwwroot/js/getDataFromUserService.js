class GetDataFromUserService {
    constructor () {
        this.userApiAddress = 'http://localhost:55680';
    }

    async getUserById(id) {
        try {
            const response = await fetch(`${this.userApiAddress}/api/Users/${id}`, {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
            });
            if (!response.ok) throw new Error(await response.text());
            const data = await response.json();
            console.log(`Пользователь ${id}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения пользователя ${id}:`, error);
        }
    }
}

export { GetDataFromUserService };