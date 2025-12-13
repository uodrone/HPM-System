import { Modal } from './Modal.js';
import { ApartmentHouses } from './ApartmentHouses.js';
import { EventClient } from './EventClient.js';
import { FileStorageClient } from './FileStorageClient.js';
import { DateFormat } from './DateFormat.js';

export class NotificationProfileManager {
    constructor () {
        this.houseProfile = new ApartmentHouses();
        this.userId = window.authManager.userData.userId;
    }

    async InsertDataToCreateEvent() {
        const houseSelector = document.getElementById('houseId');
        const eventGroup = document.querySelector('.profile-group[data-group="event"]');

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
        } else {
            // Ни в одном доме не глава => событие недоступно
            if (eventGroup) {
                eventGroup.innerHTML = `Создание события недоступно`;
            }
        }
    }

    async CollectEventDataToCreate() {
        //собираем картинку
        const fileInput = document.getElementById('fileInput');
        const file = fileInput.files[0];

        const fileManager = new FileStorageClient();
        let isFileUpload = await fileManager.UploadFile(file);
        console.log(`результат загрузки файла картинки`);
        console.log(isFileUpload);

        let eventData = null;

        if (isFileUpload != null) {
            eventData = {
                title: document.getElementById('title').value,
                description: document.getElementById('message').value,
                imageUrl: isFileUpload.fileUrl,
                eventDateTime: new Date(document.getElementById('eventDateTime').value).toISOString(),
                place: document.getElementById('place').value,
                communityId: document.getElementById('houseId').value,
                communityType: 0
            };
        }

        return eventData;
    }

    InsertDataToMainPage (data) {
        const eventsContainer = document.querySelector('.events-list');        
        if (data.length) {
            data.forEach((event) => {
                const eventMainPageTemplate = this.EventMainPageTemplate(event);
                eventsContainer.insertAdjacentHTML('beforeend', eventMainPageTemplate);
            });
        } else {
            eventsContainer.innerHTML = `Нет новых уведомлений`;
        }
    }

    EventMainPageTemplate (event) {
        let eventHTML;
        if (event) {
            eventHTML = `
                <a class="event-item" href="/event/${event.id}">
                    <div class="font-size-12 color-gray">${DateFormat.DateFormatToRuString(event.eventDateTime)}</div>
                    <div class="font-weight-600">${event.title}</div>
                </a>
            `;            
        }

        return eventHTML;
    }
}

document.addEventListener('authStateChanged', async () => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();
    const UrlParts = Regex.getUrlPathParts(window.location.href);

    if (isAuthenticated && userData) {
        const userId = window.authManager.userData.userId;
        const eventProfile = new NotificationProfileManager();
        const eventClient = new EventClient();

        console.log('Аутентификация пройдена');

        if (window.location.pathname.includes('/event/create')) {
            await eventProfile.InsertDataToCreateEvent();

            document.querySelector('[data-action="save-event-data"]').addEventListener('click', async () => {                
                // Собираем данные уведомления
                const eventData = await eventProfile.CollectEventDataToCreate();
                console.log('Данные для сохранения:', eventData);
                
                //Отправляем данные на сервер
                const eventCreate = eventClient.CreateEvent(eventData);

                if (eventCreate) {                        
                    Modal.ShowNotification('Событие создано успешно!', 'green');                        
                }
            });
        }

        if (window.location.pathname == '/') {            
            const eventByUser = await eventClient.GetUserEvents(userId);
            console.log(`события для пользователя`);
            console.log(eventByUser);
            eventProfile.InsertDataToMainPage(eventByUser);
        }


        if (UrlParts.includes(`event`) && UrlParts.includes('by-user') && UrlParts.includes(userId)) {
            const eventsByUser = await eventClient.GetUserEvents(userId);            
            eventProfileProfile.EventListListByUserId(notificationsByUser, notificationClient.gatewayUrl);
        }
    }
});