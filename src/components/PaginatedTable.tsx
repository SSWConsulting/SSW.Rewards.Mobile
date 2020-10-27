import React, { PropsWithChildren, useState } from 'react';
import {
  Table as MuiTable,
  TableBody,
  Paper,
  TablePagination,
  TableCell,
  TableHead,
  TableRow,
  TableFooter,
} from '@material-ui/core';

interface TableProps {
  items: string[];
  rowCount: number;
  contents: JSX.Element[] | undefined;
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

export const PaginatedTable = (props: PropsWithChildren<TableProps>) => {
  const { items, rowCount, contents } = props;
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const handleChangePage = (event: React.MouseEvent<HTMLButtonElement> | null, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  return (
    <Paper>
      <MuiTable stickyHeader>
        <TableHeader items={items} />
        {contents && (
          <TableBody>
            {rowsPerPage > 0 ? contents.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage) : contents}
          </TableBody>
        )}
        <TableFooter>
          <TableRow>
            <TablePagination
              rowsPerPageOptions={[5, 10, 25, { label: 'All', value: -1 }]}
              colSpan={3}
              count={rowCount}
              rowsPerPage={rowsPerPage}
              page={page}
              SelectProps={{
                inputProps: { 'aria-label': 'rows per page' },
                native: true,
              }}
              onChangePage={handleChangePage}
              onChangeRowsPerPage={handleChangeRowsPerPage}
            />
          </TableRow>
        </TableFooter>
      </MuiTable>
    </Paper>
  );
};
