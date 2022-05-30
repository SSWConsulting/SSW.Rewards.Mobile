import React, {useState} from "react";
import { Fab, Button } from "@material-ui/core";
import AddIcon from "@material-ui/icons/Add";
import { theme } from "../../../config/theme";
import { ResponsiveDialog } from '../../../components';
import { ICreateAchievementCommand  } from "services";
import { TextField } from '@material-ui/core';

interface AddAchievementProps {
  addAchievement : (values: ICreateAchievementCommand ) => void
}
export const AddAchievement = (props: AddAchievementProps) => {

  const [showModal,setShowModal] = useState(false);
  const [newAchievement,setNewAchievement] = useState({} as ICreateAchievementCommand );
  const formValid = newAchievement && newAchievement.name && newAchievement.value && newAchievement.name !== '' && newAchievement.value > 0;

  return (
    <>
      <ResponsiveDialog
        title={`Add Achievement`}
        open={showModal}
        handleClose={() => setShowModal(false)}
        actions={<>
          <Button onClick={() => setShowModal(false)} color="primary" autoFocus>
            Cancel
          </Button>
          <Button
            disabled={!formValid}
            onClick={() => {
              setShowModal(false);
              props.addAchievement(newAchievement);
            }}
            color="primary"
            autoFocus>
            Save
          </Button>
        </>}>
        <TextField
          id="name"
          label="Achievement Name"
          onChange={e =>
            setNewAchievement({ ...newAchievement, name: e.target.value })
          }
        />
        <TextField
          id="value"
          label="Value"
          type="number"
          onChange={e =>
            setNewAchievement({
              ...newAchievement,
              value: Number(e.target.value)
            })
          }
        />
      </ResponsiveDialog>
      <Fab
        onClick={() => setShowModal(true)}
        style={{
          position: "absolute",
          bottom: 0,
          right: 0,
          margin: theme.spacing(2)
        }}
        color="primary"
        aria-label="add-achievement">
        <AddIcon />
      </Fab>
    </>
  );
}
