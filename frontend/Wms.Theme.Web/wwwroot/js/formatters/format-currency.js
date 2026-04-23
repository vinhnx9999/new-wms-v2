/**
 * Formats a number into Vietnamese Currency format (VND).
 * Usage: GlobalFormatCurrency(1000000) -> "1.000.000"
 * @param {number|string} value - The value to format.
 * @returns {string} - The formatted currency string.
 */
function GlobalFormatCurrency(value) {
    if (value === null || value === undefined || value === '') return '';
    const number = Number(value);
    if (isNaN(number)) return value;
    // Format with dots as thousands separators
    return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}

// Attach to window object to ensure global availability
window.GlobalFormatCurrency = GlobalFormatCurrency;
