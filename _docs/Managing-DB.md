# Managing Database Backups and Restores

This guide explains how to back up and restore the SQL Server 2022 database used by this project.
Locally the database runs as a **.NET Aspire**-managed SQL Server container.

## Prerequisites

- Make sure to follow `Instructions-Compile.md` / [`Aspire-Local-Dev.md`](Aspire-Local-Dev.md)
- Docker installed (Mac or Windows)
- The local stack running: `cd src/AppHost && aspire run` (the `rewards-sql` resource shows **Running**)

> **Applying migrations** (not a restore): the schema is migrated on WebAPI startup, and you can
> re-run it any time from the Aspire dashboard → `rewards-sql` → **Actions ▸ Commands ▸ DB: Apply
> migrations** (or `dotnet ef database update`). Use the steps below only to restore real data.

## Container name & credentials

Under Aspire the SQL container name has a generated suffix (e.g. `rewards-sql-xxxxxxxx`), so resolve
it dynamically and use the SA password you set for the AppHost `sql-sa-password` parameter:

```sh
SQL=$(docker ps --filter "name=rewards-sql" --format '{{.Names}}')
SA_PWD='<your sql-sa-password parameter>'
```

## Backup Location

Back up inside the container, then copy the file out with `docker cp` (the Aspire container has a
persistent **data volume** but no host `/backup` mount). Keep `.bak` files in the repo's `backups` folder.

## Creating a Backup

```sh
docker exec -i "$SQL" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 -U SA -P "$SA_PWD" -C -Q "
    BACKUP DATABASE [ssw.rewards]
      TO DISK = '/var/opt/mssql/data/ssw.rewards.bak'
      WITH INIT, COMPRESSION;"

docker cp "$SQL":/var/opt/mssql/data/ssw.rewards.bak "backups/ssw.rewards_$(date +%Y%m%d_%H%M).bak"
```

## Restoring a Backup

1. Place your `.bak` file in the `backups` folder.
2. Copy it into the container and restore:

```sh
docker cp "backups/{{ BAK_FILENAME }}" "$SQL":/var/opt/mssql/data/restore.bak

docker exec -i "$SQL" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 -U SA -P "$SA_PWD" -C -Q "
    RESTORE DATABASE [ssw.rewards]
      FROM DISK = '/var/opt/mssql/data/restore.bak'
      WITH MOVE 'ssw.rewards'      TO '/var/opt/mssql/data/ssw.rewards.mdf',
           MOVE 'ssw.rewards_log'  TO '/var/opt/mssql/data/ssw.rewards.ldf',
           REPLACE;"
```

**NOTE:** Replace `{{ BAK_FILENAME }}` with the bak file name.

## Notes

- SQL Server 2022 is used as last stable supported version for both Mac OS and Windows.
- The SQL data lives in the Aspire persistent data volume `ssw-rewards-sql-data` (survives restarts).
- For more details, see `Instructions-Compile.md` / [`Aspire-Local-Dev.md`](Aspire-Local-Dev.md).
- ⚠️ When migrating from Azure SQL Edge to SQL Server 2022, you need to export DB schema and data if you want to restore it in SQL Server
