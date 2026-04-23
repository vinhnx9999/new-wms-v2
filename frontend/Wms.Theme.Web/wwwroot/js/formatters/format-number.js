/**
 * Formats a number with dot separators for thousands.
 * Usage: GlobalFormatNumber(1000) -> "1.000"
 * @param {number|string} value - The value to format.
 * @returns {string} - The formatted number string.
 */
function GlobalFormatNumber(value) {
    if (value === null || value === undefined || value === '') return '';
    const number = Number(value);
    if (isNaN(number)) return value;
    return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}

// Attach to window object to ensure global availability
window.GlobalFormatNumber = GlobalFormatNumber;
