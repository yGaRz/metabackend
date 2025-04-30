import { NextRequest, NextResponse } from 'next/server';
import Cookies from 'js-cookie';

export interface Token {
    token: string | undefined;
}

export function getStoredToken(middlewareRequest?: NextRequest): Token {
    return middlewareRequest
        ? {
              token: middlewareRequest.cookies.get('token')?.value,
          }
        : {
              token: Cookies.get('token'),
          };
}

// export function useUser(): Token | undefined {
//     let userToken = getStoredToken();

//     const getUser = async (id: number, signal?: AbortSignal): Promise<any> => {
//         if (!id) return null;

//         const response = await axios.get(`${apiUrl}/getStoreItems`, {
//             headers: {
//                 'Backend-Secret':
//             }
//         });

//         return { user };
//     };

//     useEffect(() => {
//         const orgtree = localStorage.getItem(UserDataTypes.ORGTREE);

//         setOrgtree(JSON.parse(!orgtree || orgtree === 'undefined' ? '[]' : orgtree));
//     }, []);

//     if (userData?.user?.person) userData.user.person.orgtree = orgtree;

//     return { user: userData, getUser };
// }
