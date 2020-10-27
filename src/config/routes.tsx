import HomeIcon from '@material-ui/icons/Home';
import PeopleIcon from "@material-ui/icons/People";
import StarsIcon from "@material-ui/icons/Stars";
import RedeemIcon from "@material-ui/icons/Redeem";
import Home from 'containers/Home';
import Leaderboard from "containers/Leaderboard";
import Achievements from "containers/Achievements";
import Rewards from "containers/Rewards";
import { UserDetail } from 'containers/UserDetail';

interface Route  {
    path: string;
    title: string;
    component: React.ComponentType<any>;
    icon: any,
    hidden?: boolean
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
           path: "/user/:userId",
           title: "User",
           component: UserDetail,
           hidden: true
         },
         {
           path: "/achievements",
           title: "Achievements",
           component: Achievements,
           icon: StarsIcon
         },
         {
           path: "/rewards",
           title: "Rewards",
           component: Rewards,
           icon: RedeemIcon
         }
       ] as Route[];

export default routes;