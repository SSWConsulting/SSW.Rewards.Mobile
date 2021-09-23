import React, { useState, useEffect, PropsWithChildren } from 'react';
import { useGlobalState } from 'lightweight-globalstate';
import { State } from 'store';
import { FileParameter, SkillClient, SkillListViewModel, StaffClient, StaffDto } from 'services';
import { useAuthenticatedClient } from 'hooks';
import { useParams, withRouter, RouteComponentProps } from 'react-router-dom';
import { fetchData } from 'utils';
import { Button, Chip, CircularProgress, FormControl, Input, InputLabel, MenuItem, Select, Typography } from '@material-ui/core';
import { Add } from '@material-ui/icons';
import EditableField from './components/EditableField';
import { ResponsiveDialog } from '../../components';

export interface RouteParams {
    name: string;
}

const ProfileDetailComponent = (props: PropsWithChildren<RouteComponentProps>) => {
    const [state, updateState] = useGlobalState<State>();
    const client: StaffClient = useAuthenticatedClient<StaffClient>(state.staffClient, state.token);
    const skillClient: SkillClient = useAuthenticatedClient<SkillClient>(state.skillClient, state.token);
    const { name } = useParams<RouteParams>();
    const { history } = props;

    const [skills, setSkills] = useState(['']);
    const [selectedSkill, setSelectedSkill] = useState('');
    const [profilePhotoFile, setProfilePhotoFile] = useState(null);
    const [profilePhotoFileName, setProfilePhotoFileName] = useState('')

    const [loading, setLoading] = useState(true);
    const [changesMade, setChangesMade] = useState(false);
    const [showAddSkillModal, setShowAddSkillModal] = useState(false);

    useEffect(() => {
        (async () => {
            const response = await fetchData<StaffDto>(() => client.getStaffMemberProfile(name));
            setLoading(false);
            updateState({ profileDetail: response });
        })();
    }, [client, skillClient, name, updateState]);

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
            profile.skills.push(selectedSkill);
            updateState({ profileDetail: profile });
            setChangesMade(true);
        }
        setShowAddSkillModal(false);
    }

    const handleAddSkillButtonClick = async () => {
        setShowAddSkillModal(true);
        var skills = await fetchData<SkillListViewModel>(() => skillClient.get());
        if(skills && skills.skills) {
            setSkills(skills.skills.sort((a, b) => (a > b) ? 1 : -1));
        }
    }

    const handleChooseProfilePhoto = (file: any) => {
        setProfilePhotoFile(file);
        setProfilePhotoFileName(file.name);
    }

    const handleUploadProfilePhoto = async () => {
        let fileParameter: FileParameter = { data: profilePhotoFile, fileName: profilePhotoFileName };
        await client.uploadStaffMemberProfilePicture(fileParameter);
        const response = await fetchData<StaffDto>(() => client.getStaffMemberProfile(name));
        updateState({ profileDetail: response });
        setProfilePhotoFileName('');
        setProfilePhotoFile(null);
    }

    return <div>
        {!loading && state.profileDetail ? (
            <div style={{ display: 'flex' }}>
                <div>
                    <img src={state.profileDetail.profilePhoto} alt="profile" />
                    <label htmlFor="contained-button-file" style={{ display: 'flex', flexDirection: 'column' }}>
                        <Input id="contained-button-file" type="file" style={{padding: '10px'}} onChange={(e: any) => handleChooseProfilePhoto(e.target.files[0])} />
                        {profilePhotoFile && (
                            <Button variant="contained" style={{ margin: '10px' }} onClick={() => handleUploadProfilePhoto()}>
                                Upload
                            </Button>
                        )}
                    </label>
                </div>
                <div style={{ paddingLeft: '10px', width: '100%' }}>
                    <ResponsiveDialog title={`Add Skill`} open={showAddSkillModal} handleClose={() => setShowAddSkillModal(false)}
                        actions={<>
                            <Button onClick={() => setShowAddSkillModal(false)} color="primary" autoFocus>
                                Cancel
                            </Button>
                            <Button onClick={() => handleSkillAdd()} color="primary" autoFocus>
                                Add
                            </Button>
                        </>}>
                        <FormControl fullWidth>
                            <InputLabel id="skill-select-label">Skill</InputLabel>
                            <Select style={{ width: '600px', maxWidth: '100%' }}
                                labelId="skill-select-label"
                                id="skill-select"
                                value={selectedSkill}
                                label="Skill"
                                onChange={(e: any) => setSelectedSkill(e.target.value)}
                            >
                                {skills.length > 0 && (
                                    skills.map((skill: string) => <MenuItem key={skill} value={skill}>{skill}</MenuItem>)
                                )}
                            </Select>
                        </FormControl>
                    </ResponsiveDialog>
                    <div style={{ display: 'flex' }}>
                        <EditableField name="name" label="Name" value={state.profileDetail.name} style={{ width: '100%' }} onChange={handleValueChange} />
                        <EditableField name="title" label="Title" value={state.profileDetail.title} style={{ width: '100%' }} onChange={handleValueChange} />
                    </div>
                    <div style={{display: 'flex', justifyContent: 'space-between'}}>
                        <EditableField name="email" label="Email" value={state.profileDetail.email || ''} style={{ width: '100%' }} onChange={handleValueChange} />
                        <EditableField name="twitterUsername" label="Twitter handle" value={state.profileDetail.twitterUsername || ''} style={{ width: '100%' }} onChange={handleValueChange} />
                    </div>
                    <EditableField name="profile" label="Profile" value={state.profileDetail.profile || ''} style={{ width: '100%' }} type="multiline" onChange={handleValueChange} />
                    <div style={{ width: '100%', padding: '10px' }}>
                        <Typography variant="body1" style={{margin: '10px 0 3px 5px'}}>Skills</Typography>
                        <div>
                            {state.profileDetail.skills && state.profileDetail.skills.length > 0 && (
                                state.profileDetail.skills.map((skill, i) => <Chip key={i} variant='outlined' label={skill} style={{ margin: '0 3px 3px'}} onDelete={() => handleSkillsDelete(skill)} />)
                            )}
                            <Chip icon={<Add />} label="Add Skill" style={{ margin: '0 3px 3px'}} onClick={() => handleAddSkillButtonClick()} />
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