import React from 'react';
import { TableCell, TableHead, TableRow } from "@material-ui/core";

export const TableHeader = (props: {items: string[]}) => {
  return (
    <TableHead>
      <TableRow>
        {props.items.map(i => <TableCell key={i}>{i}</TableCell>)}
      </TableRow>
    </TableHead>
  );
}

