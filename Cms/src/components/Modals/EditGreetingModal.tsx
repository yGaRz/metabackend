import React, { useState } from 'react';
import { Button, Card, CardBody, CardFooter, Dialog, Textarea, Typography } from '@material-tailwind/react';
import { ModalProps } from '@/types/modal';
import { ChatMessageResponse } from '@/backend/dto/chat.dto';
import Swal from 'sweetalert2';
import { updateGreetingMessage } from '@/backend/chat';

type EditGreetingModalProps = ModalProps & {
    greetingObject: ChatMessageResponse;
};

export const EditGreetingModal = ({ isOpen, onClose, greetingObject, callback }: EditGreetingModalProps) => {
    const [loading, setLoading] = useState(false);
    const [currentMessage, setCurrentMessage] = useState(greetingObject.text);

    const onUpdateMessage = async () => {
        if (!currentMessage) {
            return await Swal.fire({
                text: `Please type message!`,
                icon: 'error',
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
        }

        setLoading(true);
        const updateResult = await updateGreetingMessage(currentMessage);
        if (updateResult) {
            await Swal.fire({
                text: 'Greeting message successfully updated!',
                icon: 'success',
                timer: 2000,
                confirmButtonColor: '#5750F1',
                confirmButtonText: 'OK',
            });
            if (callback) {
                callback();
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
                            Greeting message
                        </Typography>
                        <Textarea
                            label="Message"
                            value={currentMessage}
                            onChange={(e) => setCurrentMessage(e.target.value)}
                        />
                    </CardBody>
                    <CardFooter className="pt-0">
                        <Button
                            loading={loading}
                            className="mb-3"
                            color="indigo"
                            onClick={onUpdateMessage}
                            fullWidth
                        >
                            Update message
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
