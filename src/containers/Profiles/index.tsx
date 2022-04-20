import { Box, Checkbox, TextField, Typography } from "@material-ui/core";
import React, { PropsWithChildren, useEffect } from "react";
import {
  StaffClient,
  StaffDto,
  StaffListViewModel,
  UpsertStaffMemberProfileCommand,
} from "services";

import AddProfile from "./components/AddProfile";
import { ProfileTableRow } from "./components/ProfileTableRow";
import { RouteComponentProps } from "react-router-dom";
import { State } from "store";
import { Table } from "components";
import { fetchData } from "utils";
import { useAuthenticatedClient } from "hooks";
import { useGlobalState } from "lightweight-globalstate";

const Profiles = (
  props: PropsWithChildren<RouteComponentProps>
): JSX.Element => {
  const { history } = props;

  const [staffList, setStaffList] = React.useState<StaffDto[]>([]);
  const [showArchived, setShowArchived] = React.useState(false);
  const [state, updateState] = useGlobalState<State>();
  const client: StaffClient = useAuthenticatedClient<StaffClient>(
    state.staffClient,
    state.token
  );

  const getProfiles = async () => {
    const response = await fetchData<StaffListViewModel>(() => client.get());
    if (response && response.staff) {
      let profiles = response.staff.sort((a, b) =>
        (a.name as string) > (b.name as string) ? 1 : -1
      );
      updateState({ staffProfiles: profiles, staffProfilesDefault: profiles });
      setStaffList(profiles.filter((p) => !p.isDeleted));
    }
  };

  const goToUser = (id: number | undefined) => {
    history.push(`/profiles/${id}`);
  };

  useEffect(() => {
    client && getProfiles();
  }, [client]);

  const addProfile = async (staffMember: UpsertStaffMemberProfileCommand) => {
    const response = await fetchData<StaffDto>(() =>
      client.upsertStaffMemberProfile(staffMember)
    );
    response && getProfiles();
  };

  const filterProfiles = (value: string | undefined) => {
    let filteredProfiles;
    if (value) {
      // Filter by the search value, Can show archived profiles
      filteredProfiles = state.staffProfilesDefault?.filter((u) => {
        return u.name?.toLowerCase().includes(value.toLowerCase());
      });
    } else {
      // Show profiles based on showArchived
      filteredProfiles = state.staffProfilesDefault?.filter((u) => {
        return u.isDeleted === !showArchived;
      });
    }

    setStaffList(filteredProfiles);
  };

  const handleArchivedChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setShowArchived(event.target.checked);
    filterProfiles(undefined);
  };

  return (
    <>
      <h1>Profiles</h1>
      <Typography>All staff profiles available for editing</Typography>
      <div
        style={{ display: "flex", justifyContent: "center", padding: "5px" }}
      >
        <TextField
          label="Search"
          onChange={(e) => filterProfiles(e.target.value)}
          style={{ flexGrow: 1 }}
        />
      </div>
      <AddProfile addProfile={(v) => addProfile(v)} />

      <Box padding={2}>
        <Checkbox checked={showArchived} onChange={handleArchivedChange} />
        Show Archived Profiles
      </Box>

      <Table items={["Name", "Title"]}>
        {staffList.map((r, i) => (
          <ProfileTableRow
            key={i}
            profile={r}
            onClick={() => {
              goToUser(r?.id);
            }}
          />
        ))}
      </Table>
    </>
  );
};

export default Profiles;
