import React from 'react';
import HomeIcon from '@material-ui/icons/Home';
import Home from '../containers/Home';

export const routes = [
{
    path: '/',
    title: 'Home',
    component: Home,
    icon: HomeIcon,
},
{
    path: '/other',
    title: 'Other',
    component: <div> other route</div>,
    icon: HomeIcon,
}
];

export default routes;