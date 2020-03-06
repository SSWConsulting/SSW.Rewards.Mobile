import React, { useEffect } from "react";
import { LeaderboardTableRow, SkeletonRow } from "./components";
import { Table } from 'components';
import { useGlobalState } from "lightweight-globalstate";
import { State } from "store";
import { LeaderboardListViewModel, LeaderboardUserDto} from "services";
import { fetchData, renderComponentXTimes } from "utils";
import { useAuthenticatedClient } from "hooks";

const Leaderboard = (): JSX.Element => {
  const [state, updateState] = useGlobalState<State>();
  const client = useAuthenticatedClient(state.leaderboardClient, state.token);

  const getLeaderboard = async () => {
    const response = await fetchData<LeaderboardListViewModel>(() => client.get());
    response && response.users && updateState({ users: response.users });
  };

  useEffect(() => {client && getLeaderboard()}, [client]);

  return (
    <Table items={["Rank", "Picture", "Name", "Points"]}>
      {state.users &&
        state.users.map((user: LeaderboardUserDto) => (
          <LeaderboardTableRow
            key={user.userId}
            user={user}></LeaderboardTableRow>
        ))}
      {!state.users && renderComponentXTimes(SkeletonRow,20)}
    </Table>
  );
};

export default Leaderboard;
