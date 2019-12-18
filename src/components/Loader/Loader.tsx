import React from 'react'
import { Typography, Container } from '@material-ui/core';


const Loader = () => {
    return (
        <Container fixed>
            <Typography align="center" variant="h3" color="primary"> SSW Rewards - Admin </Typography>
            <Typography align="center"><i className="fa fa-cog fa-spin" /> Loading...</Typography>
        </Container>
    )
}

export default Loader
