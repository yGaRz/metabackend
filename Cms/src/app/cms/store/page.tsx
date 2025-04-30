import { Metadata } from 'next';
import { getDefaultDescription, getMetadataTitle } from '@/utils/utils';
import DefaultLayout from '@/components/Layouts/DefaultLaout';
import Breadcrumb from '@/components/Breadcrumbs/Breadcrumb';
import React from 'react';
import { StoreWrapper } from '@/components/CMS/store/StoreWrapper';

const pageName = 'Store';

export const metadata: Metadata = {
    title: getMetadataTitle(pageName),
    description: getDefaultDescription(),
};

const StorePage = () => {
    return (
        <DefaultLayout>
            <div className="mx-auto w-full">
                <Breadcrumb pageName={pageName} />
                <StoreWrapper />
            </div>
        </DefaultLayout>
    );
};

export default StorePage;
