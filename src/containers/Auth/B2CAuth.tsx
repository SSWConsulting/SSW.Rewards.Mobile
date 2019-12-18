import React, { useEffect, useState, PropsWithChildren } from 'react'
import b2cauth from 'react-azure-adb2c';
import Unauthorized from 'components/Unauthorized/Unauthorized';
import decodeJWT from 'jwt-decode';

export interface DecodedJWT {
    iss: string;
    exp: number;
    nbf: number;
    aud: string;
    idp: string;
    given_name: string;
    family_name: string;
    sub: string;
    emails: string[];
    tfp: string;
    nonce: string;
    role: string;
    scp: string;
    azp: string;
    ver: string;
    iat: number;
}

export const AuthContext = React.createContext<DecodedJWT>({} as DecodedJWT);

const B2CAuth = (props: PropsWithChildren<{}>): any => {

    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [currentUser, setCurrentUser] = useState<DecodedJWT>({} as DecodedJWT);

    const getCurrentUser = () => {
        const decoded = decodeJWT(b2cauth.getAccessToken()) as any;
        setCurrentUser(decoded as DecodedJWT);
    }

    useEffect(() => {
        const token = b2cauth.getAccessToken();
        if (!token) {
            b2cauth.initialize({
                instance: 'https://sswconsultingapp.b2clogin.com/',
                validateAuthority: false,
                tenant: 'sswconsultingapp.onmicrosoft.com',
                signInPolicy: 'B2C_1A_SignUp_SignIn',
                applicationId: 'bb80971c-3a85-4d6d-aef4-cf0baf0f374b',
                cacheLocation: 'localStorage',
                scopes: ['https://sswconsultingapp.onmicrosoft.com/api/user_impersonation'],
                postLogoutRedirectUri: window.location.origin,
            });
            b2cauth.run(() => {
                const decoded = decodeJWT(b2cauth.getAccessToken()) as any;
                console.log('Role: ' + decoded.role);
                if(decoded.role === 'admin') {
                    setIsAuthenticated(true);
                }
            });
        }
    }, [])

    useEffect(() => {
        if (isAuthenticated) {
            getCurrentUser();
        }
    }, [isAuthenticated])

    const render = isAuthenticated ? props.children : <Unauthorized />;
    return (
        <AuthContext.Provider value={currentUser}>
            {render}
        </AuthContext.Provider>
    )
}

export default B2CAuth
