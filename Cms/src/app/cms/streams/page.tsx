import { Metadata } from 'next';
import { getDefaultDescription, getMetadataTitle } from '@/utils/utils';
import DefaultLayout from '@/components/Layouts/DefaultLaout';
import Breadcrumb from '@/components/Breadcrumbs/Breadcrumb';
import React from 'react';
import { StreamsWrapper } from '@/components/CMS/streams/StreamsWrapper';

const pageName = 'Streams';

export const metadata: Metadata = {
    title: getMetadataTitle(pageName),
    description: getDefaultDescription(),
};

const StreamPage = () => {
    return (
        <DefaultLayout>
            <div className="mx-auto w-full">
                <Breadcrumb pageName={pageName} />
                <StreamsWrapper />
            </div>
        </DefaultLayout>
    );
};

export default StreamPage;
