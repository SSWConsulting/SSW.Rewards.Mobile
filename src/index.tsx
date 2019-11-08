import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import * as serviceWorker from './serviceWorker';
import b2cauth from 'react-azure-adb2c';

b2cauth.initialize({
    tenant: 'sswconsultingapp.onmicrosoft.com',
    signInPolicy: 'B2C_1_Admin-Signin',
    applicationId: '33e787e1-caeb-475b-a6dc-7e014d0a252a',
    cacheLocation: 'sessionStorage',
    scopes: ['https://sswconsultingapp.onmicrosoft.com/admin/user_impersonation'],
    redirectUri: 'http://localhost:3000',
    postLogoutRedirectUri: window.location.origin,
  });


  b2cauth.run(() => {
    ReactDOM.render(<App />, document.getElementById('root'));
    serviceWorker.unregister();
  });
