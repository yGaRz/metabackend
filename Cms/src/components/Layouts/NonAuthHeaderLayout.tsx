import React from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { getDefaultDescription } from '@/utils/utils';
import { getStoredToken } from '@/hooks/useUser';

export const NonAuthHeaderLayout = ({ children }: { children: React.ReactNode }) => {
    const userToken = getStoredToken();

    const onClick = () => {
        if (userToken?.token) document.location = '/cms/home';
        else document.location = '/auth/login';
    };

    return (
        <div className="relative flex flex-1 flex-col overflow-y-auto overflow-x-hidden">
            <header className="sticky top-0 z-999 flex w-full border-b border-stroke bg-white dark:border-stroke-dark dark:bg-gray-dark">
                <div className="flex flex-grow items-center justify-between px-4 py-5 shadow-2 md:px-5 2xl:px-10">
                    <div className="flex items-center gap-2 sm:gap-4 lg:hidden">
                        <Link className="block flex-shrink-0 lg:hidden" href="/">
                            <Image width={70} height={70} src={'/images/kgs.png'} alt="Logo" />
                        </Link>
                    </div>

                    <div className="hidden xl:block">
                        <div>
                            <h1 className="mb-0.5 text-heading-5 font-bold text-dark dark:text-white">
                                Metacity
                            </h1>
                            <p className="font-medium">{getDefaultDescription()}</p>
                        </div>
                    </div>

                    <div className="flex items-center justify-normal gap-2 2xsm:gap-4 lg:w-full lg:justify-between xl:w-auto xl:justify-normal">
                        {userToken.token ? (
                            <button
                                onClick={onClick}
                                className="bg-transparent hover:bg-blue-500 text-blue-700 font-semibold hover:text-white py-2 px-4 border border-blue-500 hover:border-transparent rounded"
                            >
                                Open admin panel
                            </button>
                        ) : (
                            <button
                                onClick={onClick}
                                className="bg-transparent hover:bg-blue-500 text-blue-700 font-semibold hover:text-white py-2 px-4 border border-blue-500 hover:border-transparent rounded"
                            >
                                Login
                            </button>
                        )}
                    </div>
                </div>
            </header>
            <main>{children}</main>
        </div>
    );
};
