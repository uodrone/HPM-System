import {ApartmentProfile} from './ApartmentProfile.js';
import { Modal } from './Modal.js';

export class ApartmentStatuses {
    constructor () {
        this.apartmentAPIAddress = 'https://localhost:55683';
        this.apartmentProfile = new ApartmentProfile();
    }

    // 1. Получить все статусы
    async GetStatuses() {
        try {
            const response = await fetch(`${this.apartmentAPIAddress}/api/Status`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка ${response.status}: ${errorText}`);
            }

            const data = await response.json();
            console.log('Статусы:', data);
            return data;
        } catch (error) {
            console.error('Ошибка получения статусов:', error.message || error);
        }
    }

    // 2. Получить статус по ID
    async GetStatus(id) {
        try {
            const response = await fetch(`${this.apartmentAPIAddress}/api/Status/${id}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    console.log(`Статус с ID ${id} не найден.`);
                    return null;
                }
                const errorText = await response.text();
                throw new Error(`Ошибка ${response.status}: ${errorText}`);
            }

            const data = await response.json();
            console.log(`Статус ${id}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения статуса ${id}:`, error.message || error);
        }
    }

    // 3. Создать новый статус
    async CreateStatus(name) {
        try {
            const response = await fetch(`${this.apartmentAPIAddress}/api/Status`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: name })
            });

            if (!response.ok) {
                let errorMessage;
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.includes("application/json")) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    const errorText = await response.text();
                    errorMessage = `Ошибка ${response.status}: ${errorText}`;
                }
                throw new Error(errorMessage);
            }

            const data = await response.json();
            console.log('Статус создан:', data);
            return data;
        } catch (error) {
            console.error('Ошибка создания статуса:', error.message || error);
        }
    }

    // 4. Обновить статус
    async UpdateStatus(id, newName) {
    try {
        const response = await fetch(`${this.apartmentAPIAddress}/api/Status/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: newName }) // Предполагается, что DTO UpdateStatusDto имеет поле name
        });

        if (!response.ok) {
            if (response.status === 404) {
                console.log(`Статус с ID ${id} не найден для обновления.`);
                return false;
            }
            let errorMessage;
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.includes("application/json")) {
                const errorData = await response.json();
                errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
            } else {
                const errorText = await response.text();
                errorMessage = `Ошибка ${response.status}: ${errorText}`;
            }
            throw new Error(errorMessage);
        }

        console.log(`Статус ${id} обновлён.`);
        return true;
    } catch (error) {
        console.error(`Ошибка обновления статуса ${id}:`, error.message || error);
        return false; // Возвращаем false в случае ошибки
    }
    }

    // 5. Удалить статус
    async DeleteStatus(id) {
    try {
        const response = await fetch(`${this.apartmentAPIAddress}/api/Status/${id}`, {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' }
        });

        if (!response.ok) {
            if (response.status === 404) {
                console.log(`Статус с ID ${id} не найден для удаления.`);
                return false;
            }
            // Проверим, может быть ошибка 409 Conflict (если статус используется)
            if (response.status === 409) {
                const errorText = await response.text();
                console.log(`Конфликт при удалении статуса ${id}: ${errorText}`);
                return false;
            }
            const errorText = await response.text();
            throw new Error(`Ошибка ${response.status}: ${errorText}`);
        }

        console.log(`Статус ${id} удалён.`);
        return true;
    } catch (error) {
        console.error(`Ошибка удаления статуса ${id}:`, error.message || error);
        return false; // Возвращаем false в случае ошибки
    }
    }

    // 6. Назначить статус пользователю для квартиры
    async AssignStatusToUser(apartmentId, userId, statusId) {
        try {
            const response = await fetch(`${this.apartmentAPIAddress}/api/Status/apartment/${apartmentId}/user/${userId}/status/${statusId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
            // Тело запроса не требуется для этого эндпоинта
            });

            if (!response.ok) {
            let errorMessage;
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.includes("application/json")) {
                const errorData = await response.json();
                errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
            } else {
                const errorText = await response.text();
                errorMessage = `Ошибка ${response.status}: ${errorText}`;
            }
            throw new Error(errorMessage);
            }

            const successMessage = await response.text(); // Ожидаем текстовое сообщение
            console.log(successMessage);
            return true;
        } catch (error) {
            console.error('Ошибка назначения статуса пользователю:', error.message || error);
            return false; // Возвращаем false в случае ошибки
        }
    }

    // 7. Отозвать статус у пользователя для квартиры
    async RevokeStatusFromUser(apartmentId, userId, statusId) {
        try {
            const response = await fetch(`${this.apartmentAPIAddress}/api/Status/apartment/${apartmentId}/user/${userId}/status/${statusId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
                // Тело запроса не требуется для этого эндпоинта
            });

            if (!response.ok) {
                if (response.status === 404) {
                    console.log(`Связь статуса ${statusId} с пользователем ${userId} для квартиры ${apartmentId} не найдена.`);
                    return false;
                }
                let errorMessage;
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.includes("application/json")) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    const errorText = await response.text();
                    errorMessage = `Ошибка ${response.status}: ${errorText}`;
                }
                throw new Error(errorMessage);
            }

            const successMessage = await response.text(); // Ожидаем текстовое сообщение
            console.log(successMessage);
            return true;
        } catch (error) {
            console.error('Ошибка отзыва статуса у пользователя:', error.message || error);
            return false; // Возвращаем false в случае ошибки
        }
    }

    // 8. Установить полный набор статусов пользователя для квартиры (заменяет все текущие)
    async SetUserStatusesForApartment(apartmentId, userId, statusIds) {        
        if (!Array.isArray(statusIds)) {
            console.error("statusIds должен быть массивом (может быть пустым)");
            return false;
        }

        try {
            const response = await fetch(`${this.apartmentAPIAddress}/api/Status/apartment/${apartmentId}/user/${userId}/statuses`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ statusIds })
            });

            if (!response.ok) {
                let errorMessage;
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.includes("application/json")) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    const errorText = await response.text();
                    errorMessage = `Ошибка ${response.status}: ${errorText}`;
                }
                throw new Error(errorMessage);
            }

            const successMessage = await response.text();
            console.log(successMessage);
            return true;
        } catch (error) {
            console.error('Ошибка установки полного набора статусов:', error.message || error);
            return false;
        }
    }

    // 9. Получить все статусы пользователя для квартиры
    async GetUserStatusesForApartment(apartmentId, userId) {
        try {
            const response = await fetch(`${this.apartmentAPIAddress}/api/Status/apartment/${apartmentId}/user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    console.log(`Пользователь ${userId} не связан с квартирой ${apartmentId} или связь не найдена.`);
                    return [];
                }
                const errorText = await response.text();
                throw new Error(`Ошибка ${response.status}: ${errorText}`);
            }

            const data = await response.json();
            console.log(`Статусы пользователя ${userId} для квартиры ${apartmentId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения статусов пользователя ${userId} для квартиры ${apartmentId}:`, error.message || error);
            return []; // Возвращаем пустой массив в случае ошибки
        }
    }

    CollectUserStatusesAndSave (apartmentId) {
        document.addEventListener('click', async (e) => {
            // Проверяем, был ли клик по элементу с нужным data-атрибутом
            if (e.target.closest('[data-status="save"]')) {
                const user = e.target.closest('[data-apartment-user-id]');
                const userId = user.dataset.apartmentUserId;
                let statuses = [];
                user.querySelectorAll('[data-ts-item]').forEach(status => {
                    statuses.push(status.dataset.value);
                });

                let share = user.querySelector('[name="share"]').value != '' ? user.querySelector('[name="share"]').value : 0;
                
                try {
                    await this.SetUserStatusesForApartment(apartmentId, userId, statuses);
                    let isShareUpdadeSuccessfull = await this.apartmentProfile.UpdateUserShare(apartmentId, userId, share);
                    if (isShareUpdadeSuccessfull)
                    {
                        user.querySelector('[data-error="share"]').classList.add('invisible');
                        Modal.ShowNotification('Данные о пользователях квартиры сохранены', 'green');         
                    }
                    else
                    {
                        share = 0;
                        user.querySelector('[data-error="share"]').classList.remove('invisible');
                    }
                }
                catch (e) {
                    console.log(e);
                    user.querySelector('[data-error="share"]').classList.remove('invisible');
                    Modal.ShowNotification('Ошибка сохранения данных', 'red');
                }                
            }
        });
    }
}

document.addEventListener('authStateChanged', async () => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();
    const ApartmentUserStatuses = new ApartmentStatuses();

    if (isAuthenticated && userData) {
        const userId = window.authManager.userData.userId;

        if (Regex.isValidEntityUrl(window.location.href).valid && Regex.getUrlPathParts(window.location.href).includes('apartment')) {
            const apartmentId = Regex.isValidEntityUrl(window.location.href).id;
            ApartmentUserStatuses.CollectUserStatusesAndSave(apartmentId);
        }
    }
});