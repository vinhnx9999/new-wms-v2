
const ToastMixin = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    showCloseButton: true, 
    timer: 3000,
    timerProgressBar: false, 


    customClass: {
        popup: 'flex items-center w-full max-w-xs p-4 text-gray-500 bg-white rounded-lg shadow dark:text-gray-400 dark:bg-gray-800',

        title: 'text-sm font-normal text-gray-500 ml-2',

        closeButton: 'ms-auto -mx-1.5 -my-1.5 bg-white text-gray-400 hover:text-gray-900 rounded-lg focus:ring-2 focus:ring-gray-300 p-1.5 hover:bg-gray-100 inline-flex items-center justify-center h-8 w-8 dark:text-gray-500 dark:hover:text-white dark:bg-gray-800 dark:hover:bg-gray-700',

        icon: 'text-xs'
    },
    buttonsStyling: false, 

    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer)
        toast.addEventListener('mouseleave', Swal.resumeTimer)
    }
});

const Notify = {
    success: (message) => {
        ToastMixin.fire({
            icon: 'success',
            title: message,
            iconColor: '#10B981', 
        });
    },

    error: (message) => {
        ToastMixin.fire({
            icon: 'error',
            title: message,
            iconColor: '#EF4444', 
        });
    },

    warning: (message) => {
        ToastMixin.fire({
            icon: 'warning',
            title: message,
            iconColor: '#F59E0B', // Tailwind: text-yellow-500
        });
    },

    info: (message) => {
        ToastMixin.fire({
            icon: 'info',
            title: message,
            iconColor: '#3B82F6', // Tailwind: text-blue-500
        });
    },

    confirm: async (title, text, confirmBtnText = "Đồng ý") => {
        const result = await Swal.fire({
            icon: 'question',
            title: title,
            text: text, 

            showCancelButton: true,
            confirmButtonText: confirmBtnText,
            cancelButtonText: 'Hủy bỏ',
            reverseButtons: false, 
            focusConfirm: false,

            buttonsStyling: false,

            customClass: {

                popup: 'rounded-2xl shadow-xl border border-gray-100 bg-white p-6 w-auto min-w-[400px]',
                icon: 'text-xs',
                title: 'text-xl font-bold text-gray-900 mb-2',
                htmlContainer: 'text-gray-600 mb-6',
                actions: 'flex gap-3 justify-center w-full',
                cancelButton: 'px-6 py-2 bg-white text-gray-700 font-medium border border-gray-400 rounded-full hover:bg-gray-50 focus:ring-2 focus:ring-gray-200 transition shadow-sm',
                confirmButton: 'px-6 py-2 bg-blue-600 text-white font-medium border border-transparent rounded-full hover:bg-blue-700 focus:ring-2 focus:ring-blue-300 transition shadow-sm'
            }
        });
        return result.isConfirmed;
    }
};