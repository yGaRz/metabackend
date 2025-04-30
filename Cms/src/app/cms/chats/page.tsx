import { Metadata } from 'next';
import { getDefaultDescription, getMetadataTitle } from '@/utils/utils';
import DefaultLayout from '@/components/Layouts/DefaultLaout';
import React from 'react';
import Breadcrumb from '@/components/Breadcrumbs/Breadcrumb';
import { ChatsWrapper } from '@/components/CMS/chats/ChatsWrapper';

const pageName = 'Chats';

export const metadata: Metadata = {
    title: getMetadataTitle(pageName),
    description: getDefaultDescription(),
};

const ChatPage = () => {
    return (
        <DefaultLayout>
            <div className="mx-auto w-full">
                <Breadcrumb pageName={pageName} />
                <ChatsWrapper />
            </div>
        </DefaultLayout>
    );
};

export default ChatPage;
