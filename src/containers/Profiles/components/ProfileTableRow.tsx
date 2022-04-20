import React from "react";
import Skeleton from "@material-ui/lab/Skeleton";
import { StaffDto } from "services";
import TableCell from "@material-ui/core/TableCell";
import TableRow from "@material-ui/core/TableRow";

interface ProfileTableRowProps {
  profile: StaffDto;
  onClick: Function;
}

export const ProfileTableRow = (props: ProfileTableRowProps) => {
  const {
    profile: { name, title },
    onClick,
  } = props;

  return (
    <TableRow
      style={{ cursor: "pointer", textAlign: "center" }}
      onClick={() => onClick(name)}
    >
      <TableCell>{name}</TableCell>
      <TableCell>{title}</TableCell>
    </TableRow>
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
      <TableCell align="left">
        <Skeleton animation="wave" variant="text" width={200} />
      </TableCell>
    </TableRow>
  );
};
