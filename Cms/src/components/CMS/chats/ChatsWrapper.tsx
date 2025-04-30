'use client';
import React, { useEffect, useState } from 'react';
import { ChatMessageResponse } from '@/backend/dto/chat.dto';
import { getGreetingMessage } from '@/backend/chat';
import { Button, Card, Typography } from '@material-tailwind/react';
import { EmptyPlace } from '@/components/PlaceHolders/EmptyPlace';
import { SkeletonLoader } from '@/components/PlaceHolders/SkeletonLoader';
import { AdminMessageModal } from '@/components/Modals/AdminMessageModal';
import { EditGreetingModal } from '@/components/Modals/EditGreetingModal';

export const ChatsWrapper = () => {
    const [loading, setLoading] = useState(false);
    const [greetMessage, setGreetMessage] = React.useState<ChatMessageResponse | undefined>(undefined);
    const [adminMessageShow, setAdminMessageShow] = React.useState(false);
    const [editGreeting, setEditGreeting] = React.useState(false);

    useEffect(() => {
        getFromServerGreetingMessage();
    }, []);

    const TABLE_HEAD = ['Message text', 'Channel type', ''];
    const classes = 'p-4 border-b border-blue-gray-50';

    const getFromServerGreetingMessage = () => {
        getGreetingMessage()
            .then((r) => {
                if (r) setGreetMessage(r);
                console.log(r);
            })
            .finally(() => setLoading(false));
    };

    return (
        <div className="rounded-[10px] border border-stroke bg-white p-4 shadow-1 dark:border-dark-3 dark:bg-gray-dark dark:shadow-card sm:p-7.5">
            <div style={{ display: 'flex', justifyContent: 'space-between' }} className="mb-6">
                <div style={{ display: 'flex', alignItems: 'center' }}></div>
                <div>
                    <Button onClick={() => setAdminMessageShow(true)} color="green">
                        Send Admin message
                    </Button>
                </div>
            </div>
            {!loading && !greetMessage && (
                <EmptyPlace emptyText={'Greeting message no available. Please create'} />
            )}
            {loading && <SkeletonLoader />}
            {!loading && greetMessage && (
                <>
                    <Typography className="mb-5" variant="h4">
                        Greeting info
                    </Typography>
                    <Card className="h-full w-full">
                        <table className="w-full min-w-max table-auto text-left">
                            <thead>
                                <tr>
                                    {TABLE_HEAD.map((head) => (
                                        <th
                                            key={head}
                                            className="border-b border-blue-gray-100 bg-blue-gray-50 p-4"
                                        >
                                            <Typography
                                                variant="small"
                                                color="blue-gray"
                                                className="font-normal leading-none opacity-70"
                                            >
                                                {head}
                                            </Typography>
                                        </th>
                                    ))}
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td className={classes}>
                                        <Typography variant="small" color="blue-gray" className="font-normal">
                                            {greetMessage.text}
                                        </Typography>
                                    </td>
                                    <td className={classes}>
                                        <Typography variant="small" color="blue-gray" className="font-normal">
                                            {greetMessage.channelType}
                                        </Typography>
                                    </td>
                                    <td className={classes}>
                                        <Button color="indigo" onClick={() => setEditGreeting(true)}>
                                            Edit
                                        </Button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </Card>
                </>
            )}
            {adminMessageShow && (
                <AdminMessageModal isOpen={adminMessageShow} onClose={() => setAdminMessageShow(false)} />
            )}
            {editGreeting && greetMessage && (
                <EditGreetingModal
                    isOpen={editGreeting}
                    onClose={() => setEditGreeting(false)}
                    greetingObject={greetMessage}
                    callback={() => {
                        setEditGreeting(false);
                        getFromServerGreetingMessage();
                    }}
                />
            )}
        </div>
    );
};
