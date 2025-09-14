export class Modal {
    constructor () {
        this.OpenModal();
        this.CloseModal();
        this.CloseModalOnOutsideClick();
    }

    OpenModal () {
      document.querySelectorAll('[data-modal="open"]').forEach(modalCall => {
        modalCall.addEventListener('click', function () {
          document.querySelector('.modal-overview').classList.add('active');
        });
      });
    }

    CloseModal () {
      let crossModal = document.querySelectorAll('.modal-close');
      crossModal.forEach(cross => {
        cross.addEventListener('click', () => {
          cross.closest('.modal-overview').classList.remove('active');
        });
      });
    }

    CloseModalOnOutsideClick () {
      const modalOverview = document.querySelector('.modal-overview');
      modalOverview.addEventListener('click', (event) => {        
        if (!event.target.classList.contains('modal-content-wrapper') && event.target.closest('.modal-content-wrapper') == null) {          
          modalOverview.classList.remove('active');
        }
      });
    }

    static ShowNotification(text, color) {
        const notification = document.createElement('div');
        notification.className = 'notification';
        notification.textContent = text;
        notification.style.backgroundColor = color;
        
        // Добавляем уведомление в документ
        document.body.appendChild(notification);
        
        // Через 2 секунды удаляем уведомление
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 2000);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new Modal();
})