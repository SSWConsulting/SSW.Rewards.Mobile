# Deployment Troubleshooting

## Admin Portal CDN Cache

- **Manual Purge Access**: When requesting Azure staging resources access via My Access from SysAdmin, CDN purge permissions are now included.

### Manual Cache Purge (if needed)

If you need to manually purge the cache outside of deployment:

1. Navigate to [Azure Portal - Production Resource Group](https://portal.azure.com/#@sswcom.onmicrosoft.com/resource/subscriptions/b8b18dcf-d83b-47e2-9886-00c2e983629e/resourceGroups/SSW.Rewards.Prod/overview)
2. Open the `rewards` Front Door profile (it hosts both the staging and production endpoints)
3. Select **Endpoints** → Choose the endpoint
4. Click **Purge** and enter `/*` to purge all cached content

Alternatively, use Azure CLI:

```bash
# Purge staging Front Door cache
az afd endpoint purge \
  --resource-group SSW.Rewards.Prod \
  --profile-name rewards \
  --endpoint-name staging-sswrewards \
  --content-paths '/*' \
  --domains staging.rewards.ssw.com.au

# Purge production Front Door cache
az afd endpoint purge \
  --resource-group SSW.Rewards.Prod \
  --profile-name rewards \
  --endpoint-name sswrewards \
  --content-paths '/*' \
  --domains rewards.ssw.com.au
```

> Note: Cache purge typically takes 2-5 minutes to propagate globally.
