import React, { useState, useEffect, PropsWithChildren } from 'react';
import { useGlobalState } from 'lightweight-globalstate';
import { State } from 'store';
import { StaffClient, StaffDto } from 'services';
import { useAuthenticatedClient } from 'hooks';
import { useParams, withRouter, RouteComponentProps } from 'react-router-dom';
import { fetchData } from 'utils';
import { Button, Chip, CircularProgress, Typography, TextField } from '@material-ui/core';
import { Add } from '@material-ui/icons';
import EditableField from './components/EditableField';
import { ResponsiveDialog } from '../../components';

export interface RouteParams {
    name: string;
}

const ProfileDetailComponent = (props: PropsWithChildren<RouteComponentProps>) => {
    const [state, updateState] = useGlobalState<State>();
    const client: StaffClient = useAuthenticatedClient<StaffClient>(state.staffClient, state.token);
    const { name } = useParams<RouteParams>();
    const { history } = props;

    const [loading, setLoading] = useState(true);
    const [changesMade, setChangesMade] = useState(false);
    const [showAddSkillModal, setShowAddSkillModal] = useState(false);
    const [newSkill, setNewSkill] = useState('');

    useEffect(() => {
        (async () => {
            const response = await fetchData<StaffDto>(() => client.getStaffMemberProfile(name));
            setLoading(false);
            updateState({ profileDetail: response });
        })();
    }, [client, name, updateState]);

    const handleValueChange = (label: string, newValue: string) => {
        var profile = state.profileDetail;
        (profile as any)[label] = newValue;
        updateState({ profileDetail: profile });
        setChangesMade(true);
    }

    const saveChanges = async () => {
        if(changesMade) {
            await client.upsertStaffMemberProfile(state.profileDetail);
            setChangesMade(false);
        }
    }

    const remove = async () => {
        await client.deleteStaffMemberProfile(state.profileDetail);
        history.push(`/profiles`);
    }

    const handleSkillsDelete = async (skill: string) => {
        var profile = state.profileDetail;
        if(profile.skills && profile.skills.length > 0) {
            profile.skills = profile.skills.filter(x => x !== skill);
            updateState({ profileDetail: profile });
            setChangesMade(true);
        }
    }

    const handleSkillAdd = () => {
        var profile = state.profileDetail;
        if(profile.skills) {
            profile.skills.push(newSkill);
            updateState({ profileDetail: profile });
            setChangesMade(true);
        }
        setNewSkill('');
        setShowAddSkillModal(false);
    }

    return <div>
        {!loading && state.profileDetail ? (
            <div style={{ display: 'flex' }}>
                <img src={state.profileDetail.profilePhoto} style={{width: 247.5, height: 495}} alt="profile" />
                <div style={{ paddingLeft: '10px', width: '100%' }}>
                    <ResponsiveDialog title={`Add Skill`} open={showAddSkillModal} handleClose={() => setShowAddSkillModal(false)}
                        actions={<>
                            <Button onClick={() => setShowAddSkillModal(false)} color="primary" autoFocus>
                                Cancel
                            </Button>
                            <Button onClick={() => handleSkillAdd()} color="primary" autoFocus >
                                Add
                            </Button>
                        </>}>
                        <TextField id="skill" variant="outlined" label="Skill" onChange={e => setNewSkill(e.target.value )} style={{ margin: '10px 0' }} />
                    </ResponsiveDialog>
                    <div style={{ display: 'flex' }}>
                        <EditableField name="name" label="Name" value={state.profileDetail.name} style={{ width: '100%' }} onChange={handleValueChange} />
                        <EditableField name="title" label="Title" value={state.profileDetail.title} style={{ width: '100%' }} onChange={handleValueChange} />
                    </div>
                    <div style={{display: 'flex', justifyContent: 'space-between'}}>
                        <EditableField name="email" label="Email" value={state.profileDetail.email || ''} style={{ width: '100%' }} onChange={handleValueChange} />
                        <EditableField name="twitterUsername" label="Twitter handle" value={state.profileDetail.twitterUsername || ''} style={{ width: '100%' }} onChange={handleValueChange} />
                    </div>
                    <EditableField name="profilePhoto" label="Profile Photo" value={state.profileDetail.profilePhoto || ''} style={{ width: '100%' }} onChange={handleValueChange} />
                    <EditableField name="profile" label="Profile" value={state.profileDetail.profile || ''} style={{ width: '100%' }} type="multiline" onChange={handleValueChange} />
                    <div style={{ width: '100%', padding: '10px' }}>
                        <Typography variant="body1" style={{margin: '10px 0 3px 5px'}}>Skills</Typography>
                        <div>
                            {state.profileDetail.skills && state.profileDetail.skills.length > 0 && (
                                state.profileDetail.skills.map((skill, i) => <Chip key={i} variant='outlined' label={skill} style={{ margin: '0 3px 3px'}} onDelete={() => handleSkillsDelete(skill)} />)
                            )}
                            <Chip icon={<Add />} label="Add Skill" style={{ margin: '0 3px 3px'}} onClick={() => setShowAddSkillModal(true)} />
                        </div>
                    </div>
                    <Button variant="contained" color={changesMade ? 'primary' : 'default'} style={{ margin: '10px' }} onClick={() => saveChanges()}>Save Changes</Button>
                    <Button variant="contained" color='primary' style={{ margin: '10px' }} onClick={() => remove()}>Remove</Button>
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