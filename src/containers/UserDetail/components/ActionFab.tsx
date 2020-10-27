import React, { useState, useEffect } from 'react';
import { theme } from 'config/theme';
import { ResponsiveDialog } from 'components';
import {
  UserViewModel,
  RewardClient,
  RewardAdminListViewModel,
  RewardAdminViewModel,
  AchievementClient,
  AchievementAdminListViewModel,
  AchievementAdminViewModel,
} from 'services';
import { useAuthenticatedClient } from 'hooks';
import { State } from 'store';
import { fetchData } from 'utils';
import { useGlobalState } from 'lightweight-globalstate';
import Autocomplete from '@material-ui/lab/Autocomplete';
import AddIcon from '@material-ui/icons/Add';
import TextField from '@material-ui/core/TextField';
import Button from '@material-ui/core/Button';
import Fab from '@material-ui/core/Fab';

interface ActionFabProps {
  user: UserViewModel;
  selectedTab: string;
  claimReward: (selectedReward: string) => void;
  claimAchievement: (selectedAchievement: string) => void;
}
export const ActionFab = (props: ActionFabProps) => {
  const [state] = useGlobalState<State>();
  const [showRewardModal, setShowRewardModal] = useState(false);
  const [showAchievementModal, setShowAchievementModal] = useState(false);
  const [rewards, setRewards] = useState<RewardAdminViewModel[] | undefined>([]);
  const [achievements, setAchievements] = useState<AchievementAdminViewModel[] | undefined>([]);
  const rewardClient = useAuthenticatedClient<RewardClient>(state.rewardClient, state.token);
  const achievementClient = useAuthenticatedClient<AchievementClient>(state.achievementClient, state.token);
  const [selectedReward, setSelectedReward] = useState('');
  const [selectedAchievement, setSelectedAchievement] = useState('');
  const { user, selectedTab, claimReward, claimAchievement } = props;

  const getRewards = async () => {
    const response: RewardAdminListViewModel = (await fetchData<RewardAdminListViewModel>(() =>
      rewardClient.adminList()
    )) as RewardAdminListViewModel;
    response && setRewards(response.rewards);
  };

  const getAchievements = async () => {
    const response: AchievementAdminListViewModel = (await fetchData<AchievementAdminListViewModel>(() =>
      achievementClient.adminList()
    )) as AchievementAdminListViewModel;
    response && setAchievements(response.achievements);
  };

  useEffect(() => {
    if (!showRewardModal) {
      return undefined;
    }
    getRewards();
  }, [showRewardModal]);

  useEffect(() => {
    if (!showAchievementModal) {
      return undefined;
    }
    getAchievements();
  }, [showAchievementModal]);

  return (
    <>
      <ResponsiveDialog
        title={`Assign a reward to user: ${user && user.fullName}`}
        open={showRewardModal}
        handleClose={() => setShowRewardModal(false)}
        actions={
          <>
            <Button onClick={() => setShowRewardModal(false)} color="primary" autoFocus>
              Cancel
            </Button>
            <Button
              disabled={selectedReward === ''}
              onClick={() => {
                setShowRewardModal(false);
                claimReward(selectedReward);
              }}
              color="primary"
              autoFocus>
              Confirm
            </Button>
          </>
        }>
        <p>
          <p>
            <b>Current Points: </b>
            {user && user.balance}
          </p>
          <Autocomplete
            style={{ width: '600px', maxWidth: '100%' }}
            onChange={(event: any, newValue: RewardAdminViewModel | null) => {
              var rewardCode = newValue?.code || '';
              setSelectedReward(rewardCode);
            }}
            getOptionDisabled={(option) => (option.cost as number) > (user.balance as number)}
            options={rewards as RewardAdminViewModel[]}
            getOptionLabel={(option) => `${option.name} (${option.cost})`}
            renderInput={(params) => <TextField {...params} label="Rewards" variant="outlined" />}
          />
        </p>
      </ResponsiveDialog>
      <ResponsiveDialog
        title="Assign an achievement"
        open={showAchievementModal}
        handleClose={() => setShowAchievementModal(false)}
        actions={
          <>
            <Button onClick={() => setShowAchievementModal(false)} color="primary" autoFocus>
              Cancel
            </Button>
            <Button
              disabled={selectedAchievement === ''}
              onClick={() => {
                setShowAchievementModal(false);
                claimAchievement(selectedAchievement);
              }}
              color="primary"
              autoFocus>
              Confirm
            </Button>
          </>
        }>
        <Autocomplete
          style={{ width: '600px', maxWidth: '100%' }}
          options={achievements as AchievementAdminViewModel[]}
          onChange={(event: any, newValue: AchievementAdminViewModel | null) => {
            var achievementCode = newValue?.code || '';
            setSelectedAchievement(achievementCode);
          }}
          getOptionLabel={(option) => `${option.name} (${option.value})`}
          renderInput={(params) => <TextField {...params} label="Achievements" variant="outlined" />}
        />
      </ResponsiveDialog>
      {selectedTab === 'Achievements' ? (
        <Fab
          onClick={() => setShowAchievementModal(true)}
          style={{
            position: 'absolute',
            bottom: 0,
            margin: theme.spacing(2),
            zIndex: 100,
          }}
          color="primary">
          <AddIcon />
        </Fab>
      ) : (
        <Fab
          onClick={() => setShowRewardModal(true)}
          style={{
            position: 'absolute',
            margin: theme.spacing(2),
            bottom: 0,
            zIndex: 100,
          }}
          color="primary">
          <AddIcon />
        </Fab>
      )}
    </>
  );
};
