import React, { useState, useEffect } from 'react';
import { useGlobalState } from 'lightweight-globalstate';
import { State } from 'store';
import { StaffClient, StaffDto } from 'services';
import { useAuthenticatedClient } from 'hooks';
import { useParams, withRouter } from 'react-router-dom';
import { fetchData } from 'utils';
import { Button, CircularProgress } from '@material-ui/core';
import EditableField from './components/EditableField';

export interface RouteParams {
    name: string;
}

const ProfileDetailComponent = () => {
    const [loading, setLoading] = useState(true);

    const [state, updateState] = useGlobalState<State>();
    const client: StaffClient = useAuthenticatedClient<StaffClient>(state.staffClient, state.token);

    const { name } = useParams<RouteParams>();

    const getProfile = async () => {
        setLoading(true);
        const response = await fetchData<StaffDto>(() => client.getStaffMemberProfile(name));
        setLoading(false);
        response && updateState({ profileDetail: response });
    };

    useEffect(() => {
        setLoading(true);
        client && getProfile();
    }, [client]);

    const saveChanges = () => {
        return '';
    }

    return <div>
        {!loading && state.profileDetail ? (
            <div style={{display: 'flex'}}>
                <img src={state.profileDetail.profilePhoto} style={{width: 247.5, height: 495}}/>
                <div style={{padding: '10px', width: '100%'}}>
                    <div style={{display: 'flex', justifyContent: 'space-between'}}>
                        <EditableField label="Name" value={state.profileDetail.name} style={{ width: '100%' }} />
                        <EditableField label="Title" value={state.profileDetail.title} style={{ width: '100%' }} />
                    </div>
                    <div style={{display: 'flex', justifyContent: 'space-between'}}>
                        <EditableField label="Email" value={state.profileDetail.email} style={{ width: '100%' }} />
                        <EditableField label="Twitter handle" value={state.profileDetail.twitterUsername} style={{ width: '100%' }} />
                    </div>
                    <EditableField label="Profile" value={state.profileDetail.profile} style={{ width: '100%' }} type="multiline" />
                    <Button variant="contained" style={{ margin: '10px' }} onClick={() => saveChanges()}>Save Changes</Button>
                </div>
            </div>
        ) : (
        <div>
            <CircularProgress color="inherit" />
        </div>
        )}
    </div>
}

export const ProfileDetail = withRouter(ProfileDetailComponent);