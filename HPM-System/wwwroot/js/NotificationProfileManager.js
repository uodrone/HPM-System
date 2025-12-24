import { Modal } from './Modal.js';
import { ApartmentHouses } from './ApartmentHouses.js';
import { NotificationClient } from './NotificationClient.js';
import { FileStorageClient } from './FileStorageClient.js';
import { DateFormat } from './DateFormat.js';

export class NotificationProfileManager {
    constructor() {
        this.currentApartments = []; // Храним текущие квартиры
        this.houseProfile = new ApartmentHouses();
        this.userId = null;
    }

    async InsertDataToCreateNotification() {
        this.userId = window.authManager.userData.userId;
        const houseSelector = document.getElementById('houseId');
        const apartmentSelector = document.getElementById('apartments');
        const notificationGroup = document.querySelector('.profile-group[data-group="notification"]');

        // Очистим селектор перед заполнением
        houseSelector.innerHTML = '';

        const houses = await this.houseProfile.GetHousesByUserId(this.userId);

        const houseChecks = await Promise.all(
            // Массив промисов для параллельного выполнения
            houses.map(async (house) => {
                const houseHead = await this.houseProfile.GetHead(house.id);
                if (houseHead == null) return null;
                return { house, isHead: houseHead.id === this.userId };
            })
        );

        // Фильтруем только ненулевые значения, т.е. там где есть старший по дому
        const results = houseChecks.filter(item => item !== null);

        // Фильтруем дома, где пользователь — глава
        const eligibleHouses = results
            .filter(({ isHead }) => isHead)
            .map(({ house }) => house);

        if (eligibleHouses.length) {
            // Пользователь — глава хотя бы в одном доме => показываем селект
            eligibleHouses.forEach(house => {
                const option = document.createElement('option');
                option.value = house.id;
                option.textContent = `${house.city}, ул. ${house.street}, ${house.number}`;
                houseSelector.appendChild(option);
            });

            await this.LoadApartmentsForSelect(houseSelector.value);
        } else {
            // Ни в одном доме не глава => уведомление недоступно
            if (notificationGroup) {
                notificationGroup.innerHTML = `Создание уведомления недоступно`;
            }
        }

        houseSelector.addEventListener('change', async () => {
            await this.LoadApartmentsForSelect(houseSelector.value);
        });
    }

    async LoadApartmentsForSelect(houseId) {
        const apartmentSelector = document.getElementById('apartments');
        
        // Получаем квартиры и сохраняем их
        this.currentApartments = await this.houseProfile.GetApartmentsByHouseId(houseId);        

        // Очищаем селектор перед заполнением
        apartmentSelector.innerHTML = '';

        // Добавляем опцию "Очистить все"
        const clearOption = document.createElement('option');
        clearOption.value = 'clear';
        clearOption.textContent = 'Очистить все выбранные квартиры';
        apartmentSelector.appendChild(clearOption);

        // Добавляем опцию "Все квартиры дома"
        const allOption = document.createElement('option');
        allOption.value = 'all';
        allOption.textContent = 'Все квартиры дома';
        apartmentSelector.appendChild(allOption);

        // Добавляем квартиры
        for (const apartment of this.currentApartments) {
            const option = document.createElement('option');
            option.value = apartment.id;
            option.textContent = `квартира ${apartment.number}`;
            apartmentSelector.appendChild(option);
        }

        // Уничтожаем предыдущий экземпляр Tom Select, если он существует
        if (apartmentSelector.tomselect) {
            apartmentSelector.tomselect.destroy();
        }

        // Инициализируем Tom Select
        const tomSelectInstance = new TomSelect('#apartments', {
            plugins: ['remove_button'],
            maxItems: null,
            onChange: (values) => {
                // Если выбрана опция "Очистить все"
                if (values.includes('clear')) {
                    tomSelectInstance.clear();
                    return;
                }

                // Если выбрана опция "Все квартиры"
                if (values.includes('all')) {
                    const allApartmentIds = this.currentApartments.map(apt => apt.id.toString());
                    
                    if (values[values.length - 1] === 'all') {
                        tomSelectInstance.setValue(['all', ...allApartmentIds], true);
                    }
                } else {
                    const allApartmentIds = this.currentApartments.map(apt => apt.id.toString());
                    const hasAll = allApartmentIds.every(id => values.includes(id));
                    
                    if (!hasAll && values.includes('all')) {
                        tomSelectInstance.removeItem('all', true);
                    }
                }
            }
        });

        // Сохраняем экземпляр
        apartmentSelector.tomselect = tomSelectInstance;

        // Программно выбираем все квартиры при инициализации
        setTimeout(() => {
            const allApartmentIds = this.currentApartments.map(apt => apt.id.toString());
            tomSelectInstance.setValue(['all', ...allApartmentIds]);
        }, 0);
    }

    async CollectNotificationDataToCreate() {
        const apartmentSelector = document.getElementById('apartments');
        const tomSelectInstance = apartmentSelector.tomselect;

        if (!tomSelectInstance) {
            console.error('Tom Select не инициализирован');
            return { apartmentIds: [], userIds: [] };
        }

        // Получаем массив выбранных значений
        const selectedValues = tomSelectInstance.getValue();

        // Фильтруем 'all' и 'clear', получаем только ID квартир
        const apartmentIds = selectedValues.filter(value => value !== 'all' && value !== 'clear');
        console.log('Выбранные квартиры:', apartmentIds);

        // Проверяем, какой radio button выбран
        const toOwnersRadio = document.getElementById('toOwners');
        const isOwnersOnly = toOwnersRadio ? toOwnersRadio.checked : false;
        
        console.log('Radio toOwners checked:', isOwnersOnly);

        // Собираем пользователей из выбранных квартир
        const allUserIds = [];

        apartmentIds.forEach(apartmentId => {
            // Находим квартиру по ID из сохранённых данных
            const apartment = this.currentApartments.find(apt => apt.id.toString() === apartmentId);

            if (apartment && apartment.users && Array.isArray(apartment.users)) {
                console.log(`Квартира ${apartment.number}, пользователей:`, apartment.users.length);
                
                apartment.users.forEach(user => {
                    console.log(`Пользователь ${user.userId}, статусы:`, user.statuses);
                    
                    if (isOwnersOnly) {
                        // Фильтруем только владельцев
                        const isOwner = user.statuses && user.statuses.some(status => status.name === 'Владелец');
                        console.log(`  Является владельцем: ${isOwner}`);
                        if (isOwner) {
                            allUserIds.push(user.userId);
                        }
                    } else {
                        // Все пользователи
                        console.log(`  Добавляем всех пользователей`);
                        allUserIds.push(user.userId);
                    }
                });
            }
        });

        // Получаем уникальных пользователей
        const uniqueUserIds = [...new Set(allUserIds)];

        console.log('Все пользователи:', allUserIds);
        console.log('Уникальные пользователи:', uniqueUserIds);
        console.log('Только собственники:', isOwnersOnly);

        //собираем картинку
        const fileInput = document.getElementById('fileInput');
        const file = fileInput.files[0];

        const fileManager = new FileStorageClient();
        let isFileUpload = await fileManager.UploadFile(file);
        console.log(`результат загрузки файла картинки`);
        console.log(isFileUpload);

        const title = document.getElementById('title').value;
        const message = document.getElementById('message').value;

        return {
            title: title,
            message: message,
            type: 0,
            createdBy: window.authManager.userData.userId,
            imageUrl: isFileUpload.fileUrl,
            userIdList: uniqueUserIds
        };
    }

    InsertDataToMainPage (data) {
        const notificationsContainer = document.querySelector('.notificatiions-list');        
        if (data.length) {
            data.forEach((notification) => {
                const notificationMainPageTemplate = this.NotificationMainPageTemplate(notification);
                notificationsContainer.insertAdjacentHTML('beforeend', notificationMainPageTemplate);
            });
        } else {
            notificationsContainer.innerHTML = `Нет новых уведомлений`;
        }
    }

    NotificationMainPageTemplate (notification) {
        let notificationHTML;
        if (notification) {
            notificationHTML = `
                <a class="card-item card-item_notification" href="/notification/${notification.id}">
                    <div class="font-size-12 color-gray">${DateFormat.DateFormatToRuString(notification.createdAt)}</div>
                    <div class="font-weight-600">${notification.title}</div>
                </a>
            `;            
        }

        return notificationHTML;
    }

    NotificationDetails (notification, gatewayUrl) {
        let recipients = [];
        notification.recipients.forEach(recipient => {
            recipients.push(recipient.userId);
        });

        if (notification != null && recipients.includes(window.authManager.userData.userId)) {
            const notificationDate = document.getElementById('notification-date');
            notificationDate.innerHTML = DateFormat.DateFormatToRuString(notification.createdAt);

            const notificationImage = document.getElementById('notification-image');
            notificationImage.setAttribute('src', `${gatewayUrl}${notification.imageUrl}`);

            const notificationTitle = document.getElementById('notification-title');
            notificationTitle.innerHTML = notification.title;
            
            const notificationMessage = document.getElementById('notification-message');
            notificationMessage.innerHTML = notification.message;

            /*if (notification.createdBy == window.authManager.userData.userId) {
                document.querySelector(`[data-action="remove-notification"]`).classList.remove('d-none');
            } else {
                document.querySelector(`[data-action="remove-notification"]`).remove();
            }*/
        } else {
            document.getElementById('notification-profile').innerHTML = 'Страница недоступна';
        }        
    }

    NotificationListByUserId (notifications, gatewayUrl) {
        const notificationsContainer = document.querySelector('.notifications-by-user-list');        
        if (notifications.length) {
            for (const notification of notifications) {
                console.log(`уведомление`);
                console.log(notification);
                const notificationToListByUserId = this.NotificationTemplateByUserId(notification, gatewayUrl);
                notificationsContainer.insertAdjacentHTML('beforeend', notificationToListByUserId);
            }
        } else {
            notificationsContainer.innerHTML = `Нет новых уведомлений`;
        }
    }

    NotificationTemplateByUserId(notification, gatewayUrl) {
        let notificationHTML;
        if (notification) {
            // Проверяем, есть ли текущий пользователь в recipients и прочитал ли он уведомление
            const currentUser = window.authManager?.userData?.userId || window.authManager?.userData?.id; // в зависимости от структуры
            const currentUserRecipient = notification.recipients?.find(recipient => recipient.userId === currentUser);

            const isReadByCurrentUser = currentUserRecipient && currentUserRecipient.readAt !== null;
            const readAt = isReadByCurrentUser ? currentUserRecipient.readAt : null;

            // Формируем класс и span для даты прочтения
            const readClass = isReadByCurrentUser ? 'readed' : '';
            const readAtSpan = isReadByCurrentUser 
                ? `<span class="read-at">Прочитано: ${DateFormat.DateFormatToRuString(readAt)}</span>`
                : '';

            notificationHTML = `
                <div class="profile-group dashboard-card my-4" data-group="notification" data-notification-id="${notification.id}">
                    <h3 class="card-header card-header_notification w-100 ${readClass}">
                        <a href="/notification/${notification.id}">${notification.title}</a>
                        ${readAtSpan}
                    </h3>

                    <div class="d-flex flex-wrap flex-md-nowrap gap-3 mt-4 w-100">
                        <div class="card-image" style="background-image: url(${gatewayUrl}${notification.imageUrl});"></div>
                        <div class="card-content">
                            <div id="notification-date" class="card-date mb-3">${DateFormat.DateFormatToRuString(notification.createdAt)}</div>                        
                            <div id="notification-message">${notification.message}</div>
                        </div>
                    </div>
                </div>
            `;            
        }

        return notificationHTML;
    }
}

document.addEventListener('authStateChanged', async () => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();
    const UrlParts = Regex.getUrlPathParts(window.location.href);

    if (isAuthenticated && userData) {
        const userId = window.authManager.userData.userId;
        const notificationProfile = new NotificationProfileManager();
        const notificationClient = new NotificationClient();

        console.log('Аутентификация пройдена');

        if (window.location.pathname.includes('/notification/create')) {
            await notificationProfile.InsertDataToCreateNotification();

            document.querySelector('[data-action="save-notification-data"]').addEventListener('click', async () => {                
                // Собираем данные уведомления
                const notificationData = await notificationProfile.CollectNotificationDataToCreate();
                console.log('Данные для сохранения:', notificationData);
                
                //Отправляем данные на сервер
                const notificationCreate = notificationClient.CreateNotification(notificationData);

                if (notificationCreate) {                        
                    Modal.ShowNotification('Уведомление создано успешно!', 'green');
                }
            });
        }

        let readTimeoutId = null;

        if (window.location.pathname == '/') {            
            const notificationsByUser = await notificationClient.GetUnreadNotificationsByUserId(userId);
            console.log(`уведомления для пользователя`);
            console.log(notificationsByUser);
            notificationProfile.InsertDataToMainPage(notificationsByUser);
        }

        if (Regex.isValidEntityUrl(window.location.href).valid && UrlParts.includes('notification')) {
            const notificationId = Regex.isValidEntityUrl(window.location.href).id;            
            const notification = await notificationClient.GetNotificationById(notificationId);
            notificationProfile.NotificationDetails(notification, notificationClient.gatewayUrl);

            if (readTimeoutId) {
                clearTimeout(readTimeoutId);
            }

            // Отсечка в 10 секунд для фиксации прочитанности сообщения
            readTimeoutId = setTimeout(async () => {
                try {
                    const result = await notificationClient.MarkAsReadByIds(notificationId, userId);
                    if (result) {
                        console.log('Уведомление отмечено как прочитанное');
                    } else {
                        console.warn('Не удалось отметить уведомление как прочитанное');
                    }
                } catch (error) {
                    console.error('Ошибка при отметке уведомления как прочитанного:', error);
                }
            }, 10000);

            // Отмена таймера при уходе со страницы, если ушел раньше 15 секунд, то сообщение не считается прочитанным
            window.addEventListener('beforeunload', () => {
                if (readTimeoutId) {
                    clearTimeout(readTimeoutId);
                    console.log('Таймер отменен при уходе со страницы');
                }
            });
        }

        if (UrlParts.includes(`notification`) && UrlParts.includes('by-user') && UrlParts.includes(userId)) {
            const notificationsByUser = await notificationClient.GetNotificationsByUserId(userId);            
            notificationProfile.NotificationListByUserId(notificationsByUser, notificationClient.gatewayUrl);
        }
    }
});