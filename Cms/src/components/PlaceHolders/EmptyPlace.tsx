import React from 'react';
import { InboxIcon } from '@heroicons/react/24/solid';

type Props = {
    emptyText: string;
};

export const EmptyPlace = ({ emptyText }: Props) => {
    return (
        <div className="flex justify-center flex-col items-center mt-10 text-gray-500">
            <InboxIcon className="h-10 w-10" />
            <span>{emptyText}</span>
        </div>
    );
};
