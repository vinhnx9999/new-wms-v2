/**
 * Formats a date string or object into 'dd/mm/yyyy' format.
 * Usage: GlobalFormatDate('2023-01-01') -> "01/01/2023"
 * @param {string|Date} dateInput - The date to format.
 * @returns {string} - The formatted date string.
 */
function GlobalFormatDate(dateInput) {
    if (!dateInput) return '';
    const date = new Date(dateInput);
    if (isNaN(date.getTime())) return '';

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
}

function FormatDMY(dateStr) {
    if (!dateStr) return null;
    const parts = dateStr.split('/');
    if (parts.length !== 3) return null;

    const [dd, mm, yyyy] = parts.map(Number);
    if (!dd || !mm || !yyyy) return null;

    // Construct the date in UTC to avoid timezone shifts
    const dt = new Date(Date.UTC(yyyy, mm - 1, dd));
    if (Number.isNaN(dt.getTime())) return null;

    // Validate against input to catch invalid dates like 31/02/2024
    if (dt.getUTCDate() !== dd || dt.getUTCMonth() + 1 !== mm || dt.getUTCFullYear() !== yyyy) {
        return null;
    }

    return dt.toISOString();
}

// Attach to window object to ensure global availability
window.GlobalFormatDate = GlobalFormatDate;
window.FormatDMY = FormatDMY;
