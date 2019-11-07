import React from 'react';
import { ThemeProvider } from '@material-ui/core/styles'
import './App.css';
import theme from './config/theme';
import AppLayout from './components/AppLayout';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import routes from './config/routes';

const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <BrowserRouter>
        <AppLayout>
          <Switch>
            {routes.map(r => <Route key={r.title} path={r.path} exact >{r.component}</Route>)}
          </Switch>
        </AppLayout>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
