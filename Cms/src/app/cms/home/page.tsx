import { Metadata } from 'next';
import React from 'react';
import { getDefaultDescription, getMetadataTitle } from '@/utils/utils';
import DefaultLayout from '@/components/Layouts/DefaultLaout';
import ECommerce from '@/components/Dashboard/E-commerce';

const pageName = 'Home';

export const metadata: Metadata = {
    title: getMetadataTitle(pageName),
    description: getDefaultDescription(),
};

export default function Home() {
    return (
        <DefaultLayout>
            <ECommerce />
        </DefaultLayout>
    );
}
