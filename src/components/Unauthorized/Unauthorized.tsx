import React from 'react'
import authentication from 'react-azure-adb2c';
import { Typography, Container } from '@material-ui/core';


const Unauthorized = () => {

    const signOut = () => {
        console.log('auth: ', authentication.getAccessToken());
        authentication.signOut();
    }

    return (
        <Container fixed>
            <p>
                <Typography align="center" variant="h3" color="primary"> SSW Rewards - Admin </Typography>
                <Typography align="center"><i className="fas fa-exclamation-triangle"/> You are not authorized to view this admin portal.</Typography>
            </p>
            <p>
                <Typography align="center">
                    <p>
                        Click here to try again:
                    </p>
                    <p>
                    <button onClick={signOut}>Sign Out / Sign In</button>
                    </p>
                </Typography>
            </p>        
        </Container>
    )
}

export default Unauthorized
