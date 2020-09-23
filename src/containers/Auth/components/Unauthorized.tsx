import React from 'react'
import authentication from 'react-azure-adb2c';
import { Typography, Container } from '@material-ui/core';


export const Unauthorized = () => {

    const signOut = () => {
        authentication.signOut();
    }

    return (
        <Container fixed>
           <br/>
                <Typography align="center" variant="h3" color="primary"> SSW Rewards - Admin </Typography>
                <Typography align="center"><i className="fas fa-exclamation-triangle"/> You are not authorized to view this admin portal.</Typography>
            <br/>
           <br/>
                <Typography align="center">
                   <br/>
                        Click here to try again:
                    <br/>
                   <br/>
                    <button onClick={signOut}>Sign Out / Sign In</button>
                    <br/>
                </Typography>
            <br/>        
        </Container>
    )
}