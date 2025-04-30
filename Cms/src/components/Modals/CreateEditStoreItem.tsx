import React, { useState } from 'react';
import { ModalProps } from '@/types/modal';
import { StoreProduct, StoreType } from '@/backend/dto/store.product';
import { Button, Card, CardBody, CardFooter, Dialog, Input, Typography } from '@material-tailwind/react';
import Swal from 'sweetalert2';
import { createStoreItem, editStoreItem } from '@/backend/store';

type CreateStoreItemProps = ModalProps & {
    storeCallback: (type: StoreType) => void;
    storeItem?: StoreProduct;
};

export const CreateEditStoreItem = ({ isOpen, onClose, storeCallback, storeItem }: CreateStoreItemProps) => {
    const [loading, setLoading] = useState(false);
    const [unrealId, setUnrealId] = useState(storeItem ? storeItem.unrealId : '');
    const [selectedType, setSelectedType] = useState<StoreType | undefined>(undefined);
    const [price, setPrice] = useState<number>(storeItem ? storeItem.price : 0);

    const onAction = async () => {
        if (!unrealId) {
            return await Swal.fire({
                text: `Please type unrealId!`,
                icon: 'error',
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
        }

        if (!selectedType && !storeItem) {
            return await Swal.fire({
                text: `Please select Store Type!`,
                icon: 'error',
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
        }

        if (!price || price < 0) {
            return await Swal.fire({
                text: `Price need greater than 0!`,
                icon: 'error',
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
        }

        // process
        setLoading(true);
        if (storeItem === undefined) {
            const response = await createStoreItem({
                unrealId,
                storeType: selectedType!,
                price,
            });
            if (response) {
                await Swal.fire({
                    text: 'Store item successfully created!',
                    icon: 'success',
                    timer: 2000,
                    confirmButtonColor: '#5750F1',
                    confirmButtonText: 'OK',
                });
                storeCallback(selectedType!);
            }
        } else {
            const response = await editStoreItem({
                internalId: storeItem.internalId,
                unrealId,
                price,
            });
            if (response) {
                await Swal.fire({
                    text: 'Store item successfully edited!',
                    icon: 'success',
                    timer: 2000,
                    confirmButtonColor: '#5750F1',
                    confirmButtonText: 'OK',
                });
                storeCallback(storeItem.storeType);
            }
        }
        setLoading(false);
    };

    return (
        <>
            <Dialog
                size="xs"
                open={isOpen}
                handler={onClose}
                className="bg-transparent shadow-none"
                dismiss={{ enabled: false }}
                placeholder=""
            >
                <Card className="mx-auto w-full max-w-[24rem]">
                    <CardBody className="flex flex-col gap-4">
                        <Typography variant="h4" color="blue-gray">
                            {storeItem === undefined ? 'Create store item' : 'Edit store item'}
                        </Typography>
                        {storeItem && (
                            <Typography variant="small" className="mb-2" color="blue-gray">
                                id: {storeItem.internalId}
                            </Typography>
                        )}
                        <Input
                            value={unrealId}
                            onChange={(e) => setUnrealId(e.target.value)}
                            label="Unreal Id"
                            size="lg"
                            crossOrigin=""
                        />
                        {!storeItem && (
                            <select
                                id="storeType"
                                value={selectedType}
                                className="mt-2 border p-2 rounded"
                                onChange={(e) => setSelectedType(e.target.value as StoreType)}
                            >
                                <option value="">Select a type</option>
                                {Object.values(StoreType).map((type) => (
                                    <option key={type} value={type}>
                                        {type}
                                    </option>
                                ))}
                            </select>
                        )}
                        <Input
                            value={price}
                            type={'number'}
                            onChange={(e) => setPrice(Number(e.target.value))}
                            label="Price"
                            size="lg"
                            crossOrigin=""
                        />
                    </CardBody>
                    <CardFooter className="pt-0">
                        <Button
                            loading={loading}
                            className="mb-3"
                            color={storeItem === undefined ? 'green' : 'blue'}
                            onClick={onAction}
                            fullWidth
                        >
                            {storeItem === undefined ? 'Create item' : 'Edit item'}
                        </Button>
                        <Button variant="outlined" onClick={onClose} fullWidth>
                            Cancel
                        </Button>
                    </CardFooter>
                </Card>
            </Dialog>
        </>
    );
};
