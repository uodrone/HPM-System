export class DateFormat {
    constructor () {

    }

    static DateFormatToRuString(isoString) {
        const date = new Date(isoString); // ISO строка автоматически интерпретируется как UTC

        const day = date.getDate();
        const monthIndex = date.getMonth();
        const year = date.getFullYear();
        const hours = date.getHours();
        const minutes = date.getMinutes();

        const months = [
            'января', 'февраля', 'марта', 'апреля', 'мая', 'июня',
            'июля', 'августа', 'сентября', 'октября', 'ноября', 'декабря'
        ];

        const month = months[monthIndex];

        // Форматируем часы и минуты с ведущим нулём при необходимости
        const hh = String(hours).padStart(2, '0');
        const mm = String(minutes).padStart(2, '0');

        return `${day} ${month} ${year}, ${hh}:${mm}`;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new DateFormat();
})