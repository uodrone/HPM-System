class FileHandler {
    constructor(dropAreaSelector, fileInputSelector, previewImageSelector, removeBtnSelector, errorSelector) {
        this.dropArea = document.querySelector(dropAreaSelector);
        this.fileInput = document.querySelector(fileInputSelector);
        this.previewImage = document.querySelector(previewImageSelector);
        this.previewContainer = this.previewImage.closest('.preview-container');
        this.removeBtn = document.querySelector(removeBtnSelector);
        this.errorMessage = document.querySelector(errorSelector);

        this.maxFileSize = 10 * 1024 * 1024; // 10 МБ
        this.allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/svg+xml', 'image/gif'];
        this.allowedExtensions = ['.jpg', '.jpeg', '.png', '.webp', '.svg', '.gif'];

        this.init();
    }

    init() {
     this.bindEvents();
    }

    bindEvents() {
        // Выбор через клик
        this.dropArea.addEventListener('click', () => this.fileInput.click());
        this.fileInput.addEventListener('change', (e) => this.onFileSelected(e));

        // Drag & Drop
        const preventDefaults = (e) => {
            e.preventDefault();
            e.stopPropagation();
        };

        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            this.dropArea.addEventListener(eventName, preventDefaults, false);
        });

        ['dragenter', 'dragover'].forEach(eventName => {
            this.dropArea.addEventListener(eventName, () => this.dropArea.classList.add('drag-over'), false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            this.dropArea.addEventListener(eventName, () => this.dropArea.classList.remove('drag-over'), false);
        });

        this.dropArea.addEventListener('drop', (e) => this.onDrop(e));

        // Кнопка удаления
        this.removeBtn.addEventListener('click', () => this.resetPreview());
    }

    onFileSelected(event) {
        const file = event.target.files[0];
        if (file) this.processFile(file);
    }

    onDrop(event) {
        const file = event.dataTransfer.files[0];
        if (file) this.processFile(file);
    }

    processFile(file) {
        if (!this.isValidFile(file)) return;
        this.displayPreview(file);
    }

    isValidFile(file) {
        // Проверка расширения
        const ext = '.' + file.name.split('.').pop().toLowerCase();
        const hasValidExt = this.allowedExtensions.includes(ext);
        const hasValidType = this.allowedTypes.includes(file.type);

        if (!hasValidExt && !hasValidType) {
            this.showError('Недопустимый формат файла. Разрешены: JPG, JPEG, PNG, WEBP, SVG, GIF.');
            return false;
        }

        if (file.size > this.maxFileSize) {
            this.showError('Файл слишком большой. Максимум — 10 МБ.');
            return false;
        }

        return true;
    }

    displayPreview(file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            this.previewImage.src = e.target.result;
            this.previewImage.style.display = 'inline-block';
            this.removeBtn.style.display = 'inline-block';
            this.previewContainer.classList.remove('d-none');
            this.dropArea.classList.add('d-none');
        };
        reader.readAsDataURL(file);
    }

    resetPreview() {
        this.previewImage.src = '';
        this.previewImage.style.display = 'none';
        this.removeBtn.style.display = 'none';
        this.fileInput.value = '';
        this.previewContainer.classList.add('d-none');
        this.dropArea.classList.remove('d-none');
    }

    showError(message) {
        this.errorMessage.textContent = message;
        setTimeout(() => {
            this.errorMessage.textContent = '';
        }, 5000);
    }
}

    

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('imgDropArea') != null) {
        new FileHandler('#imgDropArea', '#fileInput', '#previewImage', '#removeBtn', '#errorMessage');
    }    
});