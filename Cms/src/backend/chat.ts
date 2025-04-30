import axios from 'axios';
import { apiUrl, getHeaderOption, handleError } from '@/backend/utils';
import { ChatMessageResponse } from '@/backend/dto/chat.dto';

export const getGreetingMessage = async (): Promise<ChatMessageResponse | undefined> => {
    try {
        const response = await axios.get<ChatMessageResponse>(
            `${apiUrl}/getGreetingMessage`,
            getHeaderOption()
        );
        return response.data;
    } catch (e) {
        handleError(e);
    }
};

export const sendAdminMessage = async (message: string): Promise<boolean> => {
    try {
        const response = await axios.post(`${apiUrl}/sendAdminMessage`, JSON.stringify(message), {
            headers: {
                'Content-Type': 'application/json',
                ...getHeaderOption().headers,
            },
        });
        if (response) {
            return true;
        }
    } catch (e) {
        handleError(e);
    }
    return false;
};

export const updateGreetingMessage = async (message: string): Promise<boolean> => {
    try {
        const response = await axios.put(`${apiUrl}/updateGreetingMessage`, JSON.stringify(message), {
            headers: {
                'Content-Type': 'application/json',
                ...getHeaderOption().headers,
            },
        });
        if (response) return true;
    } catch (e) {
        handleError(e);
    }
    return false;
};
