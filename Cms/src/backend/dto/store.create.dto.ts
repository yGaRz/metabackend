import { StoreType } from '@/backend/dto/store.product';

export interface StoreCreateDto {
    unrealId: string;
    storeType: StoreType;
    price: number;
}

export interface StoreCreateResponse {
    internalId: string;
}

export interface StoreEditDto {
    internalId: string;
    unrealId: string;
    price: number;
}
