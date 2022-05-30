import React, { PropsWithChildren } from 'react'
import { Unauthorized, Authenticating } from './';


export const AuthStatus = (props: PropsWithChildren<{authenticated: boolean,authenticating: boolean,authorised: boolean}>) => {

    const {authenticated,authenticating,authorised, children} = props;

    return (
      <>
      {!authenticated && authenticating && <Authenticating/>}
      {authenticated && !authorised && <Unauthorized />}
      {authenticated && authorised && children}
      </>
    )
}

