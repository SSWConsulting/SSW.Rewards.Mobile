import HomeIcon from '@material-ui/icons/Home';
import Home from '../containers/Home';

interface Route  {
    path: string;
    title: string;
    component: any;
    icon: any,
}

export const routes = [
{
    path: '/',
    title: 'Home',
    component: Home,
    icon: HomeIcon,
}
] as Route[];

export default routes;