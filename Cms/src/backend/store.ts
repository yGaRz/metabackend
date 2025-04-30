import axios from 'axios';
import { apiUrl, getHeaderOption, handleError } from '@/backend/utils';
import { StoreProduct, StoreType } from '@/backend/dto/store.product';
import { StoreCreateDto, StoreCreateResponse, StoreEditDto } from '@/backend/dto/store.create.dto';

export const getStoreItems = async (storeType: StoreType): Promise<StoreProduct[]> => {
    try {
        const response = await axios.get<StoreProduct[]>(
            `${apiUrl}/getStoreItems?storeType=${storeType}`,
            getHeaderOption()
        );
        return response.data;
    } catch (e) {
        handleError(e);
        return [];
    }
};

export const createStoreItem = async (dto: StoreCreateDto): Promise<boolean> => {
    try {
        const response = await axios.post<StoreCreateResponse>(
            `${apiUrl}/createStoreItem`,
            dto,
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

export const editStoreItem = async (dto: StoreEditDto): Promise<boolean> => {
    try {
        const response = await axios.put<StoreCreateResponse>(
            `${apiUrl}/updateStoreItem`,
            dto,
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

export const deleteStoreItem = async (id: string): Promise<boolean> => {
    try {
        const response = await axios.delete(`${apiUrl}/deleteStoreItem?internalId=${id}`, getHeaderOption());
        if (response) {
            return true;
        }
    } catch (e) {
        handleError(e);
    }
    return false;
};
