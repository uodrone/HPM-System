class Multiselect {
    constructor() {
        this.instances = new Map(); // храним активные инстансы по ID
    }

    /**
     * Инициализирует Tom Select с чекбоксами на <select multiple>
     * @param {string} elementId — ID элемента <select>
     * @param {Object} [customOptions] — дополнительные опции Tom Select
     * @returns {TomSelect | null}
     */
    init(elementId, customOptions = {}) {
        const selectElement = document.getElementById(elementId);

        if (!selectElement) {
            console.warn(`Multiselect: элемент с id="${elementId}" не найден.`);
            return null;
        }

        // Защита от повторной инициализации
        if (this.instances.has(elementId)) {
            console.warn(`Multiselect: уже инициализирован на "${elementId}".`);
            return this.instances.get(elementId);
        }

        // Базовые настройки: мультиселект + чекбоксы
        const defaultOptions = {
            plugins: {
                checkbox_options: {} // ← чекбоксы в выпадающем списке
            },
            maxItems: null,          // разрешить множественный выбор
            hidePlaceholder: true,
            closeAfterSelect: false, // не закрывать после выбора (удобно при мультивыборе)
            dropdownParent: 'body'   // чтобы не обрезалось в модалках и т.п.
        };

        const finalOptions = { ...defaultOptions, ...customOptions };

        // Создаём экземпляр Tom Select
        const tomSelectInstance = new TomSelect(`#${elementId}`, finalOptions);

        // Сохраняем для последующего доступа
        this.instances.set(elementId, tomSelectInstance);
        selectElement._tomSelect = tomSelectInstance;

        return tomSelectInstance;
    }

    /**
     * Получить экземпляр Tom Select по ID
     * @param {string} elementId
     * @returns {TomSelect | undefined}
     */
    getInstance(elementId) {
        return this.instances.get(elementId);
    }

    /**
     * Уничтожить инстанс (например, при удалении из DOM)
     * @param {string} elementId
     */
    destroy(elementId) {
        const instance = this.instances.get(elementId);
        if (instance) {
            instance.destroy();
            this.instances.delete(elementId);
            const el = document.getElementById(elementId);
            if (el) delete el._tomSelect;
        }
    }

    /**
     * Получить выбранные значения (массив строк)
     * @param {string} elementId
     * @returns {string[]}
     */
    getValues(elementId) {
        const instance = this.getInstance(elementId);
        return instance ? instance.getValue() : [];
    }

    /**
     * Установить значения программно
     * @param {string} elementId
     * @param {string[]} values
     */
    setValues(elementId, values) {
        const instance = this.getInstance(elementId);
        if (instance) {
            instance.setValue(values, false); // false = не вызывать onChange
        }
    }
}

window.Multiselect = Multiselect;