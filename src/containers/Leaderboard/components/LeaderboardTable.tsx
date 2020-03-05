import React from 'react'
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import  { LeaderboardTableRow, SkeletonRow } from './LeaderboardTableRow';
import { LeaderboardUserDto } from 'services';
import { TableHeader } from 'components';
import { TableRow, Avatar } from '@material-ui/core';
import { TableCell } from '@material-ui/core';
import Skeleton from "@material-ui/lab/Skeleton";


interface LeaderBoardTableProps {
    users?: LeaderboardUserDto[],
}

export const LeaderboardTable = (props:LeaderBoardTableProps) => {
    const { users } = props;
    return (
      <Table stickyHeader>
        <TableHeader items={["Rank", "Picture", "Name", "Points"]} />
        <TableBody>
          {users &&
            users.map((user: LeaderboardUserDto) => (
              <LeaderboardTableRow
                key={user.userId}
                user={user}></LeaderboardTableRow>
            ))}
          {!users &&
            Array.from("abcdefghijklmnop").map(k => <SkeletonRow key={k} />)}
          <SkeletonRow />
        </TableBody>
      </Table>
    );
}
