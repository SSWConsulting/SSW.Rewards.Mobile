import React from 'react';
import { User } from 'models/index';
import TableRow from '@material-ui/core/TableRow';
import TableCell from '@material-ui/core/TableCell';
import Avatar from '@material-ui/core/Avatar';
import { LeaderboardUserDto } from 'services';
import Skeleton from '@material-ui/lab/Skeleton';

interface LeaderboardTableRowProps {
    user: LeaderboardUserDto;
}

export const LeaderboardTableRow = (props: LeaderboardTableRowProps) => {
    const { user: { rank, name, points , profilePic} } = props;

    const getName = () => {
        if(name){
        return name.split(" ")[0][0] + name.split(" ")[1][0];
        }
    }
    return (
        <TableRow>
            <TableCell>{rank}</TableCell>
            <TableCell><Avatar alt={name} src={profilePic}>{getName()}</Avatar></TableCell>
            <TableCell>{name}</TableCell>
            <TableCell>{points}</TableCell>
        </TableRow>
    )
}

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
}