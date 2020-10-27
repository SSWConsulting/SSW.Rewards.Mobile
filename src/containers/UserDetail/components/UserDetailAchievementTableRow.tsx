import React, { useState } from 'react';
import { UserAchievementViewModel } from 'services';
import { TableRow, TableCell, Tooltip } from '@material-ui/core';
import moment from 'moment';

interface UserDetailAchievementTableRowProps {
  achievement: UserAchievementViewModel;
}

export const UserDetailAchievementTableRow = (props: UserDetailAchievementTableRowProps) => {
  const {
    achievement: { achievementName, achievementValue, awardedAt },
  } = props;
  const [showTime, setShowTime] = useState(false);

  return (
    <>
      <TableRow>
        <TableCell style={{ width: '60%' }}>{achievementName}</TableCell>
        <TableCell>{achievementValue}</TableCell>
        <TableCell style={{ width: '20%' }}>
          <Tooltip title={moment(awardedAt).format('DD/MM/yyyy hh:mm a')} aria-label="time">
            <label onClick={() => setShowTime(!showTime)}>
              {!showTime ? moment(awardedAt).fromNow() : moment(awardedAt).format('DD/MM/yyyy hh:mm a')}
            </label>
          </Tooltip>
        </TableCell>
      </TableRow>
    </>
  );
};
