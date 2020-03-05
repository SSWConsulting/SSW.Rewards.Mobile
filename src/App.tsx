import React from "react";
import { ThemeProvider } from "@material-ui/core/styles";
import "./App.css";
import theme from "config/theme";
import { AppLayout }  from "components";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import routes from "config/routes";
import B2CAuth from "containers/Auth/B2CAuth";
import { StateProvider } from "lightweight-globalstate";
import { State, createInitialState } from "store";


const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <StateProvider<State> initialState={createInitialState()}>
        <BrowserRouter>
          <B2CAuth>
            <AppLayout>
              <Switch>
                {routes.map(r => (
                  <Route
                    key={r.title}
                    path={r.path}
                    component={r.component}
                    exact
                  />
                ))}
              </Switch>
            </AppLayout>
          </B2CAuth>
        </BrowserRouter>
      </StateProvider>
    </ThemeProvider>
  );
};

export default App;
