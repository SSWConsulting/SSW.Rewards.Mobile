import React from 'react';
import { ThemeProvider } from '@material-ui/core/styles'
import './App.css';
import theme from 'config/theme';
import AppLayout from 'components/AppLayout/AppLayout';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import routes from 'config/routes';
import B2CAuth from 'containers/Auth/B2CAuth';

const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <B2CAuth>
        <BrowserRouter>
          <AppLayout>
            <Switch>
              {routes.map(r => <Route key={r.title} path={r.path} component={r.component} exact/>)}
            </Switch>
          </AppLayout>
        </BrowserRouter>
      </B2CAuth>
    </ThemeProvider>
  );
}

export default App;
