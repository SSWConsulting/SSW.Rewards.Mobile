import React from 'react'
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import { AchievementTableRow, SkeletonRow } from "./AchievementTableRow";
import { AchievementAdminViewModel } from "services";
import { TableHeader } from 'components';

interface LeaderBoardTableProps {
    achievements?: AchievementAdminViewModel[],
}

export const AchievementTable = (props:LeaderBoardTableProps) => {
    const { achievements } = props;
    return (
      <Table stickyHeader>
        <TableHeader items={["Code", "Name", "Value"]} />
        <TableBody>
          {achievements &&
            achievements.map((a: AchievementAdminViewModel) => (
              <AchievementTableRow
                key={a.id}
                achievement={a}></AchievementTableRow>
            ))}
          {/* create an array out of invidual characters so each can act as an object key*/}
          {!achievements &&
            Array.from("abcdefghijklmnop").map(k => <SkeletonRow key={k} />)}
        </TableBody>
      </Table>
    );
}
