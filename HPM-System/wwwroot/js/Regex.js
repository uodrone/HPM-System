export class RegularExtension {
    constructor() {}

    getUrlPathParts(url) {
        try {
            const urlObj = new URL(url);
            const path = urlObj.pathname;
            return path.split('/').filter(part => part !== '');
        } catch (e) {
            // Если URL некорректен — возвращаем пустой массив
            return [];
        }
    }

    isValidEntityUrl(url) {
        try {
            // Убираем завершающий слэш из всего URL (если есть)
            const normalizedUrl = url.replace(/\/$/, '');

            const urlObj = new URL(normalizedUrl);
            const path = urlObj.pathname;

            const parts = path.split('/').filter(part => part !== '');

            // Должно быть ровно две части: [тип, id]
            if (parts.length === 2) {
                const [type, idStr] = parts;

                if ((type === 'house' || type === 'apartment') && /^\d+$/.test(idStr)) {
                    return {
                        valid: true,
                        id: parseInt(idStr, 10),
                        type: type
                    };
                }
            }

            return { valid: false, id: null, type: null };
        } catch (e) {
            return { valid: false, id: null, type: null };
        }
    }
}

window.RegularExtension = RegularExtension;