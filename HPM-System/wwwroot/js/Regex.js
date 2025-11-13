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
            const normalizedUrl = url.replace(/\/$/, '');
            const urlObj = new URL(normalizedUrl);
            const path = urlObj.pathname;

            const parts = path.split('/').filter(part => part !== '');

            // Случай 1: [type, id] — house или apartment с числовым ID
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

            // Случай 2: [context, guid] — notification или user с GUID
            if (parts.length === 2) {
                const [type, guid] = parts;
                if ((type === 'notification' || type === 'user') && this.isGuid(guid)) {
                    return {
                        valid: true,
                        id: guid, // GUID остаётся строкой
                        type: type
                    };
                }
            }

            return { valid: false, id: null, type: null };
        } catch (e) {
            return { valid: false, id: null, type: null };
        }
    }

    // Вспомогательный метод для проверки GUID v4
    isGuid(str) {
        const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
        return guidRegex.test(str);
    }
}

window.RegularExtension = RegularExtension;