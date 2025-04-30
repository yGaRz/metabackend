import ECommerce from '@/components/Dashboard/E-commerce';
import { Metadata } from 'next';
import React from 'react';
import { Main } from '@/components/Main/Main';
import { getDefaultDescription, getMetadataTitle } from '@/utils/utils';

const pageName = 'Metacity Admin';

export const metadata: Metadata = {
    title: getMetadataTitle(pageName),
    description: getDefaultDescription(),
};

export default function Home() {
    return <Main />;
}
