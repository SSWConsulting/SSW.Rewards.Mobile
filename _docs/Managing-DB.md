# Managing Database Backups and Restores in Docker

This guide explains how to back up and restore the SQL Server 2022 database used by this project, running in Docker containers.

## Prerequisites

- Make sure to follow `Instructions-Compile.md`
- Docker installed (Mac or Windows)
- SQL Server 2022 container running (`docker compose --profile tools up -d`)

## Backup Location

All database backups are stored in the `backups` folder at the root of the repo.

## Creating a Backup

To create a backup of the database, run the following command from the root of the repo:

```sh
docker exec -it rewards-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 -U SA -P 'Rewards.Docker1!' -C -Q "
    BACKUP DATABASE [ssw.rewards]
      TO DISK = '/backup/ssw.rewards_$(date +%Y%m%d_%H%M).bak'
      WITH INIT, COMPRESSION;"
```

## Restoring a Backup

To restore the database from a backup file:

1. Place your `.bak` file in the `backups` folder.
2. Run the following command from the root of the repo:

```sh
docker exec -it rewards-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 -U SA -P 'Rewards.Docker1!' -C -Q "
    RESTORE DATABASE [ssw.rewards]
      FROM DISK = '/backup/{{ BAK_FILENAME }}'
      WITH MOVE 'ssw.rewards'      TO '/var/opt/mssql/data/ssw.rewards.mdf',
           MOVE 'ssw.rewards_log'  TO '/var/opt/mssql/data/ssw.rewards.ldf',
           REPLACE;"
```

**NOTE:** Replace `{{ BAK_FILENAME }}` with the bak file name.

## Notes

- SQL Server 2022 is used as last stable supported version for both Mac OS and Windows.
- The `backups` folder is mapped to `/backup` in the container (see `docker-compose.yml`).
- For more details, see `Instructions-Compile.md`.
- ⚠️ When migrating from Azure SQL Edge to SQL Server 2022, you need to export DB schema and data if you want to restore it in SQL Server
