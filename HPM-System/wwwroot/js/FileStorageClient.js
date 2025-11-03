export class FileStorageClient {
    constructor() {
        this.baseUrl = 'https://localhost:55693';
        this.apiPath = '/api/files';
    }

    /**
     * Получить полный URL для эндпоинта
     * @param {string} endpoint 
     * @returns {string}
     */
    _getUrl(endpoint) {
        return `${this.baseUrl}${this.apiPath}${endpoint}`;
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
    async uploadFile(file) {
        if (!(file instanceof File)) {
            throw new Error('Параметр должен быть экземпляром File');
        }

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await fetch(this._getUrl('/upload'), {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка загрузки: ${error}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка при загрузке файла:', error);
            throw error;
        }
    }

    /**
     * Получить URL для просмотра файла по имени
     * @param {string} bucketName - Имя бакета
     * @param {string} fileName - Имя файла
     * @returns {string} URL для просмотра
     * @example
     * const url = client.getFileViewUrl('documents', 'abc123_document.pdf');
     * document.getElementById('preview').src = url;
     */
    getFileViewUrl(bucketName, fileName) {
        return this._getUrl(`/view/${bucketName}/${fileName}`);
    }

    /**
     * Получить URL для скачивания файла по ID
     * @param {number} id - ID файла
     * @returns {string} URL для скачивания
     * @example
     * const url = client.getFileDownloadUrl(123);
     * window.open(url, '_blank');
     */
    getFileDownloadUrl(id) {
        return this._getUrl(`/download/${id}`);
    }

    /**
     * Получить метаданные файла по ID
     * @param {number} id - ID файла
     * @returns {Promise<Object>} Метаданные файла
     * @example
     * const metadata = await client.getFileMetadata(123);
     * console.log('Размер файла:', metadata.fileSize);
     */
    async getFileMetadata(id) {
        try {
            const response = await fetch(this._getUrl(`/${id}`), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка получения метаданных: ${error}`);
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
     * @param {string} saveAs - Имя файла для сохранения (опционально, по умолчанию из сервера)
     * @returns {Promise<void>}
     * @example
     * await client.downloadFile(123, 'my-document.pdf');
     */
    async downloadFile(id, saveAs = null) {
        try {
            const response = await fetch(this._getUrl(`/download/${id}`), {
                method: 'GET'
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка скачивания: ${error}`);
            }

            const blob = await response.blob();
            
            // Получаем имя файла из заголовка или используем переданное
            let fileName = saveAs;
            if (!fileName) {
                const contentDisposition = response.headers.get('Content-Disposition');
                if (contentDisposition) {
                    const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
                    if (matches && matches[1]) {
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
     * Получить Blob файла (без автоматического скачивания)
     * Полезно для предпросмотра изображений или встраивания в страницу
     * @param {number} id - ID файла
     * @returns {Promise<Blob>}
     * @example
     * const blob = await client.getFileBlob(123);
     * const url = URL.createObjectURL(blob);
     * document.getElementById('preview').src = url;
     */
    async getFileBlob(id) {
        try {
            const response = await fetch(this._getUrl(`/download/${id}`), {
                method: 'GET'
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка получения файла: ${error}`);
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
     * @returns {Promise<string>} Object URL для использования в src
     * @example
     * const url = await client.getFilePreviewUrl(123);
     * document.getElementById('image').src = url;
     * // Не забудьте вызвать URL.revokeObjectURL(url) когда URL больше не нужен
     */
    async getFilePreviewUrl(id) {
        const blob = await this.getFileBlob(id);
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
    async deleteFile(id) {
        try {
            const response = await fetch(this._getUrl(`/${id}`), {
                method: 'DELETE'
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(`Ошибка удаления: ${error}`);
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
     * @returns {Promise<Object[]>} Массив результатов загрузки
     * @example
     * const results = await client.uploadMultipleFiles(filesArray);
     * const successful = results.filter(r => r.success);
     * console.log(`Загружено ${successful.length} из ${results.length}`);
     */
    async uploadMultipleFiles(files) {
        const results = [];

        for (const file of files) {
            try {
                const result = await this.uploadFile(file);
                results.push({ 
                    success: true, 
                    fileName: file.name, 
                    data: result 
                });
            } catch (error) {
                results.push({ 
                    success: false, 
                    fileName: file.name, 
                    error: error.message 
                });
            }
        }

        return results;
    }

    /**
     * Загрузить несколько файлов параллельно (быстрее, но больше нагрузка на сервер)
     * @param {File[]} files - Массив файлов
     * @returns {Promise<Object[]>} Массив результатов загрузки
     * @example
     * const results = await client.uploadMultipleFilesParallel(filesArray);
     */
    async uploadMultipleFilesParallel(files) {
        const uploadPromises = files.map(async (file) => {
            try {
                const result = await this.uploadFile(file);
                return { 
                    success: true, 
                    fileName: file.name, 
                    data: result 
                };
            } catch (error) {
                return { 
                    success: false, 
                    fileName: file.name, 
                    error: error.message 
                };
            }
        });

        return await Promise.all(uploadPromises);
    }

    /**
     * Установить базовый URL
     * @param {string} newBaseUrl 
     */
    setBaseUrl(newBaseUrl) {
        this.baseUrl = newBaseUrl.endsWith('/') ? newBaseUrl.slice(0, -1) : newBaseUrl;
    }

    /**
     * Получить текущий базовый URL
     * @returns {string}
     */
    getBaseUrl() {
        return this.baseUrl;
    }
}

document.addEventListener('authStateChanged', async () => {    
    const { isAuthenticated, userData } = event.detail;

    if (isAuthenticated && userData) {
        const FileStorage = new FileStorageClient();
    }
});

// ============================================
// Примеры использования
// ============================================

/*
// 1. Создание клиента
const fileClient = new FileStorageClient('http://localhost:55692');

// 2. Загрузка одного файла
const fileInput = document.getElementById('fileInput');
fileInput.addEventListener('change', async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    
    try {
        const result = await fileClient.uploadFile(file);
        console.log('Файл загружен:', result);
        // result = { id: 123, bucket: "documents", message: "...", originalFileName: "..." }
    } catch (error) {
        console.error('Ошибка загрузки:', error.message);
    }
});

// 3. Загрузка нескольких файлов
const multiFileInput = document.getElementById('multiFileInput');
multiFileInput.addEventListener('change', async (e) => {
    const files = Array.from(e.target.files);
    
    // Последовательная загрузка (меньше нагрузка на сервер)
    const results = await fileClient.uploadMultipleFiles(files);
    
    // Или параллельная (быстрее)
    // const results = await fileClient.uploadMultipleFilesParallel(files);
    
    console.log('Результаты:', results);
});

// 4. Получение метаданных
const metadata = await fileClient.getFileMetadata(123);
console.log('Информация о файле:', metadata);

// 5. Скачивание файла
await fileClient.downloadFile(123); // Автоматически начнет загрузку
// или с кастомным именем:
await fileClient.downloadFile(123, 'мой-документ.pdf');

// 6. Предпросмотр изображения
const imageUrl = await fileClient.getFilePreviewUrl(123);
document.getElementById('preview').src = imageUrl;
// Важно: очистите URL когда он больше не нужен
// URL.revokeObjectURL(imageUrl);

// 7. Удаление файла
const deleteBtn = document.getElementById('deleteBtn');
deleteBtn.addEventListener('click', async () => {
    try {
        await fileClient.deleteFile(123);
        console.log('Файл удален');
    } catch (error) {
        console.error('Ошибка удаления:', error.message);
    }
});

// 8. Пример с async/await в try-catch
async function handleFileUpload(file) {
    try {
        const uploadResult = await fileClient.uploadFile(file);
        console.log('ID загруженного файла:', uploadResult.id);
        
        const metadata = await fileClient.getFileMetadata(uploadResult.id);
        console.log('Размер файла:', metadata.fileSize, 'байт');
        
        return uploadResult.id;
    } catch (error) {
        console.error('Произошла ошибка:', error.message);
        throw error;
    }
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