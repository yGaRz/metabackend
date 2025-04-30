'use client';
import React, { useEffect, useState } from 'react';
import { deleteStoreItem, getStoreItems } from '@/backend/store';
import {
    Button,
    Chip,
    IconButton,
    Menu,
    MenuHandler,
    MenuItem,
    MenuList,
    Typography,
} from '@material-tailwind/react';
import { StoreProduct, StoreType } from '@/backend/dto/store.product';
import { EmptyPlace } from '@/components/PlaceHolders/EmptyPlace';
import { SkeletonLoader } from '@/components/PlaceHolders/SkeletonLoader';
import { CreateEditStoreItem } from '@/components/Modals/CreateEditStoreItem';
import Swal from 'sweetalert2';

export const StoreWrapper = () => {
    const [type, setType] = useState<StoreType>(StoreType.Clothes);
    const [loading, setLoading] = useState(false);
    const [storeItems, setStoreItems] = useState<StoreProduct[]>([]);
    const [showCreate, setShowCreate] = useState(false);
    const [showEdit, setShowEdit] = useState(false);
    const [selectedItem, setSelectedItem] = useState<StoreProduct | undefined>(undefined);

    useEffect(() => {
        fetchByType(type);
    }, [type]);

    const fetchByType = (storeType: StoreType) => {
        setLoading(true);
        getStoreItems(storeType)
            .then((r) => {
                if (r && Array.isArray(r)) {
                    setStoreItems(r);
                }
            })
            .finally(() => setLoading(false));
    };

    const getBadgeForStoreType = (storeType: StoreType) => {
        switch (storeType) {
            case StoreType.Clothes:
                return <Chip color="indigo" className="inline-block" value={storeType} />;
            case StoreType.Emotion:
                return <Chip color="purple" className="inline-block" value={storeType} />;
            case StoreType.Customization:
                return <Chip color="teal" className="inline-block" value={storeType} />;
        }
    };

    const TABLE_HEAD = ['Id', 'UnrealId', 'StoreType', 'Price', 'Created', 'Actions'];

    const deleteConfirm = async (internalId: string) => {
        const result = await Swal.fire({
            text: 'Do you want to delete the store item?',
            showDenyButton: false,
            showCancelButton: true,
            confirmButtonColor: '#ed2323',
            confirmButtonText: 'Yes, delete it!',
            cancelButtonColor: '#5a5a5a',
            cancelButtonText: 'Cancel',
            icon: 'question',
        });

        if (result.isConfirmed) {
            const response = await deleteStoreItem(internalId);
            if (response) {
                await Swal.fire({
                    text: 'Store item successfully deleted!',
                    icon: 'success',
                    timer: 2000,
                    confirmButtonColor: '#5750F1',
                    confirmButtonText: 'OK',
                });
                fetchByType(type);
            }
        }
    };

    return (
        <div className="rounded-[10px] border border-stroke bg-white p-4 shadow-1 dark:border-dark-3 dark:bg-gray-dark dark:shadow-card sm:p-7.5">
            <div style={{ display: 'flex', justifyContent: 'space-between' }} className="mb-6">
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <label htmlFor="storeType">Store Type:</label>
                    <select
                        id="storeType"
                        value={type}
                        onChange={(e) => setType(e.target.value as StoreType)}
                    >
                        {Object.values(StoreType).map((type) => (
                            <option key={type} value={type}>
                                {type}
                            </option>
                        ))}
                    </select>
                </div>
                <div>
                    <Button onClick={() => setShowCreate(true)} placeholder="" color="green">
                        Create store item
                    </Button>
                </div>
            </div>

            {!loading && storeItems.length > 0 && (
                <table className="w-full min-w-max table-auto text-left">
                    <thead>
                        <tr>
                            {TABLE_HEAD.map((head) => (
                                <th key={head} className="border-b border-blue-gray-100 bg-blue-gray-50 p-4">
                                    <Typography
                                        variant="small"
                                        color="blue-gray"
                                        className="font-normal leading-none opacity-70"
                                        placeholder=""
                                    >
                                        {head}
                                    </Typography>
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {storeItems.map((item, index) => {
                            const isLast = index === storeItems.length - 1;
                            const classes = isLast ? 'p-4' : 'p-4 border-b border-blue-gray-50';

                            return (
                                <tr key={item.internalId}>
                                    <td className={classes}>
                                        <Typography
                                            placeholder=""
                                            variant="small"
                                            color="blue-gray"
                                            className="font-normal"
                                        >
                                            {item.internalId}
                                        </Typography>
                                    </td>
                                    <td className={classes}>
                                        <Typography
                                            placeholder=""
                                            variant="small"
                                            color="blue-gray"
                                            className="font-normal"
                                        >
                                            {item.unrealId}
                                        </Typography>
                                    </td>
                                    <td className={classes}>
                                        <Typography
                                            placeholder=""
                                            variant="small"
                                            color="blue-gray"
                                            className="font-normal"
                                        >
                                            {getBadgeForStoreType(item.storeType)}
                                        </Typography>
                                    </td>
                                    <td className={classes}>
                                        <Typography
                                            placeholder=""
                                            as="a"
                                            href="#"
                                            variant="small"
                                            color="blue-gray"
                                            className="font-medium"
                                        >
                                            {item.price}
                                        </Typography>
                                    </td>
                                    <td className={classes}>
                                        <Typography
                                            placeholder=""
                                            variant="small"
                                            color="blue-gray"
                                            className="font-medium"
                                        >
                                            {new Date(item.created).toLocaleString()}
                                        </Typography>
                                    </td>
                                    <td className={classes}>
                                        <Menu
                                            animate={{
                                                mount: { y: 0 },
                                                unmount: { y: 25 },
                                            }}
                                        >
                                            <MenuHandler>
                                                <IconButton variant="text">
                                                    <span className="font-semibold">...</span>
                                                </IconButton>
                                            </MenuHandler>
                                            <MenuList>
                                                <MenuItem
                                                    onClick={() => {
                                                        setSelectedItem(item);
                                                        setShowEdit(true);
                                                    }}
                                                >
                                                    <span>Edit item</span>
                                                </MenuItem>
                                                <MenuItem onClick={() => deleteConfirm(item.internalId)}>
                                                    <Typography color="red">Delete item</Typography>
                                                </MenuItem>
                                            </MenuList>
                                        </Menu>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            )}
            {loading && <SkeletonLoader />}
            {!loading && storeItems.length === 0 && (
                <EmptyPlace emptyText={'No items available in the store. Please create'} />
            )}
            {showCreate && (
                <CreateEditStoreItem
                    isOpen={showCreate}
                    onClose={() => setShowCreate(false)}
                    storeCallback={(type) => {
                        fetchByType(type);
                        setShowCreate(false);
                    }}
                />
            )}
            {showEdit && selectedItem && (
                <CreateEditStoreItem
                    isOpen={showEdit}
                    onClose={() => setShowEdit(false)}
                    storeCallback={(type) => {
                        fetchByType(type);
                        setShowEdit(false);
                    }}
                    storeItem={selectedItem}
                />
            )}
        </div>
    );
};
