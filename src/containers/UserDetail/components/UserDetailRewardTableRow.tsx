import React, { useState } from 'react';
import { UserRewardViewModel } from 'services';
import { TableRow, TableCell, Tooltip } from '@material-ui/core';
import moment from 'moment';

interface UserDetailRewardTableRowProps {
  achievement: UserRewardViewModel;
}

export const UserDetailRewardTableRow = (props: UserDetailRewardTableRowProps) => {
  const {
    achievement: { rewardName, rewardCost, awardedAt },
  } = props;
  const [showTime, setShowTime] = useState(false);
  return (
    <>
      <TableRow>
        <TableCell style={{ width: '60%' }}>{rewardName}</TableCell>
        <TableCell>{rewardCost}</TableCell>
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
