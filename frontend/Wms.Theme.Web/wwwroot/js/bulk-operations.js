/**
 * Bulk Operations Module
 * Handles checkbox selection, bulk actions, and toolbar management
 */

class BulkOperationsManager {
    constructor(options = {}) {
        this.options = {
            selectAllSelector: '#selectAllCheckbox',
            itemCheckboxSelector: '.item-checkbox',
            bulkToolbarSelector: '.bulk-toolbar',
            selectedCountSelector: '.selected-count',
            bulkDeleteBtnSelector: '.bulk-delete-btn',
            bulkStatusBtnSelector: '.bulk-status-btn',
            entityType: options.entityType || 'items',
            ...options
        };
        
        this.init();
    }

    init() {
        const selectAllCheckbox = document.querySelector(this.options.selectAllSelector);
        const itemCheckboxes = document.querySelectorAll(this.options.itemCheckboxSelector);
        const bulkDeleteBtn = document.querySelector(this.options.bulkDeleteBtnSelector);
        const bulkStatusBtn = document.querySelector(this.options.bulkStatusBtnSelector);

        if (!selectAllCheckbox || itemCheckboxes.length === 0) {
            console.warn('Bulk operations: required elements not found');
            return;
        }

        // Select all checkbox
        if (selectAllCheckbox) {
            selectAllCheckbox.addEventListener('change', () => this.toggleSelectAll());
        }

        // Individual item checkboxes
        itemCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', () => this.handleItemCheckboxChange());
        });

        // Bulk action buttons
        if (bulkDeleteBtn) {
            bulkDeleteBtn.addEventListener('click', () => this.handleBulkDelete());
        }

        if (bulkStatusBtn) {
            bulkStatusBtn.addEventListener('click', () => this.handleBulkStatusUpdate());
        }

        // Initial toolbar state
        this.updateToolbar();
    }

    getSelectedIds() {
        return Array.from(document.querySelectorAll(this.options.itemCheckboxSelector + ':checked'))
            .map(checkbox => checkbox.value);
    }

    toggleSelectAll() {
        const selectAllCheckbox = document.querySelector(this.options.selectAllSelector);
        const itemCheckboxes = document.querySelectorAll(this.options.itemCheckboxSelector);

        itemCheckboxes.forEach(checkbox => {
            checkbox.checked = selectAllCheckbox.checked;
        });

        this.updateToolbar();
    }

    handleItemCheckboxChange() {
        const selectAllCheckbox = document.querySelector(this.options.selectAllSelector);
        const itemCheckboxes = document.querySelectorAll(this.options.itemCheckboxSelector);
        const allChecked = Array.from(itemCheckboxes).every(cb => cb.checked);
        const someChecked = Array.from(itemCheckboxes).some(cb => cb.checked);

        selectAllCheckbox.checked = allChecked;
        selectAllCheckbox.indeterminate = someChecked && !allChecked;

        this.updateToolbar();
    }

    updateToolbar() {
        const toolbar = document.querySelector(this.options.bulkToolbarSelector);
        if (!toolbar) return;

        const selectedCount = this.getSelectedIds().length;
        const countElement = toolbar.querySelector(this.options.selectedCountSelector);

        if (selectedCount > 0) {
            toolbar.classList.remove('hidden');
            toolbar.classList.add('visible');

            if (countElement) {
                countElement.textContent = selectedCount;
            }
        } else {
            toolbar.classList.remove('visible');
            toolbar.classList.add('hidden');
        }
    }

    handleBulkDelete() {
        const selectedIds = this.getSelectedIds();

        if (selectedIds.length === 0) {
            alert('Please select items to delete');
            return;
        }

        const message = selectedIds.length === 1
            ? `Are you sure you want to delete this ${this.options.entityType}?`
            : `Are you sure you want to delete ${selectedIds.length} ${this.options.entityType}?`;

        if (!confirm(message)) {
            return;
        }

        this.submitBulkAction('bulk-delete', selectedIds);
    }

    handleBulkStatusUpdate() {
        const selectedIds = this.getSelectedIds();

        if (selectedIds.length === 0) {
            alert('Please select items to update');
            return;
        }

        // Get available statuses from data attribute or hardcoded
        const statusSelect = document.querySelector('.bulk-status-select');
        if (!statusSelect) {
            alert('Status selector not available');
            return;
        }

        const newStatus = statusSelect.value;
        if (!newStatus) {
            alert('Please select a status');
            return;
        }

        const message = `Are you sure you want to update status for ${selectedIds.length} ${this.options.entityType}?`;

        if (!confirm(message)) {
            return;
        }

        this.submitBulkAction('bulk-status-update', selectedIds, { status: newStatus });
    }

    submitBulkAction(action, ids, additionalData = {}) {
        const form = document.createElement('form');
        form.method = 'POST';
        form.style.display = 'none';

        // Add action field
        const actionInput = document.createElement('input');
        actionInput.type = 'hidden';
        actionInput.name = 'bulkAction';
        actionInput.value = action;
        form.appendChild(actionInput);

        // Add IDs
        ids.forEach(id => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'selectedIds';
            input.value = id;
            form.appendChild(input);
        });

        // Add additional data
        Object.keys(additionalData).forEach(key => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = additionalData[key];
            form.appendChild(input);
        });

        document.body.appendChild(form);
        form.submit();
    }
}

// Auto-initialize bulk operations on page load
document.addEventListener('DOMContentLoaded', () => {
    const toolbar = document.querySelector('.bulk-toolbar');
    if (toolbar) {
        const entityType = toolbar.dataset.entityType || 'items';
        window.bulkOpsManager = new BulkOperationsManager({ entityType });
    }
});
