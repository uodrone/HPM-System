import { Modal } from './Modal.js';
import { ApartmentHouses } from './ApartmentHouses.js';
import { NotificationClient } from './NotificationClient.js';
import { FileStorageClient } from './FileStorageClient.js';

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

        // Массив промисов для параллельного выполнения
        const houseChecks = houses.map(async (house) => {
            const houseHead = await this.houseProfile.GetHead(house.id);
            return { house, isHead: houseHead.id === this.userId };
        });

        // Дожидаемся всех проверок
        const results = await Promise.all(houseChecks);

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
        let isFileUpload = await fileManager.uploadFile(file);
        console.log(`результат загрузки файла картинки`);
        console.log(isFileUpload);

        return {
            apartmentIds: apartmentIds,
            userIds: uniqueUserIds,
            isOwnersOnly: isOwnersOnly
        };
    }
}

document.addEventListener('authStateChanged', async () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const notificationProfile = new NotificationProfileManager();

        console.log('Аутентификация пройдена');

        if (window.location.pathname.includes('/notification/create')) {
            await notificationProfile.InsertDataToCreateNotification();

            document.querySelector('[data-action="save-notification-data"]').addEventListener('click', () => {
                console.log('Клик по кнопке сохранения уведомления');
                
                // Собираем данные уведомления
                const notificationData = notificationProfile.CollectNotificationDataToCreate();
                console.log('Данные для сохранения:', notificationData);
                
                // Здесь можно отправить данные на сервер
                // await notificationClient.CreateNotification(notificationData);
            });
        }
    }
});