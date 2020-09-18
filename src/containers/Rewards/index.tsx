import React, { useEffect } from "react";
import { Table } from "components";
import { useGlobalState } from "lightweight-globalstate";
import { State } from "store";
import { TableCell, TableRow } from "@material-ui/core";
import { fetchData } from "utils";
import { RewardAdminListViewModel, RewardClient } from "services";
import { useAuthenticatedClient } from "hooks";
import { getTrailingCommentRanges } from "typescript";
import { RewardTableRow } from "./components/RewardTableRow";

const data = [
  { id: 1, code: "test", name: "Xiao Mi Band" },
  { id: 2, code: "test2", name: "Water Bottle" },
];

const Rewards = (): JSX.Element => {
  const [state, updateState] = useGlobalState<State>();
  const client: RewardClient = useAuthenticatedClient<RewardClient>(
    state.rewardClient,
    state.token
  );

  const getRewards = async () => {
    const response = await fetchData<RewardAdminListViewModel>(() =>
      client.adminList()
    );
    response && response.rewards && updateState({ rewards: response.rewards });
  };

  useEffect(() => {
    client && getRewards();
  }, [client]);

  return (
    <>
      <Table items={["Code", "Name"]}>
        {state.rewards &&
          state.rewards.map((r) => <RewardTableRow key={r.id} reward={r} />)}
      </Table>
    </>
  );
};

export default Rewards;
