export class DateFormat {
    constructor () {

    }

    static DateFormatToRuString (isoString) {
        const date = new Date(isoString);
        const hours = date.getHours();
        const minutes = date.getMinutes();
        const day = date.getUTCDate(); // без ведущего нуля
        const year = date.getUTCFullYear();

        const months = [
            'января', 'февраля', 'марта', 'апреля', 'мая', 'июня',
            'июля', 'августа', 'сентября', 'октября', 'ноября', 'декабря'
        ];

        const month = months[date.getUTCMonth()]; // getUTCMonth() → 0–11

        return `${day} ${month} ${year}, ${hours}:${minutes}`;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new DateFormat();
})