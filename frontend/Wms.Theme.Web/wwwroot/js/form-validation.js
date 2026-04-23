/**
 * Form Validation Module
 * Real-time form validation with inline error messages
 */

class FormValidator {
    constructor(formSelector) {
        this.form = document.querySelector(formSelector);
        if (!this.form) {
            console.warn(`Form not found: ${formSelector}`);
            return;
        }
        this.initializeValidation();
    }

    initializeValidation() {
        // Add event listeners to all form inputs
        const inputs = this.form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', (e) => this.validateField(e.target));
            input.addEventListener('change', (e) => this.validateField(e.target));
        });

        // Form submit validation
        this.form.addEventListener('submit', (e) => this.validateForm(e));
    }

    validateField(field) {
        const rules = this.getValidationRules(field);
        const errors = [];

        // Check required
        if (field.hasAttribute('required') && !field.value.trim()) {
            errors.push(field.getAttribute('data-required-msg') || `${this.getFieldLabel(field)} là bắt buộc`);
        }

        // Check min length
        if (field.hasAttribute('data-minlength')) {
            const minLength = parseInt(field.getAttribute('data-minlength'));
            if (field.value && field.value.length < minLength) {
                errors.push(field.getAttribute('data-minlength-msg') || `Tối thiểu ${minLength} ký tự`);
            }
        }

        // Check max length
        if (field.hasAttribute('data-maxlength')) {
            const maxLength = parseInt(field.getAttribute('data-maxlength'));
            if (field.value && field.value.length > maxLength) {
                errors.push(field.getAttribute('data-maxlength-msg') || `Tối đa ${maxLength} ký tự`);
            }
        }

        // Check pattern (email, phone, etc.)
        if (field.hasAttribute('data-pattern')) {
            const pattern = field.getAttribute('data-pattern');
            const regex = new RegExp(pattern);
            if (field.value && !regex.test(field.value)) {
                errors.push(field.getAttribute('data-pattern-msg') || 'Định dạng không hợp lệ');
            }
        }

        // Check date rules
        if (field.type === 'date') {
            const dateError = this.validateDate(field);
            if (dateError) errors.push(dateError);
        }

        // Update UI
        this.displayErrors(field, errors);
        return errors.length === 0;
    }

    validateDate(field) {
        const value = field.value;
        if (!value) return null;

        const selectedDate = new Date(value + 'T00:00:00');
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        // Check if date cannot be future (for order dates, receipt dates)
        if (field.name.includes('OrderDate') || field.name.includes('ReceiptDate')) {
            if (selectedDate > today) {
                return 'Ngày không thể là tương lai';
            }
        }

        // Check if delivery date >= order date
        if (field.name.includes('DeliveryDate') || field.name.includes('ExpectedDeliveryDate')) {
            const orderDateField = this.form.querySelector('[name*="OrderDate"], [name*="ReceiptDate"]');
            if (orderDateField && orderDateField.value) {
                const orderDate = new Date(orderDateField.value + 'T00:00:00');
                if (selectedDate < orderDate) {
                    return 'Ngày giao phải >= ngày đặt hàng';
                }
            }
        }

        return null;
    }

    validateForm(event) {
        const inputs = this.form.querySelectorAll('input, select, textarea');
        let isValid = true;

        inputs.forEach(input => {
            if (!this.validateField(input)) {
                isValid = false;
            }
        });

        if (!isValid) {
            event.preventDefault();
            this.focusFirstError();
        }

        return isValid;
    }

    displayErrors(field, errors) {
        // Remove existing error message
        const existingError = field.parentElement.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }

        // Add border and error state
        if (errors.length > 0) {
            field.classList.add('is-invalid');

            // Create error message element
            const errorDiv = document.createElement('div');
            errorDiv.className = 'field-error';
            errorDiv.innerHTML = errors.join(', ');
            field.parentElement.appendChild(errorDiv);
        } else {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        }
    }

    getFieldLabel(field) {
        const label = document.querySelector(`label[for="${field.id}"]`);
        return label ? label.textContent.trim() : field.name;
    }

    focusFirstError() {
        const firstInvalid = this.form.querySelector('input.is-invalid, select.is-invalid, textarea.is-invalid');
        if (firstInvalid) {
            firstInvalid.focus();
            firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    getValidationRules(field) {
        return {
            required: field.hasAttribute('required'),
            minLength: field.getAttribute('data-minlength'),
            maxLength: field.getAttribute('data-maxlength'),
            pattern: field.getAttribute('data-pattern')
        };
    }
}

// Auto-initialize all forms with class 'validated-form'
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('form.validated-form').forEach(form => {
        new FormValidator(`#${form.id || ''}`);
    });
});

// Export for manual use
window.FormValidator = FormValidator;
