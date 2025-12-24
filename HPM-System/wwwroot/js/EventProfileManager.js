import { Modal } from './Modal.js';
import { ApartmentHouses } from './ApartmentHouses.js';
import { EventClient } from './EventClient.js';
import { FileStorageClient } from './FileStorageClient.js';
import { DateFormat } from './DateFormat.js';

export class EventProfileManager {
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
                <a class="card-item card-item_event" href="/event/${event.id}">
                    <div class="font-size-12 color-gray">${DateFormat.DateFormatToRuString(event.eventDateTime)}</div>
                    <div class="font-weight-600">${event.title}</div>
                </a>
            `;            
        }

        return eventHTML;
    }

    EventDetails (event, gatewayUrl) {
        console.log(`событие`);
        console.log(event);

        if (event != null) {
            const eventData = document.getElementById('event-date');
            eventData.innerHTML = DateFormat.DateFormatToRuString(event.eventDateTime);

            const eventImage = document.getElementById('event-image');
            eventImage.setAttribute('src', `${gatewayUrl}${event.imageUrl}`);

            const eventTitle = document.getElementById('event-title');
            eventTitle.innerHTML = event.title;
            
            const eventDescription = document.getElementById('event-description');
            eventDescription.innerHTML = event.description;
        } else {
            document.getElementById('event-profile').innerHTML = 'Страница недоступна';
        }        
    }

    EventsListByUserId (events, gatewayUrl) {
        const eventsContainer = document.querySelector('.events-by-user-list');        
        if (events.length) {
            for (const event of events) {
                console.log(`событие`);
                console.log(event);
                const eventToListByUserId = this.EventTemplateByUserId(event, gatewayUrl);
                eventsContainer.insertAdjacentHTML('beforeend', eventToListByUserId);
            }
        } else {
            eventsContainer.innerHTML = `Нет новых уведомлений`;
        }
    }
    
    EventTemplateByUserId(event, gatewayUrl) {
        let eventHTML;
        if (event) {
            eventHTML = `
                <div class="profile-group dashboard-card my-4" data-group="event" data-event-id="${event.id}">
                    <h3 class="card-header card-header_event w-100">
                        <a href="/notification/${event.id}">${event.title}</a>
                    </h3>

                    <div class="d-flex flex-wrap flex-md-nowrap gap-3 mt-4 w-100">
                        <div class="card-image" style="background-image: url(${gatewayUrl}${event.imageUrl});"></div>
                        <div class="card-content">
                            <div id="notification-date" class="card-date mb-3">${DateFormat.DateFormatToRuString(event.eventDateTime)}</div>                        
                            <div id="notification-message">${event.description}</div>
                        </div>
                    </div>
                </div>
            `;            
        }

        return eventHTML;
    }

    HideButtonsSubcribeToEvent (IsCurrentUserSubscribed) {
        if (IsCurrentUserSubscribed) {
            document.querySelector('.btn[data-action="subscribe-to-event"]').classList.add('d-none');
            document.querySelector('.btn[data-action="unsubscribe-to-event"]').classList.remove('d-none');
        } else {
            document.querySelector('.btn[data-action="unsubscribe-to-event"]').classList.add('d-none');
            document.querySelector('.btn[data-action="subscribe-to-event"]').classList.remove('d-none');
        }
    }

    SubscribeUnsubscribeActions (eventClient, eventId, IsCurrentUserSubscribed) {
        this.HideButtonsSubcribeToEvent(IsCurrentUserSubscribed);

        if (document.querySelector('.btn[data-action="subscribe-to-event"]') != null) {
            document.querySelector('.btn[data-action="subscribe-to-event"]').addEventListener('click', async () => {
                const subscribe = await eventClient.SubscribeToEvent(eventId);                        

                if (subscribe) {
                    this.HideButtonsSubcribeToEvent(subscribe);
                    Modal.ShowNotification('Подписка на событие прошла успешно!', 'green');
                }
            });
        }
        
        if (document.querySelector('.btn[data-action="unsubscribe-to-event"]') != null) {
            document.querySelector('.btn[data-action="unsubscribe-to-event"]').addEventListener('click', async () => {
                const unsubscribe = await eventClient.UnsubscribeFromEvent(eventId);
                
                if (unsubscribe) {
                    Modal.ShowNotification('Подписка на событие прошла успешно!', 'green');
                    this.HideButtonsSubcribeToEvent(!unsubscribe);
                }                        
            });
        }
    }
}

document.addEventListener('authStateChanged', async () => {
    const { isAuthenticated, userData } = event.detail;
    const Regex = new window.RegularExtension();
    const UrlParts = Regex.getUrlPathParts(window.location.href);

    if (isAuthenticated && userData) {
        const userId = window.authManager.userData.userId;
        const eventProfile = new EventProfileManager();
        const eventClient = new EventClient();

        if (window.location.pathname.includes('/event/create')) {
            await eventProfile.InsertDataToCreateEvent();

            document.querySelector('[data-action="save-event-data"]').addEventListener('click', async () => {                
                // Собираем данные уведомления
                const eventData = await eventProfile.CollectEventDataToCreate();
                console.log('Данные для сохранения:', eventData);
                
                //Отправляем данные на сервер
                const eventCreate = await eventClient.CreateEvent(eventData);

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

        if (UrlParts.includes(`event`)) {
            if (UrlParts.includes('by-user') && UrlParts.includes(userId)) {
                const EventsListByUserId = await eventClient.GetUserEvents();
                console.log('все события пользователя:');
                console.log(EventsListByUserId);
                eventProfile.EventsListByUserId(EventsListByUserId, eventClient.gatewayUrl);
            } else if (!isNaN(Number(UrlParts[1]))) {     
                const eventId = UrlParts[1];           
                const isUserParticipant = await eventClient.isUserParticipant(userId, eventId);
                if (isUserParticipant) {
                    const event = await eventClient.GetEventById(eventId);
                    eventProfile.EventDetails(event, eventClient.gatewayUrl);

                    const IsCurrentUserSubscribed = await eventClient.IsCurrentUserSubscribed(eventId);                    
                    eventProfile.SubscribeUnsubscribeActions(eventClient, eventId, IsCurrentUserSubscribed);
                } else {
                    document.getElementById('event-profile').innerHTML = 'Страница недоступна';
                }
            }
        }
    }
});