import { getStoredToken } from '@/hooks/useUser';
import axios from 'axios';
import Cookies from 'js-cookie';
import Swal from 'sweetalert2';

export const apiUrl = `${process.env.NEXT_PUBLIC_API_PROTOCOL || 'https'}://${process.env.NEXT_PUBLIC_API_HOST}:${process.env.NEXT_PUBLIC_API_PORT || ''}/api/public`;

export const getHeaderOption = (token?: string) => {
    const userToken = getStoredToken();
    return {
        headers: {
            'Backend-Secret': !token ? userToken?.token : token,
        },
    };
};

export const handleError = (error: any) => {
    let errorMessage = error.message;
    if (axios.isAxiosError(error)) {
        if (error.response && error.response.status === 401) {
            Cookies.remove('token');
            document.location = '/auth/login';
        } else if (error.response) {
            if (error.response.data && typeof error.response.data === 'object') {
                if (error.response.data.message) {
                    errorMessage = error.response.data.message;
                } else if (error.response.data.title) {
                    errorMessage = error.response.data.title;
                } else if (Array.isArray(error.response.data)) {
                    errorMessage = Object.values(error.response.data[0])[0];
                }
            }
        }
    }

    console.error(error);
    Swal.fire({
        text: errorMessage,
        icon: 'error',
        confirmButtonColor: '#5750F1',
        confirmButtonText: 'OK',
    }).then();
    //toast.error(errorMessage, getToastStyle(ToastColor.danger));
};
