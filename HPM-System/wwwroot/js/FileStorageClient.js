export class FileStorageClient {
    constructor() {
        // ИЗМЕНЕНИЕ 1: Используем Gateway вместо прямого адреса микросервиса
        this.gatewayUrl = 'http://localhost:55699'; // Gateway
        this.apiPath = '/api/files';
    }

    /**
     * Получить полный URL для эндпоинта (через Gateway)
     * @param {string} endpoint 
     * @returns {string}
     */
    _getUrl(endpoint) {
        return `${this.gatewayUrl}${this.apiPath}${endpoint}`;
    }

    /**
     * Загрузить файл на сервер
     * @param {File} file - Файл для загрузки
     * @returns {Promise<Object>} Метаданные загруженного файла с URL
     * @property {number} id - ID файла
     * @property {string} fileUrl - URL для просмотра файла
     * @property {string} downloadUrl - URL для скачивания файла
     * @example
     * const result = await client.uploadFile(file);
     * console.log('Файл доступен по:', result.fileUrl);
     * document.getElementById('preview').src = result.fileUrl;
     */
    async UploadFile(file) {
        if (!(file instanceof File)) {
            throw new Error('Параметр должен быть экземпляром File');
        }

        const formData = new FormData();
        formData.append('file', file);

        try {
            // ИЗМЕНЕНИЕ 2: Используем window.apiCall вместо fetch для автоматической авторизации
            const response = await window.apiCall(this._getUrl('/upload'), {
                method: 'POST',
                body: formData
                // Не устанавливаем Content-Type — браузер сам задаст multipart/form-data с boundary
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка загрузки: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при загрузке файла:', error);
            throw error;
        }
    }

    /**
     * Получить URL для просмотра файла по имени (публичный URL через Gateway)
     * @param {string} bucketName - Имя бакета
     * @param {string} fileName - Имя файла
     * @returns {string}
     */
    GetFileViewUrl(bucketName, fileName) {
        return this._getUrl(`/view/${bucketName}/${fileName}`);
    }

    /**
     * Получить URL для скачивания файла по ID (публичный URL через Gateway)
     * @param {number} id - ID файла
     * @returns {string}
     */
    GetFileDownloadUrl(id) {
        return this._getUrl(`/download/${id}`);
    }

    /**
     * Получить метаданные файла по ID
     * @param {number} id - ID файла
     * @returns {Promise<Object>}
     */
    async GetFileMetadata(id) {
        try {
            const response = await window.apiCall(this._getUrl(`/${id}`), {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения метаданных: ${errorText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при получении метаданных:', error);
            throw error;
        }
    }

    /**
     * Скачать файл по ID
     * @param {number} id - ID файла
     * @param {string} saveAs - Имя файла для сохранения (опционально)
     */
    async DownloadFile(id, saveAs = null) {
        try {
            const response = await window.apiCall(this._getUrl(`/download/${id}`), {
                method: 'GET'
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка скачивания: ${errorText}`);
            }

            const blob = await response.blob();

            // Получаем имя файла из заголовка или используем переданное
            let fileName = saveAs;
            if (!fileName) {
                const contentDisposition = response.headers.get('Content-Disposition');
                if (contentDisposition) {
                    const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
                    if (matches?.[1]) {
                        fileName = matches[1].replace(/['"]/g, '');
                    }
                }
            }
            fileName = fileName || `file-${id}`;

            // Создаем ссылку для скачивания
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();

            // Очищаем
            URL.revokeObjectURL(url);
            document.body.removeChild(a);
        } catch (error) {
            console.error('Ошибка при скачивании файла:', error);
            throw error;
        }
    }

    /**
     * Получить Blob файла
     * @param {number} id - ID файла
     * @returns {Promise<Blob>}
     * @example
     * const blob = await client.getFileBlob(123);
     * const url = URL.createObjectURL(blob);
     * document.getElementById('preview').src = url;
     */
    async GetFileBlob(id) {
        try {
            const response = await window.apiCall(this._getUrl(`/download/${id}`), {
                method: 'GET'
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка получения файла: ${errorText}`);
            }

            return await response.blob();
        } catch (error) {
            console.error('Ошибка при получении blob:', error);
            throw error;
        }
    }

    /**
     * Получить URL для предпросмотра файла
     * @param {number} id - ID файла
     * @returns {Promise<string>}
     */
    async GetFilePreviewUrl(id) {
        const blob = await this.GetFileBlob(id);
        return URL.createObjectURL(blob);
    }

    /**
     * Удалить файл по ID
     * @param {number} id - ID файла
     * @returns {Promise<boolean>}
     * @example
     * await client.deleteFile(123);
     * console.log('Файл удален');
     */
    async DeleteFile(id) {
        try {
            const response = await window.apiCall(this._getUrl(`/${id}`), {
                method: 'DELETE'
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Ошибка удаления: ${errorText}`);
            }

            return true;
        } catch (error) {
            console.error('Ошибка при удалении файла:', error);
            throw error;
        }
    }

    /**
     * Загрузить несколько файлов последовательно
     * @param {File[]} files - Массив файлов
     * @returns {Promise<Object[]>}
     */
    async UploadMultipleFiles(files) {
        const results = [];

        for (const file of files) {
            try {
                const result = await this.UploadFile(file);
                results.push({ success: true, fileName: file.name, data: result });
            } catch (error) {
                results.push({ success: false, fileName: file.name, error: error.message });
            }
        }

        return results;
    }

    /**
     * Загрузить несколько файлов параллельно
     * @param {File[]} files - Массив файлов
     * @returns {Promise<Object[]>}
     */
    async UploadMultipleFilesParallel(files) {
        const uploadPromises = files.map(async (file) => {
            try {
                const result = await this.UploadFile(file);
                return { success: true, fileName: file.name, data: result };
            } catch (error) {
                return { success: false, fileName: file.name, error: error.message };
            }
        });

        return await Promise.all(uploadPromises);
    }

    /**
     * Установить базовый URL (редко нужно, но оставлено для совместимости)
     * @param {string} newBaseUrl 
     */
    SetBaseUrl(newBaseUrl) {
        this.gatewayUrl = newBaseUrl.endsWith('/') ? newBaseUrl.slice(0, -1) : newBaseUrl;
    }

    /**
     * Получить текущий базовый URL
     * @returns {string}
     */
    GetBaseUrl() {
        return this.gatewayUrl;
    }
}

// Инициализация при авторизации (остаётся без изменений)
document.addEventListener('authStateChanged', () => {
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        // Экземпляр можно создать, но он не хранится глобально — лучше создавать по месту использования
        // window.fileStorageClient = new FileStorageClient(); // опционально
    }

// 9. Работа с Blob для встраивания
async function embedFile(fileId, targetElement) {
    const blob = await fileClient.getFileBlob(fileId);
    const url = URL.createObjectURL(blob);
    
    targetElement.src = url;
    
    // Очистка после использования
    targetElement.addEventListener('load', () => {
        URL.revokeObjectURL(url);
});
}
*/