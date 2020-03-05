import React, { useEffect } from "react";
import { LeaderboardTable } from "./components";
import { useGlobalState } from "lightweight-globalstate";
import { State } from "store";
import { LeaderboardListViewModel} from "services";
import { fetchData } from "utils";
import { useAuthenticatedClient } from "hooks";

const Leaderboard = (): JSX.Element => {
  const [state, updateState] = useGlobalState<State>();
  const client = useAuthenticatedClient(state.leaderboardClient, state.token);

  const getLeaderboard = async () => {
    const response = await fetchData<LeaderboardListViewModel>(() => client.get());
    response && response.users && updateState({ users: response.users });
  };

  useEffect(() => {client && getLeaderboard()}, [client]);

  return <LeaderboardTable users={state.users}></LeaderboardTable>;
};

export default Leaderboard;
