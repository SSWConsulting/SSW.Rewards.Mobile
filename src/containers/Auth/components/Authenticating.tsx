import React from 'react';
import { Container, Typography } from '@material-ui/core';

export const Authenticating = () => {
  return (
    <Container fixed>
      <br />
      <Typography align="center" variant="h3" color="primary">
        SSW Rewards - Admin
      </Typography>
      <Typography align="center">Authenticating...</Typography>
    </Container>
  );
};
