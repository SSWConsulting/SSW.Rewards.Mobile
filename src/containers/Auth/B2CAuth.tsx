import React, { useEffect, useState, PropsWithChildren } from 'react'
import b2cauth from 'react-azure-adb2c';
import Loader from 'components/Loader/Loader';
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
    scp: string;
    azp: string;
    ver: string;
    iat: number;
}

export const AuthContext = React.createContext<DecodedJWT>({} as DecodedJWT);

const B2CAuth = (props: PropsWithChildren<{}>): any => {

    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [currentUser, setCurrentUser] = useState<DecodedJWT>({} as DecodedJWT);

    b2cauth.initialize({
        instance: 'https://sswconsultingapp.b2clogin.com/',
        validateAuthority: false,
        tenant: 'sswconsultingapp.onmicrosoft.com',
        signInPolicy: 'B2C_1_Admin-Signin',
        applicationId: '33e787e1-caeb-475b-a6dc-7e014d0a252a',
        cacheLocation: 'localStorage',
        scopes: ['https://sswconsultingapp.onmicrosoft.com/admin/user_impersonation'],
        postLogoutRedirectUri: window.location.origin,
    });

    const getCurrentUser = () => {
        const decoded = decodeJWT(b2cauth.getAccessToken()) as any;
        setCurrentUser( decoded as DecodedJWT);
    }

    useEffect(() => {
        setIsAuthenticated(b2cauth.getAccessToken());
        b2cauth.run(() => {
            setIsAuthenticated(b2cauth.getAccessToken());
            getCurrentUser();
        });
    }, [])


    const render = isAuthenticated ? props.children : <Loader />;
    return (
        <AuthContext.Provider value={currentUser}>
            {render}
        </AuthContext.Provider>
    )

}

export default B2CAuth
