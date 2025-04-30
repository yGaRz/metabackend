import { NextRequest, NextResponse } from 'next/server';

export async function middleware(request: NextRequest) {
    console.log('Page refreshed: ', request.nextUrl.pathname);
    const { pathname, href } = request.nextUrl;
    const response = NextResponse.next();

    // TODO: set auth logic here
    return response;
}
