import React from 'react'
import { User } from 'models/index';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import LeaderboardTableRow from './LeaderboardTableRow';

interface LeaderBoardTableProps {
    users: User[]
}

const LeaderboardTable = (props:LeaderBoardTableProps) => {
    return (
        <Table>
            <TableHead>
                <TableRow>
                    <TableCell>Rank</TableCell>
                    <TableCell>Picture</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Points</TableCell>
                </TableRow>
            </TableHead>
            <TableBody>
            {props.users.map((user:User) =>
                <LeaderboardTableRow key={user.userId} user={user}></LeaderboardTableRow>
            )}
            </TableBody>
        </Table>
    )
}
 
export default LeaderboardTable;