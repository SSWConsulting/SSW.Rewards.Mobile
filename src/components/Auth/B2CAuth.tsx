import React, { useEffect, useState, PropsWithChildren } from 'react'
import b2cauth from 'react-azure-adb2c';
import Loader from '../Loader/Loader';
import decodeJWT from 'jwt-decode';

export const AuthContext = React.createContext({});

const B2CAuth = (props: PropsWithChildren<{}>): any => {

    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [currentUser, setCurrentUser] = useState({});

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
        setCurrentUser({
            name: decoded.name,
            firstName: decoded.given_name,
            lastName: decoded.family_name,
            emails: decoded.emails,
            city: decoded.city,
            country: decoded.country,
        });
    }

    useEffect(() => {
            setIsAuthenticated(b2cauth.getAccessToken());
        b2cauth.run(() => {
            setIsAuthenticated(b2cauth.getAccessToken());
            getCurrentUser();
        });
    }, [])


  

const  render = isAuthenticated ? props.children : <Loader />;
return (
    <AuthContext.Provider value={currentUser}>
        {render}
    </AuthContext.Provider>
)
  
}

export default B2CAuth
