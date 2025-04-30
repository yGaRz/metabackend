import axios from 'axios';
import { apiUrl, getHeaderOption, handleError } from '@/backend/utils';
import { StreamPaginatedResponse } from '@/backend/dto/stream.dto';

export const getStreams = async (pageNumber = 0, pageSize = 20) => {
    try {
        const response = await axios.get<StreamPaginatedResponse>(
            `${apiUrl}/getStreams?offset=${pageNumber}&count=${pageSize}`,
            getHeaderOption()
        );
        return response.data;
    } catch (e) {
        handleError(e);
    }
};

export const createStream = async (url: string): Promise<boolean> => {
    try {
        const response = await axios.post(
            `${apiUrl}/createStream`,
            {
                url,
                startTime: new Date(), // TODO: change after Vetluzhkih Dima logic
            },
            getHeaderOption()
        );
        if (response) {
            return true;
        }
    } catch (e) {
        handleError(e);
    }
    return false;
};

export const editStream = async (id: string, url: string): Promise<boolean> => {
    try {
        const response = await axios.put(
            `${apiUrl}/updateStream`,
            {
                streamId: id,
                url,
                startTime: new Date(), // TODO: change after Vetluzhkih Dima logic
            },
            getHeaderOption()
        );
        if (response) {
            return true;
        }
    } catch (e) {
        handleError(e);
    }
    return false;
};

export const deleteStream = async (id: string): Promise<boolean> => {
    try {
        const response = await axios.delete(`${apiUrl}/deleteStream/${id}`, getHeaderOption());
        if (response) return true;
    } catch (e) {
        handleError(e);
    }
    return false;
};
