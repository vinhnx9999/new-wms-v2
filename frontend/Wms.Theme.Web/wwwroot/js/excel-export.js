/**
 * Excel Export Utility for DataTable Components
 * Requires: SheetJS (xlsx) library
 * Usage: exportToExcel(options)
 */

/**
 * Export table data to Excel file
 * @param {Object} options - Export configuration options
 * @param {string} options.tableSelector - CSS selector for the table (default: 'table')
 * @param {string} options.filename - Excel filename without extension (default: 'export')
 * @param {string} options.sheetName - Excel sheet name (default: 'Sheet1')
 * @param {boolean} options.includeHidden - Include hidden columns (default: false)
 * @param {Array<string>} options.excludeColumns - Column names to exclude from export
 * @param {Array<string>} options.includeOnlyColumns - Only include these columns (if specified)
 * @param {boolean} options.selectedRowsOnly - Export only selected rows (default: false)
 * @param {function} options.onSuccess - Callback function on successful export
 * @param {function} options.onError - Callback function on error
 */
function exportToExcel(options = {}) {
    try {
        // Default options
        const config = {
            tableSelector: 'table',
            filename: 'export',
            sheetName: 'Sheet1',
            includeHidden: false,
            excludeColumns: ['Select', 'Actions'],
            includeOnlyColumns: null,
            selectedRowsOnly: false,
            onSuccess: null,
            onError: null,
            ...options
        };

        // Check if SheetJS library is loaded
        if (typeof XLSX === 'undefined') {
            throw new Error('SheetJS (XLSX) library is not loaded. Please include the library.');
        }

        // Find the table
        const table = document.querySelector(config.tableSelector);
        if (!table) {
            throw new Error(`Table not found with selector: ${config.tableSelector}`);
        }

        // Extract table data
        const data = extractTableData(table, config);
        
        if (data.length === 0) {
            throw new Error('No data available for export');
        }

        // Create workbook and worksheet
        const workbook = XLSX.utils.book_new();
        const worksheet = XLSX.utils.json_to_sheet(data);

        // Auto-size columns
        const columnWidths = calculateColumnWidths(data);
        worksheet['!cols'] = columnWidths;

        // Add worksheet to workbook
        XLSX.utils.book_append_sheet(workbook, worksheet, config.sheetName);

        // Generate filename with timestamp
        const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
        const filename = `${config.filename}_${timestamp}.xlsx`;

        // Download file
        XLSX.writeFile(workbook, filename);

        // Success callback
        if (config.onSuccess && typeof config.onSuccess === 'function') {
            config.onSuccess(filename, data.length);
        }

        console.log(`Excel export completed: ${filename} (${data.length} rows)`);

    } catch (error) {
        console.error('Excel export error:', error);
        
        // Error callback
        if (config.onError && typeof config.onError === 'function') {
            config.onError(error);
        } else {
            alert(`Export failed: ${error.message}`);
        }
    }
}

/**
 * Extract data from table based on configuration
 */
function extractTableData(table, config) {
    const rows = [];
    const thead = table.querySelector('thead');
    const tbody = table.querySelector('tbody');

    if (!thead || !tbody) {
        throw new Error('Table must have thead and tbody elements');
    }

    // Get header information
    const headerRow = thead.querySelector('tr');
    const headerCells = Array.from(headerRow.querySelectorAll('th'));
    
    // Build column mapping
    const columnMapping = buildColumnMapping(headerCells, config);
    
    if (columnMapping.length === 0) {
        throw new Error('No valid columns found for export');
    }

    // Extract data rows
    const dataRows = Array.from(tbody.querySelectorAll('tr'));
    
    dataRows.forEach(row => {
        // Skip if row should be filtered out
        if (config.selectedRowsOnly && !isRowSelected(row)) {
            return;
        }

        // Skip empty rows or "no data" rows
        if (isEmptyRow(row)) {
            return;
        }

        const rowData = {};
        const cells = Array.from(row.querySelectorAll('td'));

        columnMapping.forEach(col => {
            if (cells[col.index]) {
                rowData[col.name] = extractCellValue(cells[col.index]);
            }
        });

        rows.push(rowData);
    });

    return rows;
}

/**
 * Build column mapping based on configuration
 */
function buildColumnMapping(headerCells, config) {
    const mapping = [];

    headerCells.forEach((cell, index) => {
        const columnName = cell.textContent.trim();
        
        // Skip if column should be excluded
        if (config.excludeColumns.includes(columnName)) {
            return;
        }

        // If includeOnlyColumns is specified, only include those
        if (config.includeOnlyColumns && !config.includeOnlyColumns.includes(columnName)) {
            return;
        }

        // Skip hidden columns unless specifically included
        if (!config.includeHidden && isColumnHidden(cell)) {
            return;
        }

        mapping.push({
            name: columnName,
            index: index
        });
    });

    return mapping;
}

/**
 * Extract clean value from table cell
 */
function extractCellValue(cell) {
    // Check for specific data patterns
    
    // Status badges - extract text content
    const statusBadge = cell.querySelector('.rounded-full');
    if (statusBadge) {
        return statusBadge.textContent.trim();
    }

    // Date values - extract from datetime attributes or text
    const timeElement = cell.querySelector('time');
    if (timeElement) {
        return timeElement.getAttribute('datetime') || timeElement.textContent.trim();
    }

    // Checkboxes - extract checked state
    const checkbox = cell.querySelector('input[type="checkbox"]');
    if (checkbox) {
        return checkbox.checked ? 'Yes' : 'No';
    }

    // Links - extract text content
    const link = cell.querySelector('a');
    if (link) {
        return link.textContent.trim();
    }

    // Default - clean text content
    let value = cell.textContent.trim();
    
    // Remove extra whitespace
    value = value.replace(/\s+/g, ' ');
    
    // Convert common display values
    if (value === '-' || value === '—' || value === '') {
        return '';
    }

    // Try to parse numbers
    const numValue = parseFloat(value.replace(/[^\d.-]/g, ''));
    if (!isNaN(numValue) && value.match(/^\d+([.,]\d+)?$/)) {
        return numValue;
    }

    return value;
}

/**
 * Check if a table row is selected
 */
function isRowSelected(row) {
    const checkbox = row.querySelector('.row-checkbox, .item-checkbox, input[type="checkbox"]');
    return checkbox ? checkbox.checked : false;
}

/**
 * Check if a table row is empty or "no data" row
 */
function isEmptyRow(row) {
    const text = row.textContent.trim().toLowerCase();
    return text.includes('no data found') || 
           text.includes('no results') || 
           text.includes('loading') ||
           row.cells.length <= 1;
}

/**
 * Check if a column is hidden
 */
function isColumnHidden(headerCell) {
    return headerCell.style.display === 'none' || 
           headerCell.classList.contains('hidden') ||
           getComputedStyle(headerCell).display === 'none';
}

/**
 * Calculate optimal column widths for Excel
 */
function calculateColumnWidths(data) {
    if (data.length === 0) return [];

    const widths = [];
    const sampleSize = Math.min(data.length, 100); // Sample for performance

    Object.keys(data[0]).forEach(key => {
        let maxLength = key.length; // Header length

        // Check sample data for max length
        for (let i = 0; i < sampleSize; i++) {
            const value = String(data[i][key] || '');
            maxLength = Math.max(maxLength, value.length);
        }

        // Set reasonable bounds
        const width = Math.min(Math.max(maxLength + 2, 10), 50);
        widths.push({ wch: width });
    });

    return widths;
}

/**
 * Quick export function for common use cases
 */
function quickExportTable(filename = 'export', selectedOnly = false) {
    exportToExcel({
        filename: filename,
        selectedRowsOnly: selectedOnly,
        onSuccess: (filename, rowCount) => {
            const message = selectedOnly 
                ? `Exported ${rowCount} selected rows to ${filename}`
                : `Exported ${rowCount} rows to ${filename}`;
            
            // Show success notification if available
            if (typeof showNotification === 'function') {
                showNotification(message, 'success');
            } else {
                console.log(message);
            }
        }
    });
}

/**
 * Export only selected rows
 */
function exportSelectedRows(filename = 'selected_export') {
    const selectedCount = document.querySelectorAll('.row-checkbox:checked, .item-checkbox:checked').length;
    
    if (selectedCount === 0) {
        alert('Please select at least one row to export.');
        return;
    }

    quickExportTable(filename, true);
}

/**
 * Export all visible rows
 */
function exportAllRows(filename = 'all_export') {
    quickExportTable(filename, false);
}

// Make functions available globally
window.exportToExcel = exportToExcel;
window.quickExportTable = quickExportTable;
window.exportSelectedRows = exportSelectedRows;
window.exportAllRows = exportAllRows;