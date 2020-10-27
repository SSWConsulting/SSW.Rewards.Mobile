import React, { useEffect, PropsWithChildren } from 'react';
import { LeaderboardTableRow, SkeletonRow } from './components';
import { Table } from 'components';
import { useGlobalState } from 'lightweight-globalstate';
import { State } from 'store';
import { LeaderboardListViewModel, LeaderboardUserDto } from 'services';
import { fetchData, renderComponentXTimes } from 'utils';
import { useAuthenticatedClient } from 'hooks';
import { Card, CardContent, Paper, TextField, Typography } from '@material-ui/core';
import { withRouter, RouteComponentProps } from 'react-router-dom';

const Leaderboard = (props: PropsWithChildren<RouteComponentProps>): JSX.Element => {
  const { history } = props;

  const [state, updateState] = useGlobalState<State>();
  const client = useAuthenticatedClient(state.leaderboardClient, state.token);

  const getLeaderboard = async () => {
    const response = await fetchData<LeaderboardListViewModel>(() => client.get());
    response && response.users && updateState({ usersDefault: response.users, users: response.users });
  };

  const filterUsers = (value: string) => {
    const filteredUsers = state.usersDefault?.filter((u) => {
      return u.name?.toLowerCase().includes(value.toLowerCase());
    });

    filteredUsers && updateState({ users: filteredUsers });
  };

  const goToUser = (userId: number) => {
    history.push(`/user/${userId}`);
  };

  useEffect(() => {
    client && getLeaderboard();
  }, [client]);

  return (
    <>
      <h1>Leaderboard</h1>
      <Typography>All users ranked by total points</Typography>
      <div style={{ display: 'flex', justifyContent: 'center', padding: '5px' }}>
        <TextField label="Search" onChange={(e) => filterUsers(e.target.value)} style={{ flexGrow: 1 }} />
      </div>
      <Table items={['Rank', 'Name', 'Points']}>
        {state.users &&
          state.users.map((user: LeaderboardUserDto) => (
            <LeaderboardTableRow key={user.userId} user={user} onClick={goToUser}></LeaderboardTableRow>
          ))}
        {!state.users && renderComponentXTimes(SkeletonRow, 20)}
      </Table>
    </>
  );
};

export default withRouter(Leaderboard);
