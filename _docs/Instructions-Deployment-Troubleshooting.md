# Deployment Troubleshooting

## Mobile iOS Signing Certificate Expired

The iOS build fails during `Build and sign` when the Apple Distribution certificate in GitHub
Actions has expired or does not match the provisioning profile.

Typical log lines:

```text
The certificate '***' has expired
iOS code signing key '***' not found in keychain
```

GitHub uses two different Apple files:

- `APPLE_CERT` - base64 of a `.p12` file containing the Apple Distribution certificate and its private key.
- `APPLE_PROFILE` - base64 of the `SSWRewards.mobileprovision` App Store provisioning profile.

A `.cer` file downloaded from Apple is not enough for `APPLE_CERT`; it does not include the
private key. If the certificate was created by another developer, only that developer's Mac
(or a secure backup) normally has the private key needed to export the `.p12`.

### Create a New Apple Distribution Certificate

Create the CSR and private key outside the repository:

```bash
workdir="$HOME/Developer/clients/ssw/work/rewards-mobile-signing-rotation/$(date +%Y%m%d-%H%M%S)"
mkdir -p "$workdir"
chmod 700 "$workdir"

openssl genrsa -out "$workdir/apple-distribution-private.key" 2048
chmod 600 "$workdir/apple-distribution-private.key"

openssl req -new \
  -key "$workdir/apple-distribution-private.key" \
  -out "$workdir/apple-distribution.csr" \
  -subj "/emailAddress=<your-ssw-email>/CN=<your-name> Apple Distribution/O=Superior Software for Windows Pty Ltd/C=AU"

openssl req -in "$workdir/apple-distribution.csr" -noout -subject
```

In Apple Developer:

1. Open **Certificates, Identifiers & Profiles**.
2. Go to **Certificates** and click `+`.
3. Select **Apple Distribution**.
4. Upload `apple-distribution.csr`.
5. Download the generated `.cer` file into the same `workdir`.

Convert the downloaded `.cer` and private key into the `.p12` that GitHub Actions expects:

```bash
cd "$workdir"

openssl x509 -inform DER \
  -in "Apple Developer Distribution Certificate.cer" \
  -out apple-distribution.cer.pem

openssl x509 -in apple-distribution.cer.pem \
  -noout -subject -serial -enddate -fingerprint -sha1

openssl pkcs12 -export \
  -inkey apple-distribution-private.key \
  -in apple-distribution.cer.pem \
  -out apple-distribution.p12 \
  -name "Apple Distribution: Superior Software for Windows Pty Ltd (B5652JTA7Q)"
```

Use a strong export password for the `.p12`; this password becomes `APPLE_CERT_PASSWORD`.

### Regenerate the App Store Provisioning Profile

In Apple Developer:

1. Go to **Profiles**.
2. Open `SSWRewards` (not `SSWRewards_Dev`).
3. Confirm it is:
   - Platform: `iOS`
   - Type: `App Store`
   - App ID: `SSW Mobile App (com.SSW.SSW.Consulting)`
4. Click **Edit**.
5. Select the new Apple Distribution certificate.
6. Save and download the regenerated `SSW Rewards.mobileprovision`.

Verify the profile and certificate match:

```bash
security cms -D \
  -i "SSW Rewards.mobileprovision" \
  > SSWRewards.plist

/usr/libexec/PlistBuddy -c 'Print :Name' SSWRewards.plist
/usr/libexec/PlistBuddy -c 'Print :ExpirationDate' SSWRewards.plist
/usr/libexec/PlistBuddy -c 'Print :Entitlements:application-identifier' SSWRewards.plist

/usr/libexec/PlistBuddy -c 'Print :DeveloperCertificates:0' SSWRewards.plist > profile-cert0.der

openssl x509 -inform DER \
  -in profile-cert0.der \
  -noout -subject -serial -enddate -fingerprint -sha1

openssl x509 \
  -in apple-distribution.cer.pem \
  -noout -subject -serial -enddate -fingerprint -sha1
```

The two fingerprints must match.

### Update GitHub Actions Secrets

GitHub secrets are write-only, so verify the local files before uploading them.

```bash
repo="SSWConsulting/SSW.Rewards.Mobile"

base64 -i apple-distribution.p12 | tr -d '\n' |
  gh secret set APPLE_CERT -R "$repo"

read -rsp "p12 export password: " APPLE_CERT_PASSWORD
printf '\n'
printf '%s' "$APPLE_CERT_PASSWORD" |
  gh secret set APPLE_CERT_PASSWORD -R "$repo"
unset APPLE_CERT_PASSWORD

printf '%s' "Apple Distribution: Superior Software for Windows Pty Ltd (B5652JTA7Q)" |
  gh secret set APPLE_CERT_NAME -R "$repo"

base64 -i "SSW Rewards.mobileprovision" | tr -d '\n' |
  gh secret set APPLE_PROFILE -R "$repo"

printf '%s' "SSWRewards" |
  gh secret set APPLE_PROFILE_NAME -R "$repo"

gh secret list -R "$repo" | grep APPLE_
```

### Rerun the Mobile Workflow

```bash
gh workflow run mobile-main.yml -R SSWConsulting/SSW.Rewards.Mobile --ref main

gh run list \
  -R SSWConsulting/SSW.Rewards.Mobile \
  --workflow mobile-main.yml \
  --limit 3
```

The workflow uses the `prod` GitHub environment, so build jobs may wait for environment approval
before the iOS signing step runs.

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
