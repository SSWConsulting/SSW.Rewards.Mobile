import {
  AchievementClient,
  ClaimAchievementResult,
  ClaimAchievementStatus,
  ClaimRewardResult,
  RewardClient,
  RewardStatus,
  UserClient,
  UserViewModel,
} from 'services';
import { ColDef, ColParams, DataGrid, SortDirection } from '@material-ui/data-grid';
import MuiAlert, { AlertProps } from '@material-ui/lab/Alert';
import React, { useEffect, useState } from 'react';
import { useParams, withRouter } from 'react-router-dom';

import { ActionFab } from './components';
import Avatar from '@material-ui/core/Avatar';
import CircularProgress from '@material-ui/core/CircularProgress';
import Grid from '@material-ui/core/Grid';
import Snackbar from '@material-ui/core/Snackbar';
import { State } from 'store';
import { Tabs } from 'components';
import { fetchData } from 'utils';
import { makeStyles } from '@material-ui/core/styles';
import { useAuthenticatedClient } from 'hooks';
import { useGlobalState } from 'lightweight-globalstate';

function Alert(props: AlertProps) {
  return <MuiAlert elevation={6} variant="filled" {...props} />;
}
export type MessageType = 'success' | 'error' | 'warning' | 'info';

export interface SnackbarProps {
  message: string;
  severity: MessageType;
}

export interface RouteParams {
  userId: string;
}

const achievementColumns: ColDef[] = [
  {
    field: 'achievementName',
    headerName: 'Name',
    width: 600,
    renderHeader: (params: ColParams) => <strong>Name</strong>,
  },
  {
    field: 'achievementValue',
    headerName: 'Value',
    type: 'number',
    renderHeader: (params: ColParams) => <strong>Value</strong>,
  },
  {
    field: 'awardedAt',
    headerName: 'Awarded On',
    width: 300,
    type: 'dateTime',
    renderHeader: (params: ColParams) => <strong>Awarded On</strong>,
  },
];

const rewardColumns: ColDef[] = [
  {
    field: 'rewardName',
    headerName: 'Name',
    width: 600,
    renderHeader: (params: ColParams) => <strong>Name</strong>,
  },
  {
    field: 'rewardCost',
    headerName: 'Cost',
    type: 'number',
    renderHeader: (params: ColParams) => <strong>Cost</strong>,
  },
  {
    field: 'awardedAt',
    headerName: 'Awarded On',
    width: 300,
    type: 'dateTime',
    renderHeader: (params: ColParams) => <strong>Awarded On</strong>,
  },
];

const sortModel = [
  {
    field: 'awardedAt',
    sort: 'desc' as SortDirection,
  },
];

const useStyles = makeStyles((theme) => ({
  detail: {
    marginBottom: theme.spacing(3),
  },
  detailItem: {
    width: '75px',
    fontWeight: 'bold',
    display: 'inline-block',
  },
  table: {
    backgroundColor: '#fff',
  },
}));

const UserDetailComponent = () => {
  const [state, updateState] = useGlobalState<State>();
  const [currentTab, setCurrentTab] = useState('Achievements');
  const classes = useStyles();
  const client = useAuthenticatedClient<UserClient>(state.userClient, state.token);
  const rewardClient = useAuthenticatedClient<RewardClient>(state.rewardClient, state.token);
  const achievementClient = useAuthenticatedClient<AchievementClient>(state.achievementClient, state.token);
  const [loading, setLoading] = useState(true);
  const [snackbar, setSnackbar] = useState<SnackbarProps>();
  const [open, setOpen] = useState(false);
  const { userId } = useParams<RouteParams>();
  const [totalRewards, setTotalRewards] = useState(0);

  const handleClose = () => {
    setOpen(false);
  };

  const getUserDetails = async () => {
    setLoading(true);
    const response = await fetchData<UserViewModel>(() => client && client?.getUser(userId));
    setLoading(false);
    if (response) {
      updateState({ userDetail: response });
      response.rewards && setTotalRewards(response.rewards.reduce((a, b) => a + (b.rewardCost || 0), 0));
    }
  };

  const claimReward = async (selectedReward: string) => {
    const response = await fetchData<ClaimRewardResult>(() =>
      rewardClient.claimForUser({
        code: selectedReward,
        userId,
      })
    );
    if (response) {
      switch (response.status) {
        case RewardStatus.NotEnoughPoints:
          openSnackbar('Not enough points to claim', 'error');
          break;
        case RewardStatus.Claimed:
          openSnackbar('Reward claimed!', 'success');
          loadData();
          break;
        default:
          openSnackbar('A problem occured', 'error');
      }
    } else {
      openSnackbar('Could not get a response from the server', 'error');
    }
  };

  const claimAchievement = async (selectedAchievement: string) => {
    const response = await fetchData<ClaimAchievementResult>(() =>
      achievementClient?.claimForUser({
        code: selectedAchievement,
        userId,
      })
    );
    if (response) {
      switch (response.status) {
        case ClaimAchievementStatus.NotEnoughPoints:
          openSnackbar('Not enough points to claim', 'error');
          break;
        case ClaimAchievementStatus.Claimed:
          openSnackbar('Achievement claimed!', 'success');
          loadData();
          break;
        case ClaimAchievementStatus.Duplicate:
          openSnackbar('Already claimed!', 'error');
          break;
        default:
          openSnackbar('A problem occured', 'error');
      }
    } else {
      openSnackbar('Could not get a response from the server', 'error');
    }
  };

  const changeFab = (tab: string) => {
    setCurrentTab(tab);
  };

  const openSnackbar = (message: string, severity: MessageType): void => {
    setSnackbar({
      message,
      severity,
    });
    setOpen(true);
  };

  const loadData = () => {
    setLoading(true);
    client && getUserDetails();
  };

  useEffect(() => {
    loadData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [client]);

  return (
    <>
      <ActionFab
        user={state.userDetail}
        claimReward={claimReward}
        claimAchievement={claimAchievement}
        selectedTab={currentTab}
      />
      {!loading && state.userDetail ? (
        <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <Avatar alt={state.userDetail.fullName} src={state.userDetail.profilePic}>
              {state.userDetail.fullName}
            </Avatar>
            <span style={{ marginLeft: '10px' }}>{state.userDetail.fullName}</span>
          </div>
          <p>
            <Grid container spacing={1} direction="column">
              <Grid item>
                <div className={classes.detailItem}>Total:</div> {state.userDetail.points?.toLocaleString()} pts
              </Grid>
              <Grid item>
                <div className={classes.detailItem}>Spent:</div> -{totalRewards?.toLocaleString()} pts
              </Grid>
              <Grid item>
                <div className={classes.detailItem}>Available:</div> {state.userDetail.balance?.toLocaleString()} pts
              </Grid>
            </Grid>
          </p>

          <Tabs
            titles={['Achievements', 'Rewards']}
            tabChanged={changeFab}
            tabContent={[
              <DataGrid
                autoHeight
                rows={
                  state?.userDetail?.achievements?.map((a, i) => {
                    var b: any = a;
                    b.id = i;
                    return b;
                  }) || []
                }
                columns={achievementColumns}
                className={classes.table}
                sortModel={sortModel}
                autoPageSize
                disableColumnReorder
                hideFooterPagination
              />,
              <DataGrid
                autoHeight
                rows={
                  state?.userDetail?.rewards?.map((a, i) => {
                    var b: any = a;
                    b.id = i;
                    return b;
                  }) || []
                }
                columns={rewardColumns}
                className={classes.table}
                sortModel={sortModel}
                autoPageSize
                disableColumnReorder
                hideFooterPagination
              />,
            ]}></Tabs>
        </div>
      ) : (
        <div>
          <CircularProgress color="inherit" />
        </div>
      )}
      <Snackbar open={open} autoHideDuration={6000} onClose={handleClose}>
        <Alert onClose={handleClose} severity={snackbar?.severity}>
          {snackbar?.message}
        </Alert>
      </Snackbar>
    </>
  );
};

export const UserDetail = withRouter(UserDetailComponent);
