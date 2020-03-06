import React, { PropsWithChildren } from 'react';
import {AppBar ,Box, CssBaseline , Divider, Drawer, Hidden,IconButton, List, ListItem,ListItemIcon,ListItemText,Toolbar,Typography } from '@material-ui/core';
import { makeStyles, useTheme } from '@material-ui/core/styles';
import MenuIcon from '@material-ui/icons/Menu';
import routes from 'config/routes';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import { SSWRed } from 'config/theme';
import { useGlobalState } from 'lightweight-globalstate';
import Paper from "@material-ui/core/Paper";


const drawerWidth = 240;

const useStyles = makeStyles(theme => ({
  root: {
    display: "flex",
    overflowY: "hidden"
  },
  drawer: {
    [theme.breakpoints.up("sm")]: {
      width: drawerWidth,
      flexShrink: 0
    }
  },
  appBar: {
    zIndex: 1301,
    [theme.breakpoints.up("sm")]: {
      width: "100%",
      marginLeft: drawerWidth
    }
  },
  menuButton: {
    marginRight: theme.spacing(2),
    [theme.breakpoints.up("sm")]: {
      display: "none"
    }
  },
  toolbar: theme.mixins.toolbar,
  drawerPaper: {
    width: drawerWidth
  },
  content: {
    flexGrow: 1,
    paddingLeft: theme.spacing(3),
    paddingRight: theme.spacing(3),
    marginTop: theme.spacing(10),
    marginBottom: theme.spacing(10),
    paddingBottom: theme.spacing(15),

    overflowY: "scroll",
    maxHeight: "100vh"
  }
}));

const AppLayoutComponent = (props: PropsWithChildren<RouteComponentProps>) => {

    const classes = useStyles();
    const theme = useTheme();
    const [mobileOpen, setMobileOpen] = React.useState(false);

    const handleDrawerToggle = () => {
        setMobileOpen(!mobileOpen);
    };

    const [state]:any = useGlobalState();

    const currentRoute = routes.find(r =>  props.location.pathname === r.path);


    const drawer = (
        <div>
            <div className={classes.toolbar} />
              <Typography variant="body1" noWrap>
                  Welcome {state.currentUser && state.currentUser.given_name},
                </Typography>
            <Divider />
            <List>
                {routes.map((r) => {
                    const isCurrentRoute = currentRoute && currentRoute.path === r.path;
                    const handleClick = () => {
                        props.history.push(r.path);
                        if (mobileOpen) {
                            handleDrawerToggle();
                        }
                    }
                    return (
                      
                        <ListItem key={r.path} button onClick={handleClick}>
                            <ListItemIcon>
                                <r.icon style={isCurrentRoute ? { fill: `${SSWRed}` } : {}} />
                            </ListItemIcon>
                            <ListItemText primary={r.title} style={isCurrentRoute ? { color: `${SSWRed}` } : {}} />
                        </ListItem>
                        
                    )
                })}
            </List>
            <Divider />
        </div>
    );

    return (
      <div className={classes.root}>
        <CssBaseline />
        <AppBar position="fixed" className={classes.appBar}>
          <Toolbar>
            <IconButton
              color="inherit"
              aria-label="open drawer"
              edge="start"
              onClick={handleDrawerToggle}
              className={classes.menuButton}>
              <MenuIcon />
            </IconButton>
            <Box display="flex" justifyContent="space-between" width={'100%'}>
              <Box >
                          <Hidden xsDown implementation="css">


                </Hidden>
              </Box>
              <Box marginRight={'40%'}>
                <Typography variant="h6" align="center">
                  {currentRoute && currentRoute.title}{" "}
                </Typography>
              </Box>
            </Box>
          </Toolbar>
        </AppBar>
        <nav className={classes.drawer} aria-label="app-drawer">
          <Hidden smUp implementation="css">
            <Drawer
              variant="temporary"
              anchor={theme.direction === "rtl" ? "right" : "left"}
              open={mobileOpen}
              onClose={handleDrawerToggle}
              classes={{
                paper: classes.drawerPaper
              }}
              ModalProps={{
                keepMounted: true
              }}>
              {drawer}
            </Drawer>
          </Hidden>
          <Hidden xsDown implementation="css">
            <Drawer
              classes={{
                paper: classes.drawerPaper
              }}
              variant="permanent"
              open>
              {drawer}
            </Drawer>
          </Hidden>
        </nav>
        <main className={classes.content}>
          {/* <div className={classes.toolbar} /> */}
          {props.children}
        </main>
      </div>
    );
}

export const AppLayout = withRouter(AppLayoutComponent);