import React, { useEffect } from "react";
import { useGlobalState } from "lightweight-globalstate";
import { State } from "store";
import {
  AchievementClient,
  AchievementAdminListViewModel
} from "services";
import { fetchData } from "utils";
import { useAuthenticatedClient } from "hooks";
import { AchievementTable } from './components'

const Achievements = (): JSX.Element => {
  const [state, updateState] = useGlobalState<State>();
  const client = useAuthenticatedClient<AchievementClient>(
    state.achievementClient,
    state.token
  );
  

  const getAchievements = async () => {
    const response = await fetchData<AchievementAdminListViewModel>(() => client.adminList());
    response && response.achievements && updateState({ achievements: response.achievements });
  };

  useEffect(() => {
    client && getAchievements();
  }, [client]);

return (<AchievementTable achievements={state.achievements}/>)
};

export default Achievements;
