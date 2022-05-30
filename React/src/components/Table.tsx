import React, { PropsWithChildren } from 'react';
import { Table as MuiTable, TableBody, Paper } from '@material-ui/core';
import { TableCell, TableHead, TableRow } from '@material-ui/core';

interface TableProps {
  items: string[];
}

const TableHeader = (props: { items: string[] }) => {
  return (
    <TableHead>
      <TableRow>
        {props.items.map((i) => (
          <TableCell key={i}>{i}</TableCell>
        ))}
      </TableRow>
    </TableHead>
  );
};

export const Table = (props: PropsWithChildren<TableProps>) => {
  const { items } = props;

  return (
    <Paper>
      <MuiTable stickyHeader>
        <TableHeader items={items} />
        <TableBody>{props.children}</TableBody>
      </MuiTable>
    </Paper>
  );
};
