import React from 'react';
import { User } from 'models/index';

import TableRow from '@material-ui/core/TableRow';
import TableCell from '@material-ui/core/TableCell';
import Avatar from '@material-ui/core/Avatar';

interface LeaderboardTableRowProps {
    user:User
}

const LeaderboardTableRow = (props: LeaderboardTableRowProps) => {
    return (
        <TableRow>
            <TableCell>{props.user.rank}</TableCell>
            <TableCell><Avatar alt={props.user.name} src={props.user.profilePic}>{props.user.name.split(' ')[0][0] + props.user.name.split(' ')[1][0]}</Avatar></TableCell>
            <TableCell>{props.user.name}</TableCell>
            <TableCell>{props.user.points}</TableCell>
        </TableRow>
    )
}

export default LeaderboardTableRow;