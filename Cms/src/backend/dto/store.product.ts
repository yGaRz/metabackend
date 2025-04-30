export interface StoreProduct {
    internalId: string;
    unrealId: string;
    storeType: StoreType;
    price: number;
    created: Date;
}

export enum StoreType {
    Clothes = 'Clothes',
    Emotion = 'Emotion',
    Customization = 'Customization',
    Transport = 'Transport',
}
