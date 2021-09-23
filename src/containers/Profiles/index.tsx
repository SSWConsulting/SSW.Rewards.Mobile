import React, { useEffect, PropsWithChildren } from 'react';
import { Table } from 'components';
import { useGlobalState } from 'lightweight-globalstate';
import { State } from 'store';
import { TextField, Typography } from '@material-ui/core';
import { fetchData } from 'utils';
import { StaffListViewModel, StaffClient, StaffDto, UpsertStaffMemberProfileCommand } from 'services';
import { useAuthenticatedClient } from 'hooks';
import { ProfileTableRow } from './components/ProfileTableRow';
import { RouteComponentProps } from 'react-router-dom';
import AddProfile from './components/AddProfile';

const Profiles = (props: PropsWithChildren<RouteComponentProps>): JSX.Element => {
    const { history } = props;

    const [state, updateState] = useGlobalState<State>();
    const client: StaffClient = useAuthenticatedClient<StaffClient>(state.staffClient, state.token);

    const getProfiles = async () => {
        const response = await fetchData<StaffListViewModel>(() => client.get());
        if(response && response.staff) {
            let profiles = response.staff.sort((a, b) => ((a.name as string) > (b.name as string)) ? 1 : -1);
            updateState({ staffProfiles: profiles, staffProfilesDefault: profiles });
        }
    };

    const goToUser = (name: string) => {
        history.push(`/profiles/${name}`);
    };

    useEffect(() => {
        client && getProfiles();
    }, [client]);

    const addProfile = async (staffMember: UpsertStaffMemberProfileCommand) => {
        const response = await fetchData<StaffDto>(() => client.upsertStaffMemberProfile(staffMember));
        response && getProfiles();
    };

    const filterProfiles = (value: string) => {
        const filteredProfiles = state.staffProfilesDefault.filter((u) => {
          return u.name?.toLowerCase().includes(value.toLowerCase());
        });
    
        filteredProfiles && updateState({ staffProfiles: filteredProfiles });
      };

    return (
        <>
            <h1>Profiles</h1>
            <Typography>All staff profiles available for editing</Typography>
            <div style={{ display: 'flex', justifyContent: 'center', padding: '5px' }}>
                <TextField label="Search" onChange={(e) => filterProfiles(e.target.value)} style={{ flexGrow: 1 }} />
            </div>
            <AddProfile addProfile={(v) => addProfile(v)} />
            <Table items={['Name', 'Title']}>
                {state.staffProfiles && state.staffProfiles.length > 0 && 
                    state.staffProfiles.map((r, i) => <ProfileTableRow key={i} profile={r} onClick={goToUser} />)}
            </Table>
        </>
    );
};

export default Profiles;
