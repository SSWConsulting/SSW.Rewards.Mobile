import React, { useEffect } from "react";
import { useGlobalState } from "lightweight-globalstate";
import { State } from "store";
import {
  AchievementClient,
  AchievementAdminListViewModel,
  AchievementAdminViewModel,
  ICreateAchievementCommand
} from "services";
import { fetchData, renderComponentXTimes } from "utils";
import { useAuthenticatedClient } from "hooks";
import { AchievementTableRow, SkeletonRow, AddAchievement } from "./components";
import { Table } from "components";

const Achievements = (): JSX.Element => {
  const [state, updateState] = useGlobalState<State>();
  const client = useAuthenticatedClient<AchievementClient>(
    state.achievementClient,
    state.token
  );

  const getAchievements = async () => {
    const response = await fetchData<AchievementAdminListViewModel>(() =>
      client.adminList()
    );
    response && response.achievements && updateState({ achievements: response.achievements });
  };

  const addAchievement = async (values: ICreateAchievementCommand) => {
    const response = await fetchData<AchievementAdminViewModel>(() =>
      client.create(values)
    );
    response &&
      updateState({ achievements: [...state.achievements, response] });
  }; 

  useEffect(() => {
    client && getAchievements();
  }, [client]);

  return (
    <>
      <AddAchievement addAchievement={(v) => addAchievement(v)} />
      <Table items={["Code", "Name", "Value"]}>
        {state.achievements &&
          state.achievements.map((a: AchievementAdminViewModel) => (
            <AchievementTableRow
              key={a.id}
              achievement={a}></AchievementTableRow>
          ))}
        {!state.achievements && renderComponentXTimes(SkeletonRow, 20)}
      </Table>
    </>
  );
};

export default Achievements;
