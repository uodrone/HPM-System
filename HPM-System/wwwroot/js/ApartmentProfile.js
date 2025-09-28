﻿import {ApartmentStatuses} from './ApartmentStatuses.js';
import {ApartmentHouses} from './ApartmentHouses.js';

class ApartmentProfile {
    constructor () {
        this.ApartmentAPIAddress = 'https://localhost:55683';
        this.House = new ApartmentHouses();
    }

    //Вставить данные о квартирах пользователя в карточку на главной странице
    async InsertApartmentDataToCardOnMainPage(userId) {
    try {
        // Получаем квартиры пользователя
        const apartments = await this.GetApartmentsByUserId(userId);
        console.log('Квартиры пользователя:', apartments);

        const apartmentsListContainer = document.querySelector('.apartments-card .apartments-list');
        apartmentsListContainer.innerHTML = '';

        // Обрабатываем каждую квартиру
        for (const apartment of apartments) {
            // Получаем дом для текущей квартиры
            const house = await this.House.GetHouse(apartment.houseId);
            // Можно передать house в шаблон, если нужно
            let apartmentTemplate = this.SetApartmentTemplate(apartment, house);
            apartmentsListContainer.insertAdjacentHTML('beforeend', apartmentTemplate);
        }
    } catch (error) {
        console.error('Ошибка при загрузке данных квартиры на главную страницу:', error);
    }
}

    SetApartmentTemplate (apartment, house) {
        let apartmentHTML;        
        let apartmentNumber;
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
            if (!response.ok) throw new Error(data);
            console.log(data);
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
            if (!response.ok) throw new Error(data);
            console.log(data);
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
            if (!response.ok) throw new Error(data);
            console.log(data);
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

document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const apartmentProfile = new ApartmentProfile();
        const userId = window.authManager.userData.userId;

       if (window.location.pathname == '/') {
            apartmentProfile.InsertApartmentDataToCardOnMainPage(userId);
        }
    }
});