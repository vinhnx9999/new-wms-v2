/**
 * Global Helper Functions for DataTable Components
 */

/**
 * Updates the header checkbox state based on the row checkboxes.
 */
function updateSelectAllCheckbox() {
    const selectAllCheckbox = document.getElementById('selectAll');
    const checkboxes = document.querySelectorAll('.row-checkbox');
    const checkedCount = document.querySelectorAll('.row-checkbox:checked').length;

    if (selectAllCheckbox) {
        selectAllCheckbox.checked = checkboxes.length > 0 && checkedCount === checkboxes.length;
        selectAllCheckbox.indeterminate = checkedCount > 0 && checkedCount < checkboxes.length;
    }
}

/**
 * Toggles all row checkboxes when the header checkbox is clicked.
 * @param {HTMLInputElement} selectAllCheckbox - The header checkbox element.
 */
function toggleAllCheckboxes(selectAllCheckbox) {
    const checkboxes = document.querySelectorAll('.row-checkbox');
    checkboxes.forEach(checkbox => { checkbox.checked = selectAllCheckbox.checked; });
    updateSelectAllCheckbox();
}

// Ensure functions are available globally
window.updateSelectAllCheckbox = updateSelectAllCheckbox;
window.toggleAllCheckboxes = toggleAllCheckboxes;
