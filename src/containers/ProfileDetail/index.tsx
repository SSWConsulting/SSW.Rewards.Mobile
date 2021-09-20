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
    const [state, updateState] = useGlobalState<State>();
    const client: StaffClient = useAuthenticatedClient<StaffClient>(state.staffClient, state.token);
    const { name } = useParams<RouteParams>();

    const [loading, setLoading] = useState(true);

    useEffect(() => {
        (async () => {
            const response = await fetchData<StaffDto>(() => client.getStaffMemberProfile(name));
            setLoading(false);
            updateState({ profileDetail: response });
        })();
    }, [client]);

    const handleValueChange = (label: string, newValue: string) => {
        var profile = state.profileDetail;
        (profile as any)[label] = newValue;
        updateState({ profileDetail: profile });
    }

    const saveChanges = async () => {
        await client.upsertStaffMemberProfile(state.profileDetail);
    }

    return <div>
        {!loading && state.profileDetail ? (
            <div style={{ display: 'flex' }}>
                <img src={state.profileDetail.profilePhoto} style={{width: 247.5, height: 495}}/>
                <div style={{ paddingLeft: '10px', width: '99%' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between'}}>
                        <EditableField name="name" label="Name" value={state.profileDetail.name} style={{ width: '100%' }} onChange={handleValueChange} />
                        <EditableField name="title" label="Title" value={state.profileDetail.title} style={{ width: '100%' }} onChange={handleValueChange} />
                    </div>
                    <div style={{display: 'flex', justifyContent: 'space-between'}}>
                        <EditableField name="email" label="Email" value={state.profileDetail.email} style={{ width: '100%' }} onChange={handleValueChange} />
                        <EditableField name="twitterUsername" label="Twitter handle" value={state.profileDetail.twitterUsername} style={{ width: '100%' }} onChange={handleValueChange} />
                    </div>
                    <EditableField name="profile" label="Profile" value={state.profileDetail.profile} style={{ width: '100%' }} type="multiline" onChange={handleValueChange} />
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