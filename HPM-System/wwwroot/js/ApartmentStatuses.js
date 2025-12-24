import { ApartmentProfile } from './ApartmentProfile.js';
import { Modal } from './Modal.js';

export class ApartmentStatuses {
    constructor() {
        // ИЗМЕНЕНИЕ: используем Gateway вместо прямого адреса микросервиса
        this.gatewayUrl = 'http://localhost:55699';
        this.apartmentProfile = new ApartmentProfile();
    }

    // 1. Получить все статусы
    async GetStatuses() {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status`, {
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
            return null;
        }
    }

    // 2. Получить статус по ID
    async GetStatus(id) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status/${id}`, {
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
            return null;
        }
    }

    // 3. Создать новый статус
    async CreateStatus(name) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name })
            });

            if (!response.ok) {
                const contentType = response.headers.get('content-type');
                let errorMessage;
                if (contentType?.includes('application/json')) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    errorMessage = `Ошибка ${response.status}: ${await response.text()}`;
                }
                throw new Error(errorMessage);
            }

            const data = await response.json();
            console.log('Статус создан:', data);
            return data;
        } catch (error) {
            console.error('Ошибка создания статуса:', error.message || error);
            return null;
        }
    }

    // 4. Обновить статус
    async UpdateStatus(id, newName) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: newName })
            });

            if (!response.ok) {
                if (response.status === 404) {
                    console.log(`Статус с ID ${id} не найден для обновления.`);
                    return false;
                }
                const contentType = response.headers.get('content-type');
                let errorMessage;
                if (contentType?.includes('application/json')) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    errorMessage = `Ошибка ${response.status}: ${await response.text()}`;
                }
                throw new Error(errorMessage);
            }

            console.log(`Статус ${id} обновлён.`);
            return true;
        } catch (error) {
            console.error(`Ошибка обновления статуса ${id}:`, error.message || error);
            return false;
        }
    }

    // 5. Удалить статус
    async DeleteStatus(id) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status/${id}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    console.log(`Статус с ID ${id} не найден для удаления.`);
                    return false;
                }
                if (response.status === 409) {
                    const conflictMsg = await response.text();
                    console.log(`Конфликт при удалении статуса ${id}: ${conflictMsg}`);
                    return false;
                }
                throw new Error(`Ошибка ${response.status}: ${await response.text()}`);
            }

            console.log(`Статус ${id} удалён.`);
            return true;
        } catch (error) {
            console.error(`Ошибка удаления статуса ${id}:`, error.message || error);
            return false;
        }
    }

    // 6. Назначить статус пользователю для квартиры
    async AssignStatusToUser(apartmentId, userId, statusId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status/apartment/${apartmentId}/user/${userId}/status/${statusId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const contentType = response.headers.get('content-type');
                let errorMessage;
                if (contentType?.includes('application/json')) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    errorMessage = `Ошибка ${response.status}: ${await response.text()}`;
                }
                throw new Error(errorMessage);
            }

            const successMessage = await response.text();
            console.log(successMessage);
            return true;
        } catch (error) {
            console.error('Ошибка назначения статуса пользователю:', error.message || error);
            return false;
        }
    }

    // 7. Отозвать статус у пользователя для квартиры
    async RevokeStatusFromUser(apartmentId, userId, statusId) {
        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status/apartment/${apartmentId}/user/${userId}/status/${statusId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    console.log(`Связь статуса ${statusId} с пользователем ${userId} для квартиры ${apartmentId} не найдена.`);
                    return false;
                }
                const contentType = response.headers.get('content-type');
                let errorMessage;
                if (contentType?.includes('application/json')) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    errorMessage = `Ошибка ${response.status}: ${await response.text()}`;
                }
                throw new Error(errorMessage);
            }

            const successMessage = await response.text();
            console.log(successMessage);
            return true;
        } catch (error) {
            console.error('Ошибка отзыва статуса у пользователя:', error.message || error);
            return false;
        }
    }

    // 8. Установить полный набор статусов пользователя для квартиры
    async SetUserStatusesForApartment(apartmentId, userId, statusIds) {
        if (!Array.isArray(statusIds)) {
            console.error('statusIds должен быть массивом');
            return false;
        }

        try {
            const response = await window.apiCall(`${this.gatewayUrl}/api/status/apartment/${apartmentId}/user/${userId}/statuses`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ statusIds })
            });

            if (!response.ok) {
                const contentType = response.headers.get('content-type');
                let errorMessage;
                if (contentType?.includes('application/json')) {
                    const errorData = await response.json();
                    errorMessage = `Ошибка ${response.status}: ${JSON.stringify(errorData)}`;
                } else {
                    errorMessage = `Ошибка ${response.status}: ${await response.text()}`;
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
            const response = await window.apiCall(`${this.gatewayUrl}/api/status/apartment/${apartmentId}/user/${userId}`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    console.log(`Пользователь ${userId} не связан с квартирой ${apartmentId}.`);
                    return [];
                }
                throw new Error(`Ошибка ${response.status}: ${await response.text()}`);
            }

            const data = await response.json();
            console.log(`Статусы пользователя ${userId} для квартиры ${apartmentId}:`, data);
            return data;
        } catch (error) {
            console.error(`Ошибка получения статусов пользователя ${userId} для квартиры ${apartmentId}:`, error.message || error);
            return [];
        }
    }

    // Обработчик сохранения статусов и доли
    CollectUserStatusesAndSave(apartmentId) {
        document.addEventListener('click', async (e) => {
            if (e.target.closest('[data-status="save"]')) {
                const user = e.target.closest('[data-apartment-user-id]');
                const userId = user.dataset.apartmentUserId;

                // Собираем статусы из элементов с data-ts-item (предполагается, что они управляются multiselect)
                const statuses = [];
                user.querySelectorAll('[data-ts-item]').forEach(status => {
                    statuses.push(status.dataset.value);
                });

                const shareInput = user.querySelector('[name="share"]');
                const share = shareInput?.value !== '' ? parseFloat(shareInput.value) : 0;

                try {
                    const statusesSaved = await this.SetUserStatusesForApartment(apartmentId, userId, statuses);
                    const shareUpdated = await this.apartmentProfile.UpdateUserShare(apartmentId, userId, share);

                    const shareErrorEl = user.querySelector('[data-error="share"]');
                    if (statusesSaved && shareUpdated) {
                        if (shareErrorEl) shareErrorEl.classList.add('invisible');
                        Modal.ShowNotification('Данные о пользователях квартиры сохранены', 'success');
                    } else {
                        if (shareErrorEl) shareErrorEl.classList.remove('invisible');
                        Modal.ShowNotification('Ошибка сохранения доли или статусов', 'error');
                    }
                } catch (error) {
                    console.error('Ошибка при сохранении статусов и доли:', error);
                    const shareErrorEl = user.querySelector('[data-error="share"]');
                    if (shareErrorEl) shareErrorEl.classList.remove('invisible');
                    Modal.ShowNotification('Ошибка сохранения данных', 'error');
                }
            }
        });
    }
}

// Обработчик авторизации
document.addEventListener('authStateChanged', (event) => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();

    if (isAuthenticated && userData) {
        const apartmentUserStatuses = new ApartmentStatuses();
        const apartmentIdMatch = Regex.isValidEntityUrl(window.location.href);
        if (apartmentIdMatch.valid && Regex.getUrlPathParts(window.location.href).includes('apartment')) {
            const apartmentId = apartmentIdMatch.id;
            apartmentUserStatuses.CollectUserStatusesAndSave(apartmentId);
        }
    }
});