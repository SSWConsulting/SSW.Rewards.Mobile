import { createMuiTheme } from '@material-ui/core/styles';

export const SSWRed = '#CC4141';
const SSWGrey = '#333333';
const SSWLightGrey = '#CCCCCC';

export const theme = createMuiTheme({
    palette: {
      primary: {
        main: SSWRed,
      },
      secondary: {
        light: SSWLightGrey,
        main: SSWGrey,
      },
    },
  });

  export default theme;