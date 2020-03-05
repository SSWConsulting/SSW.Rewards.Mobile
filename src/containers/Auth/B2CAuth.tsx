import React, { useEffect, useState, PropsWithChildren } from 'react'
import b2cauth from 'react-azure-adb2c';
import { AuthStatus } from './components';
import decodeJWT from 'jwt-decode';
import { useGlobalState } from 'lightweight-globalstate';
import { State } from "store";



const B2CAuth = (props: PropsWithChildren<{}>): any => {

      const [state,updateState] = useGlobalState<State>();
      const [authenticating,setAuthenticating] = useState(false);
      const { authenticated, authorised } = state;

    useEffect(() => {
        const token = b2cauth.getAccessToken();
        if (!token) {
            setAuthenticating(true);
            b2cauth.initialize({
              instance: "https://sswconsultingapp.b2clogin.com/",
              validateAuthority: false,
              tenant: "sswconsultingapp.onmicrosoft.com",
              signInPolicy: "B2C_1A_signup_signin",
              applicationId: "bb80971c-3a85-4d6d-aef4-cf0baf0f374b",
              cacheLocation: "localStorage",
              scopes: [
                "https://sswconsultingapp.onmicrosoft.com/api/user_impersonation"
              ],
              postLogoutRedirectUri: window.location.origin
            });
            b2cauth.run(() => {
                setAuthenticating(false);
                const t = b2cauth.getAccessToken();
                const decoded = decodeJWT(t) as any;
                console.log('Role: ' + decoded.role);
                updateState({ authenticated: true });
                if(decoded.role === 'admin') {
                    console.log('authed');
                    if (t) {
                      updateState({ token: t, authorised:true});
                    }
                }
            });
        }
    }, [])

    useEffect(() => {
        if (authenticated && !state.currentUser) {
           if (!state.currentUser) {
             const decoded = decodeJWT(b2cauth.getAccessToken()) as any;
             updateState({ currentUser: decoded });
           }
        }
    }, [authenticated,state.currentUser,updateState])

    return (<AuthStatus authenticated={authenticated} authenticating={authenticating} authorised={authorised} >
            {props.children}
            </AuthStatus>
    )
}

export default B2CAuth
