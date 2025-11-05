# Deployment Troubleshooting

## Admin Portal CDN Cache

- **Manual Purge Access**: When requesting Azure staging resources access via My Access from SysAdmin, CDN purge permissions are now included.

### Manual Cache Purge (if needed)

If you need to manually purge the cache outside of deployment:

1. Navigate to [Azure Portal - Production Resource Group](https://portal.azure.com/#@sswcom.onmicrosoft.com/resource/subscriptions/b8b18dcf-d83b-47e2-9886-00c2e983629e/resourceGroups/SSW.Consulting.Prod/overview)
2. Find the Front Door profile for the environment you need to purge
3. Select **Endpoints** → Choose the endpoint
4. Click **Purge** and enter `/*` to purge all cached content

Alternatively, use Azure CLI:

```bash
# Purge staging Front Door cache
az afd endpoint purge \
  --resource-group SSW.Consulting.Prod \
  --profile-name <staging-profile-name> \
  --endpoint-name <staging-endpoint-name> \
  --content-paths '/*' \
  --domains <staging-domain>

# Purge production Front Door cache
az afd endpoint purge \
  --resource-group SSW.Consulting.Prod \
  --profile-name <prod-profile-name> \
  --endpoint-name <prod-endpoint-name> \
  --content-paths '/*' \
  --domains <prod-domain>
```

> Note: Cache purge typically takes 2-5 minutes to propagate globally.
