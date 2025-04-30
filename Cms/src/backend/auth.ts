import axios from 'axios';
import { apiUrl } from '@/backend/utils';
import Cookies from 'js-cookie';

export const checkAuth = async (token: string): Promise<boolean> => {
    try {
        const response = await axios.get(`${apiUrl}/checkCmsAuth`, {
            headers: {
                'Backend-Secret': token,
            },
        });
        if (response && response.status === 200) {
            Cookies.set('token', token);
            return true;
        }
    } catch (error) {
        console.error(error);
    }
    return false;
};
