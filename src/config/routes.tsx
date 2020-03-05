import HomeIcon from '@material-ui/icons/Home';
import PeopleIcon from "@material-ui/icons/People";
import StarsIcon from "@material-ui/icons/Stars";
import Home from 'containers/Home';
import Leaderboard from "containers/Leaderboard";
import Achievements from "containers/Achievements";

interface Route  {
    path: string;
    title: string;
    component: React.ComponentType<any>;
    icon: any,
}

export const routes = [
         {
           path: "/",
           title: "Home",
           component: Home,
           icon: HomeIcon
         },
         {
           path: "/leaderboard",
           title: "Leaderboard",
           component: Leaderboard,
           icon: PeopleIcon
         },
         {
           path: "/achievements",
           title: "Achievements",
           component: Achievements,
           icon: StarsIcon
         }
       ] as Route[];

export default routes;