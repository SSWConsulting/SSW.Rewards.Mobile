---
name: manage-database
description: Back up, restore, and migrate the local SSW.Rewards SQL Server database (the Aspire-managed `rewards-sql` container). Use when someone needs to back up or restore the dev DB, seed real data from staging/another dev, or apply EF migrations locally.
---

# Managing the local database (backup / restore / migrations)

Locally the database runs as a **.NET Aspire**-managed SQL Server 2022 container (`rewards-sql`).
Get the stack up first with `aspire run` (from the repo root; see `_docs/Aspire-Local-Dev.md`).

## Applying migrations (not a restore)

The schema is migrated on WebAPI startup. To re-run on demand, use the Aspire dashboard →
`rewards-sql` → **Actions ▸ Commands ▸ DB: Apply migrations** (or `dotnet ef database update
--project src/Infrastructure --startup-project src/WebAPI`). Use the backup/restore steps below
only to move **real data**.

## Container name & credentials

The Aspire SQL container has a generated suffix (e.g. `rewards-sql-xxxxxxxx`), so resolve it
dynamically and use the SA password from the AppHost `sql-sa-password` parameter:

```sh
SQL=$(docker ps --filter "name=rewards-sql" --format '{{.Names}}')
SA_PWD='<your sql-sa-password parameter>'
```

## Create a backup

Back up inside the container, then copy it out with `docker cp` (the Aspire container has a
persistent data volume but no host `/backup` mount). Keep `.bak` files in the repo's `backups` folder.

```sh
docker exec -i "$SQL" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost,1433 -U SA -P "$SA_PWD" -C -Q "
    BACKUP DATABASE [ssw.rewards]
      TO DISK = '/var/opt/mssql/data/ssw.rewards.bak'
      WITH INIT, COMPRESSION;"

docker cp "$SQL":/var/opt/mssql/data/ssw.rewards.bak "backups/ssw.rewards_$(date +%Y%m%d_%H%M).bak"
```

## Restore a backup

1. Place your `.bak` file in the `backups` folder.
2. Copy it into the container and restore (replace `{{ BAK_FILENAME }}`):

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

## Notes

- SQL Server 2022 is the last stable version supported on both macOS and Windows.
- The SQL data lives in the Aspire persistent data volume `ssw-rewards-sql-data` (survives restarts).
- ⚠️ When migrating from Azure SQL Edge to SQL Server 2022, export the DB schema **and** data if you
  want to restore it into SQL Server.
