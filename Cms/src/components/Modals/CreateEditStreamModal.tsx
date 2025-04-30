import React, { useState } from 'react';
import { ModalProps } from '@/types/modal';
import { StreamEntity } from '@/backend/dto/stream.dto';
import { Button, Card, CardBody, CardFooter, Dialog, Input, Typography } from '@material-tailwind/react';
import { StoreType } from '@/backend/dto/store.product';
import Swal from 'sweetalert2';
import { createStream, editStream } from '@/backend/stream';

type EditStreamModalProps = ModalProps & {
    stream?: StreamEntity;
};

export const CreateEditStreamModal = ({ isOpen, onClose, callback, stream }: EditStreamModalProps) => {
    const [loading, setLoading] = useState(false);
    const [url, setUrl] = useState(stream?.url || '');

    const onAction = async () => {
        const regex = /^(https?|ftp):\/\/[^\s/$.?#].[^\s]*$/i;
        const isValidUrl = regex.test(url);
        if (!isValidUrl) {
            return await Swal.fire({
                text: `Url is not valid!`,
                icon: 'error',
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
        }

        setLoading(true);
        if (stream === undefined) {
            const response = await createStream(url);
            if (response) {
                await Swal.fire({
                    text: 'Stream successfully created!',
                    icon: 'success',
                    timer: 2000,
                    confirmButtonColor: '#5750F1',
                    confirmButtonText: 'OK',
                });
                if (callback) {
                    callback();
                }
            }
        } else {
            const response = await editStream(stream.streamId, url);
            if (response) {
                await Swal.fire({
                    text: 'Stream successfully created!',
                    icon: 'success',
                    timer: 2000,
                    confirmButtonColor: '#5750F1',
                    confirmButtonText: 'OK',
                });
                if (callback) {
                    callback();
                }
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
                            {stream === undefined ? 'Create stream' : 'Edit stream'}
                        </Typography>
                        {stream && (
                            <Typography variant="small" className="mb-2" color="blue-gray">
                                id: {stream.streamId}
                            </Typography>
                        )}
                        <Input
                            value={url}
                            onChange={(e) => setUrl(e.target.value)}
                            label="Stream url"
                            placeholder="https://stream.url"
                            size="lg"
                            crossOrigin=""
                        />
                    </CardBody>
                    <CardFooter className="pt-0">
                        <Button
                            loading={loading}
                            className="mb-3"
                            color={stream === undefined ? 'green' : 'blue'}
                            onClick={onAction}
                            fullWidth
                        >
                            {stream === undefined ? 'Create item' : 'Edit item'}
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
