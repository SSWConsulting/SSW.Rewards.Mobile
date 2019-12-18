import React, { PropsWithChildren, useContext } from 'react';
import AppBar from '@material-ui/core/AppBar';
import CssBaseline from '@material-ui/core/CssBaseline';
import Divider from '@material-ui/core/Divider';
import Drawer from '@material-ui/core/Drawer';
import Hidden from '@material-ui/core/Hidden';
import IconButton from '@material-ui/core/IconButton';
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import MenuIcon from '@material-ui/icons/Menu';
import Toolbar from '@material-ui/core/Toolbar';
import Typography from '@material-ui/core/Typography';
import { makeStyles, useTheme } from '@material-ui/core/styles';
import routes from 'config/routes';
import { withRouter, RouteComponentProps } from 'react-router-dom';
import { SSWRed } from 'config/theme';
import { AuthContext } from 'containers/Auth/B2CAuth'

const drawerWidth = 240;

const useStyles = makeStyles(theme => ({
    root: {
        display: 'flex',
    },
    drawer: {
        [theme.breakpoints.up('sm')]: {
            width: drawerWidth,
            flexShrink: 0,
        },
    },
    appBar: {
        zIndex: 1301,
        [theme.breakpoints.up('sm')]: {
            width: '100%',
            marginLeft: drawerWidth,
        },
    },
    menuButton: {
        marginRight: theme.spacing(2),
        [theme.breakpoints.up('sm')]: {
            display: 'none',
        },
    },
    toolbar: theme.mixins.toolbar,
    drawerPaper: {
        width: drawerWidth,
    },
    content: {
        flexGrow: 1,
        padding: theme.spacing(3),
    },
}));

const AppLayout = (props: PropsWithChildren<RouteComponentProps>) => {

    const classes = useStyles();
    const theme = useTheme();
    const [mobileOpen, setMobileOpen] = React.useState(false);

    const handleDrawerToggle = () => {
        setMobileOpen(!mobileOpen);
    };

    const user = useContext(AuthContext);


    const drawer = (
        <div>
            <div className={classes.toolbar} />
            <Divider />
            <List>
                {routes.map((r) => {
                    const isCurrentRoute = props.location.pathname === r.path;
                    const handleClick = () => {
                        props.history.push(r.path);
                        if (mobileOpen) {
                            handleDrawerToggle();
                        }
                    }
                    console.log(props.location)
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
                        className={classes.menuButton}
                    >
                        <MenuIcon />
                    </IconButton>
                    <Typography variant="h6" noWrap>
                        Welcome {user && user.given_name},
                    </Typography>
                </Toolbar>
            </AppBar>
            <nav className={classes.drawer} aria-label="app-drawer">
                <Hidden smUp implementation="css">
                    <Drawer
                        variant="temporary"
                        anchor={theme.direction === 'rtl' ? 'right' : 'left'}
                        open={mobileOpen}
                        onClose={handleDrawerToggle}
                        classes={{
                            paper: classes.drawerPaper,
                        }}
                        ModalProps={{
                            keepMounted: true,
                        }}
                    >
                        {drawer}
                    </Drawer>
                </Hidden>
                <Hidden xsDown implementation="css">
                    <Drawer
                        classes={{
                            paper: classes.drawerPaper,
                        }}
                        variant="permanent"
                        open
                    >
                        {drawer}
                    </Drawer>
                </Hidden>
            </nav>
            <main className={classes.content}>
                <div className={classes.toolbar} />
                {props.children}
            </main>
        </div>
    );
}

export default withRouter(AppLayout);