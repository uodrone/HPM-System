export class RegularExtension {
    constructor() {

    }

    isValidHouseUrl(url) {
        try {
            // Убираем завершающий слэш из всего URL (если есть)
            const normalizedUrl = url.replace(/\/$/, '');

            const urlObj = new URL(normalizedUrl);
            const path = urlObj.pathname;

            const parts = path.split('/').filter(part => part !== '');

            if (parts.length === 2 && parts[0] === 'house' && /^\d+$/.test(parts[1])) {
                return { valid: true, id: parseInt(parts[1], 10) };
            }

            return { valid: false, id: null };
        } catch (e) {
            return { valid: false, id: null };
        }
    }
}