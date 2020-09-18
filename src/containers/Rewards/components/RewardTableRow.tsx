import React, { useState } from "react";
import TableRow from "@material-ui/core/TableRow";
import TableCell from "@material-ui/core/TableCell";
import { RewardAdminViewModel } from "services";
import Skeleton from "@material-ui/lab/Skeleton";
import { Box } from "@material-ui/core";
import Code from "qrcode.react";

interface RewardTableRowProps {
  reward: RewardAdminViewModel;
}

export const RewardTableRow = (props: RewardTableRowProps) => {
  const {
    reward: { name, code },
  } = props;

  return (
    <>
      <TableRow>
        <TableCell>
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
        <TableCell>{name}</TableCell>
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
    </TableRow>
  );
};
