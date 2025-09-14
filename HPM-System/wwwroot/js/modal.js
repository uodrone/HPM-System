class Modal {
    constructor () {
        this.OpenModal();
        this.CloseModal();
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
}

document.addEventListener('DOMContentLoaded', () => {
    new Modal();
})