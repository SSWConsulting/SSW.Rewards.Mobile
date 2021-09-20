import React from 'react';
import { useGlobalState } from 'lightweight-globalstate';
import { State } from 'store';
import { StaffListViewModel, StaffClient } from 'services';
import { useAuthenticatedClient } from 'hooks';
import { useParams, withRouter } from 'react-router-dom';
import { fetchData } from 'utils';

const ProfileDetailComponent = () => {
    const [state, updateState] = useGlobalState<State>();
    const client: StaffClient = useAuthenticatedClient<StaffClient>(state.staffClient, state.token);

    const getProfiles = async () => {
        const response = await fetchData<StaffListViewModel>(() => client.get());
        response && response.staff && updateState({ staffProfiles: response.staff });
    };
    return <></>
}

export const ProfileDetail = withRouter(ProfileDetailComponent);