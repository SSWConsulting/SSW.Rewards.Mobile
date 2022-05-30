import React, { useState } from "react";
import TableRow from "@material-ui/core/TableRow";
import TableCell from "@material-ui/core/TableCell";
import { AchievementAdminViewModel } from "services";
import Code from "qrcode.react";
import { ResponsiveDialog } from "components";
import Skeleton from "@material-ui/lab/Skeleton";
import { Box } from "@material-ui/core";

interface AchievementTableRowProps {
  achievement: AchievementAdminViewModel;
}

export const AchievementTableRow = (props: AchievementTableRowProps) => {
  const {
    achievement: { name, value, code }
  } = props;

  const [showdal, setShowdal] = useState(false);

  return (
    <>
      <ResponsiveDialog
        title={`${name} - ${value} - ${code}`}
        open={showdal}
        handleClose={() => setShowdal(false)}>
        {code && (
          <Code
            style={{ width: "400px", height: "400px" }}
            size={400}
            value={code}
          />
        )}
      </ResponsiveDialog>
      <TableRow 
        style={{'cursor': 'pointer'}}
        onClick={() => setShowdal(true)}>
        <TableCell align="left">
          {code && (
            <Box>
            <Code
              style={{ width: "80px", height: "80px" }}
              size={400}
              value={code}
            />
          </Box>
          )}
        </TableCell>
        <TableCell align="left">{name}</TableCell>
        <TableCell>{value}</TableCell>
      </TableRow>
    </>
  );
};

export const SkeletonRow = () => {
  return (
    <TableRow>
      <TableCell align="left">
          <Skeleton animation="wave" variant="rect" width={80} height={80} />
      </TableCell>
      <TableCell align="left">
        <Skeleton animation="wave" variant="text" width={200} />
      </TableCell>
      <TableCell>
        <Skeleton animation="wave" variant="text" width={20} />
      </TableCell>
    </TableRow>
  );
};
