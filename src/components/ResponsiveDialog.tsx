import React, { PropsWithChildren } from "react";
import Button from "@material-ui/core/Button";
import Dialog from "@material-ui/core/Dialog";
import DialogActions from "@material-ui/core/DialogActions";
import DialogContent from "@material-ui/core/DialogContent";
import DialogContentText from "@material-ui/core/DialogContentText";
import DialogTitle from "@material-ui/core/DialogTitle";
import useMediaQuery from "@material-ui/core/useMediaQuery";
import { useTheme } from "@material-ui/core/styles";

interface DialogProps{
  title: string;
  open: boolean;
  handleClose: () => void;
}

export const ResponsiveDialog = (props: PropsWithChildren<DialogProps>) => {
         const theme = useTheme();
         const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));

         return (
           <div>
             <Dialog
               fullScreen={fullScreen}
               open={props.open}
               onClose={props.handleClose}
               aria-labelledby="responsive-dialog-title">
               <DialogTitle style={fullScreen ?{paddingTop:'80px'} : {}} id="responsive-dialog-title">
                 {props.title}
               </DialogTitle>
               <DialogContent>
                <DialogContentText>
                  
                 </DialogContentText>
                 {props.children}
               </DialogContent>
               <DialogActions>
                 <Button onClick={props.handleClose} color="primary" autoFocus>
                   Ok
                 </Button>
               </DialogActions>
             </Dialog>
           </div>
         );
       };
