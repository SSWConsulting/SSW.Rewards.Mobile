import React, { useState, PropsWithChildren } from 'react';
import { Tabs as MuiTabs, Tab, Box, Typography, AppBar } from '@material-ui/core';

export interface TabsProps {
  titles: string[];
  tabChanged: (selectedTab: string) => void;
  tabContent: JSX.Element[];
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: any;
  value: any;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}>
      {value === index && (
        <Box paddingTop={1}>
          <Typography>{children}</Typography>
        </Box>
      )}
    </div>
  );
}

function a11yProps(index: any) {
  return {
    id: `full-width-tab-${index}`,
    'aria-controls': `full-width-tabpanel-${index}`,
  };
}

export function Tabs(props: PropsWithChildren<TabsProps>) {
  const { titles, tabContent, tabChanged } = props;
  const [value, setValue] = useState(0);

  const handleChange = (event: React.ChangeEvent<{}>, newValue: number) => {
    setValue(newValue);
    tabChanged(titles[newValue]);
  };

  return (
    <div>
      <AppBar position="static" color="default">
        <MuiTabs
          value={value}
          onChange={handleChange}
          indicatorColor="primary"
          textColor="primary"
          variant="fullWidth"
          aria-label="full width tabs"
          centered>
          {titles.map((t, i) => (
            <Tab label={t} {...a11yProps(i)} style={{ fontWeight: 'bold' }} />
          ))}
        </MuiTabs>
      </AppBar>
      {tabContent.map((t, i) => (
        <TabPanel value={value} index={i}>
          {t}
        </TabPanel>
      ))}
    </div>
  );
}
