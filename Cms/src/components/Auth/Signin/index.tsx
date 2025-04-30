'use client';
import React from 'react';
import SigninWithPassword from '../SigninWithPassword';
import { Toaster } from 'react-hot-toast';

export default function Signin() {
    return (
        <div>
            <SigninWithPassword />
            <Toaster position="top-right" reverseOrder={false} />
        </div>
    );
}
