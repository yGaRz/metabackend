'use client';
import React from 'react';
import { NonAuthHeaderLayout } from '@/components/Layouts/NonAuthHeaderLayout';

export const Main = () => {
    return (
        <NonAuthHeaderLayout>
            <div className="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">Стартовая страница</div>
        </NonAuthHeaderLayout>
    );
};
