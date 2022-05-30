import AccountBoxIcon from "@material-ui/icons/AccountBox";
import Achievements from "containers/Achievements";
import Home from 'containers/Home';
import HomeIcon from '@material-ui/icons/Home';
import Leaderboard from "containers/Leaderboard";
import PeopleIcon from "@material-ui/icons/People";
import { ProfileDetail } from 'containers/ProfileDetail';
import Profiles from "containers/Profiles";
import RedeemIcon from "@material-ui/icons/Redeem";
import Rewards from "containers/Rewards";
import StarsIcon from "@material-ui/icons/Stars";
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
         },
         {
           path: "/profiles",
           title: "Profiles",
           component: Profiles,
           icon: AccountBoxIcon
         },
         {
           path: "/profiles/:id",
           title: "Profile",
           component: ProfileDetail,
           hidden: true
         }
       ] as Route[];

export default routes;