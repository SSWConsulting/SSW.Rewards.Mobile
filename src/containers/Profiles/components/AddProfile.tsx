import React, { useState } from "react";
import { Fab, Button } from "@material-ui/core";
import AddIcon from "@material-ui/icons/Add";
import { theme } from "../../../config/theme";
import { ResponsiveDialog } from '../../../components';
import { AddStaffMemberProfileCommand } from "services";
import { TextField } from '@material-ui/core';

interface AddProfileProps {
    addProfile: (values: AddStaffMemberProfileCommand) => void
}

const AddProfile = (props: AddProfileProps) => {
    const [showModal, setShowModal] = useState(false);
    const [newProfile, setNewProfile] = useState({});

    return (
        <>
            <ResponsiveDialog title={`Add Profile`} open={showModal} handleClose={() => setShowModal(false)}
                actions={<>
                    <Button onClick={() => setShowModal(false)} color="primary" autoFocus>
                        Cancel
                    </Button>
                    <Button onClick={() => { setShowModal(false); props.addProfile(newProfile as AddStaffMemberProfileCommand); }} color="primary" autoFocus >
                        Save
                    </Button>
                </>}>
                <div style={{ display: 'flex', flexDirection: 'column' }}>
                    <TextField id="name" variant="outlined" label="Name" onChange={e => setNewProfile({ ...newProfile, name: e.target.value })} style={{ margin: '10px 0' }} />
                    <TextField id="email" variant="outlined" label="Email" onChange={e => setNewProfile({ ...newProfile, email: e.target.value })} style={{ margin: '10px 0' }} />
                    <TextField id="title" variant="outlined" label="Title" onChange={e => setNewProfile({ ...newProfile, title: e.target.value })} style={{ margin: '10px 0' }} />
                    <TextField id="profile" variant="outlined" rows='4' label="Profile" multiline onChange={e => setNewProfile({ ...newProfile, profile: e.target.value })} style={{ margin: '10px 0' }} />
                </div>
            </ResponsiveDialog>
            <Fab onClick={() => setShowModal(true)}
                style={{
                    position: "absolute",
                    bottom: 0,
                    right: 0,
                    margin: theme.spacing(2)
                }} color="primary" aria-label="add-Profile">
                <AddIcon />
            </Fab>
        </>
    );
}

export default AddProfile;