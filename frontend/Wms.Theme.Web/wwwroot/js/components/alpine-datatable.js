function AlpineDataTable(config) {
    return {
        apiUrl: config.apiUrl || '',
        columns: config.columns || [],
        options: {
            pageSize: 10,
            selection: true,
            ...config.options
        },

        // State
        isLoading: false,
        rows: [],
        pagination: {
            pageIndex: 1,
            pageSize: 10,
            total: 0,
            totalPages: 1
        },
        filters: config.filters || {},
        selectedRows: [],
        allSelected: false,

        init() {
            console.log('AlpineDataTable initialized');
            this.pagination.pageSize = this.options.pageSize;
            // Load initial data if apiUrl is provided
            if (this.apiUrl) {
                this.fetchData();
            }


            // Watchers
            this.$watch('allSelected', value => {
                // Skip if this change was triggered by updateSelection() to avoid loop
                if (this._updatingFromRowSelection) return;

                this.rows.forEach(row => row.selected = value);
                this.updateSelection();
            });
        },

        // Data Fetching
        async fetchData(pageIndex = 1) {
            this.isLoading = true;
            this.allSelected = false; // Reset select all on page change

            try {
                // Construct URL with params
                const params = new URLSearchParams({
                    pageIndex: pageIndex,
                    pageSize: this.pagination.pageSize,
                    ...this.filters
                });

                const response = await fetch(`${this.apiUrl}&${params.toString()}`);
                if (!response.ok) throw new Error('Network response was not ok');

                const json = await response.json();

                if (json.status) {
                    this.rows = (json.data.rows ?? []).map(row => ({
                        ...row,
                        selected: false
                    }));
                    this.pagination = {
                        pageIndex: json.data.pageIndex,
                        pageSize: this.pagination.pageSize,
                        total: json.data.total,
                        totalPages: json.data.totalPages
                    };
                } else {
                    this.rows = [];
                    this.pagination.total = 0;
                }

            } catch (error) {
                console.error('Error fetching data:', error);
                this.rows = [];
            } finally {
                // Minimum loading time for smooth UX
                setTimeout(() => {
                    this.isLoading = false;
                }, 300);
            }
        },

        // Helpers
        getItemValue(item, field) {
            if (!field) return '';
            return field.split('.').reduce((obj, key) => obj && obj[key], item) || '';
        },

        // Render Helpers (for use in x-html if needed, though mostly handled in View)
        formatDate(dateStr) {
            if (!dateStr || dateStr.startsWith('0001')) return '-';
            return new Date(dateStr).toLocaleDateString('vi-VN');
        },

        formatNumber(num) {
            return num ? num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") : "0";
        },

        // Pagination
        nextPage() {
            if (this.pagination.pageIndex < this.pagination.totalPages) {
                this.fetchData(this.pagination.pageIndex + 1);
            }
        },

        prevPage() {
            if (this.pagination.pageIndex > 1) {
                this.fetchData(this.pagination.pageIndex - 1);
            }
        },

        getRowStart() {
            return (this.pagination.pageIndex - 1) * this.pagination.pageSize + 1;
        },

        getRowEnd() {
            return Math.min(this.pagination.pageIndex * this.pagination.pageSize, this.pagination.total);
        },

        // Selection
        toggleSelectAll() {
            this.allSelected = !this.allSelected;
        },

        updateSelection() {
            this.selectedRows = JSON.parse(JSON.stringify(this.rows.filter(r => r.selected)));

            // Auto-check "Select All" when all rows are manually selected
            // Auto-uncheck when any row is deselected
            if (this.rows.length > 0) {
                const allRowsSelected = this.rows.every(r => r.selected);

                // Only update if state actually changed to avoid infinite loop with $watch
                if (this.allSelected !== allRowsSelected) {
                    // Temporarily disable the watcher effect by checking inside watcher
                    this._updatingFromRowSelection = true;
                    this.allSelected = allRowsSelected;

                    // Use setTimeout to ensure the watcher (which runs synchronously or as a microtask)
                    // sees the flag as true. Resetting immediately caused the watcher to see false.
                    setTimeout(() => {
                        this._updatingFromRowSelection = false;
                    }, 0);
                }
            }
        },


        // Safe Render Helper (Called from View)
        safeRender(col, item) {
            if (!item) {
                // Defensive check: item should not be null/undefined here
                return '';
            }
            if (!col || typeof col.render !== 'function') return '';
            try {
                return col.render(item);
            } catch (e) {
                console.warn('Render error for column:', col.title, e, item);
                return '';
            }
        },

        // External Methods (can be called from parent)
        reload() {
            this.fetchData(1); // Reload to page 1
        },

        refresh() {
            this.fetchData(this.pagination.pageIndex); // Refresh current page
        },

        setFilter(key, value) {
            this.filters[key] = value;
            this.reload();
        },

        updateConfig(newConfig) {
            if (!newConfig) return;
            this.apiUrl = newConfig.apiUrl || '';
            this.columns = newConfig.columns || [];
            this.options = { ...this.options, ...newConfig.options };

            // Reset State
            this.pagination.pageIndex = 1;
            this.allSelected = false;
            this.selectedRows = [];
            this.filters = newConfig.filters || {};

            if (this.apiUrl) {
                this.fetchData(1);
            } else {
                this.rows = [];
            }
        }
    }
}
