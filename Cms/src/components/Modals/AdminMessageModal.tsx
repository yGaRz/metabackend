import React, { useState } from 'react';
import { ModalProps } from '@/types/modal';
import { Button, Card, CardBody, CardFooter, Dialog, Textarea, Typography } from '@material-tailwind/react';
import Swal from 'sweetalert2';
import { sendAdminMessage } from '@/backend/chat';

export const AdminMessageModal = ({ isOpen, onClose }: ModalProps) => {
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');

    const onSendMessage = async () => {
        if (!message) {
            return await Swal.fire({
                text: `Please type message!`,
                icon: 'error',
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
        }

        setLoading(true);
        const sendResult = await sendAdminMessage(message);
        if (sendResult) {
            await Swal.fire({
                text: 'Message successfully sent!',
                icon: 'success',
                timer: 2000,
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
            onClose();
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
                            Admin message
                        </Typography>
                        <Textarea
                            label="Message"
                            value={message}
                            onChange={(e) => setMessage(e.target.value)}
                        />
                    </CardBody>
                    <CardFooter className="pt-0">
                        <Button
                            loading={loading}
                            className="mb-3"
                            color="green"
                            onClick={onSendMessage}
                            fullWidth
                        >
                            Send message
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
