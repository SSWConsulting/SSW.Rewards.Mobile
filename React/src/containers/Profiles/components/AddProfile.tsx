import { Button, Fab } from "@material-ui/core";
import React, { useState } from "react";

import AddIcon from "@material-ui/icons/Add";
import { ResponsiveDialog } from "../../../components";
import { TextField } from "@material-ui/core";
import { UpsertStaffMemberProfileCommand } from "services";
import { theme } from "../../../config/theme";

interface AddProfileProps {
  addProfile: (values: UpsertStaffMemberProfileCommand) => void;
}

const AddProfile = (props: AddProfileProps) => {
  const [showModal, setShowModal] = useState(false);
  const [newProfile, setNewProfile] = useState({} as any);

  return (
    <>
      <ResponsiveDialog
        title={`Add Profile`}
        open={showModal}
        handleClose={() => setShowModal(false)}
        actions={
          <>
            <Button
              onClick={() => setShowModal(false)}
              color="primary"
              autoFocus
            >
              Cancel
            </Button>
            <Button
              onClick={() => {
                if (newProfile.name) {
                  setShowModal(false);
                  props.addProfile(
                    newProfile as UpsertStaffMemberProfileCommand
                  );
                }
              }}
              color="primary"
              autoFocus
            >
              Save
            </Button>
          </>
        }
      >
        <div style={{ display: "flex", flexDirection: "column" }}>
          <TextField
            id="name"
            variant="outlined"
            label="Name"
            required
            error={!newProfile?.name}
            onChange={(e) =>
              setNewProfile({ ...newProfile, name: e.target.value })
            }
            style={{ width: "600px", maxWidth: "100%", margin: "10px 0" }}
          />
          <TextField
            id="title"
            variant="outlined"
            label="Title"
            onChange={(e) =>
              setNewProfile({ ...newProfile, title: e.target.value })
            }
            style={{ width: "600px", maxWidth: "100%", margin: "10px 0" }}
          />
        </div>
      </ResponsiveDialog>
      <Fab
        onClick={() => setShowModal(true)}
        style={{
          position: "absolute",
          bottom: 0,
          right: 0,
          margin: theme.spacing(2),
        }}
        color="primary"
        aria-label="add-Profile"
      >
        <AddIcon />
      </Fab>
    </>
  );
};

export default AddProfile;
