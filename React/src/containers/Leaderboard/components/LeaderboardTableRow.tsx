import React from 'react';
import TableRow from '@material-ui/core/TableRow';
import TableCell from '@material-ui/core/TableCell';
import Avatar from '@material-ui/core/Avatar';
import { LeaderboardUserDto } from 'services';
import Skeleton from '@material-ui/lab/Skeleton';

interface LeaderboardTableRowProps {
  user: LeaderboardUserDto;
  onClick: Function;
}

export const LeaderboardTableRow = (props: LeaderboardTableRowProps) => {
  const {
    user: { userId, rank, name, points, profilePic },
    onClick,
  } = props;

  const getName = () => {
    if (name) {
      return name.split(' ')[0][0] + name.split(' ')[1][0];
    }
  };

  return (
    <>
      <TableRow style={{ cursor: 'pointer' }} onClick={() => onClick(userId)}>
        <TableCell>{rank}</TableCell>
        <TableCell style={{ display: 'flex', alignItems: 'center' }}>
          <Avatar alt={name} src={profilePic} style={{ marginRight: '10px' }}>
            {getName()}
          </Avatar>
          <span>{name}</span>
        </TableCell>
        <TableCell>{points}</TableCell>
      </TableRow>
    </>
  );
};

export const SkeletonRow = () => {
  return (
    <TableRow>
      <TableCell>
        <Skeleton animation="wave" variant="text" width={20} />
      </TableCell>
      <TableCell>
        <Skeleton animation="wave" variant="circle" width={42} height={42} />
      </TableCell>
      <TableCell>
        <Skeleton animation="wave" variant="text" width={200} />
      </TableCell>
      <TableCell>
        <Skeleton animation="wave" variant="text" width={20} />
      </TableCell>
    </TableRow>
  );
};
