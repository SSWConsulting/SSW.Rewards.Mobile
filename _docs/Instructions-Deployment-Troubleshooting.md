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
- `APPLE_PROFILE` - base64 of the App Store provisioning profile (`.mobileprovision`).

The workflow also expects:

- `APPLE_CERT_PASSWORD` - the export password for the `.p12`.
- `APPLE_CERT_NAME` - the certificate identity used by the build, for example `Apple Distribution: <Company Name> (<Team ID>)`.
- `APPLE_PROFILE_NAME` - the profile name embedded in the provisioning profile.

A `.cer` file downloaded from Apple is not enough for `APPLE_CERT`; it does not include the
private key. If the certificate was created by another developer, only that developer's Mac
(or a secure backup) normally has the private key needed to export the `.p12`.

### Create a New Apple Distribution Certificate

Create the CSR and private key outside the repository:

```bash
workdir="$HOME/work/mobile-signing-rotation/$(date +%Y%m%d-%H%M%S)"
mkdir -p "$workdir"
chmod 700 "$workdir"

openssl genrsa -out "$workdir/apple-distribution-private.key" 2048
chmod 600 "$workdir/apple-distribution-private.key"

openssl req -new \
  -key "$workdir/apple-distribution-private.key" \
  -out "$workdir/apple-distribution.csr" \
  -subj "/emailAddress=<your-email>/CN=<your-name> Apple Distribution/O=<organization-name>/C=<country-code>"

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
  -name "Apple Distribution: <Company Name> (<Team ID>)"
```

Use a strong export password for the `.p12`; this password becomes `APPLE_CERT_PASSWORD`.

For the CSR pattern, see SSW Rules: [Do you Code-Sign and Notarize your Apple applications?](https://www.ssw.com.au/rules/code-sign-and-notarize-apple-application/).
That rule targets macOS Developer ID signing, so use Apple Developer's docs for the iOS-specific
certificate and App Store profile types.

### Regenerate the App Store Provisioning Profile

In Apple Developer:

1. Go to **Profiles**.
2. Open the App Store provisioning profile used by CI, not a Development profile.
3. Confirm it is:
   - Platform: `iOS`
   - Type: `App Store`
   - App ID: the app's production bundle identifier
4. Click **Edit**.
5. Select the new Apple Distribution certificate.
6. Save and download the regenerated `.mobileprovision` file.

Verify the profile and certificate match:

```bash
security cms -D \
  -i "<App Store Profile>.mobileprovision" \
  > AppStoreProfile.plist

/usr/libexec/PlistBuddy -c 'Print :Name' AppStoreProfile.plist
/usr/libexec/PlistBuddy -c 'Print :ExpirationDate' AppStoreProfile.plist
/usr/libexec/PlistBuddy -c 'Print :Entitlements:application-identifier' AppStoreProfile.plist

/usr/libexec/PlistBuddy -c 'Print :DeveloperCertificates:0' AppStoreProfile.plist > profile-cert0.der

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
repo="<owner>/<repo>"

base64 -i apple-distribution.p12 | tr -d '\n' |
  gh secret set APPLE_CERT -R "$repo"

read -rsp "p12 export password: " APPLE_CERT_PASSWORD
printf '\n'
printf '%s' "$APPLE_CERT_PASSWORD" |
  gh secret set APPLE_CERT_PASSWORD -R "$repo"
unset APPLE_CERT_PASSWORD

printf '%s' "Apple Distribution: <Company Name> (<Team ID>)" |
  gh secret set APPLE_CERT_NAME -R "$repo"

base64 -i "<App Store Profile>.mobileprovision" | tr -d '\n' |
  gh secret set APPLE_PROFILE -R "$repo"

printf '%s' "<profile-name>" |
  gh secret set APPLE_PROFILE_NAME -R "$repo"

gh secret list -R "$repo" | grep APPLE_
```

### Rerun the Mobile Workflow

```bash
gh workflow run mobile-main.yml -R <owner>/<repo> --ref main

gh run list \
  -R <owner>/<repo> \
  --workflow mobile-main.yml \
  --limit 3
```

The workflow uses the `prod` GitHub environment, so build jobs may wait for environment approval
before the iOS signing step runs.

References:

- [Apple - Create a certificate signing request](https://developer.apple.com/help/account/certificates/create-a-certificate-signing-request/)
- [Apple - Certificates overview](https://developer.apple.com/help/account/certificates/certificates-overview/)
- [Apple - Create an App Store Connect provisioning profile](https://developer.apple.com/help/account/provisioning-profiles/create-an-app-store-provisioning-profile/)

## Mobile Android Signing Keystore

The Android build signs the release AAB before uploading it to Google Play. In this repo the build
expects these secrets:

- `ANDROID_KEYSTORE` - base64 of the release/upload keystore file.
- `ANDROID_KEYPASSWORD` - password for the key alias.
- `ANDROID_KEYSTOREALIAS` - alias inside the keystore.
- `ANDROID_KEYSTOREPASSWORD` - password for the keystore.

The deploy workflow then uploads the signed AAB to Google Play using:

- `GCP_SERVICE_ACCOUNT` - service account JSON for the Google Play Developer API.

The current workflow uploads Android builds to the `internal` Google Play track. Moving the same
build to open testing or production is a separate Play Console release/promotion step unless a
dedicated workflow is added for that track.

### Verify the Keystore Metadata

Use metadata checks only; do not print passwords or commit the keystore.

```bash
workdir="$HOME/work/mobile-android-signing-check"
mkdir -p "$workdir"
chmod 700 "$workdir"

# macOS:
base64 -D -i android-keystore.base64 -o "$workdir/upload.keystore"
# Linux:
# base64 --decode < android-keystore.base64 > "$workdir/upload.keystore"
chmod 600 "$workdir/upload.keystore"

keytool -list -v \
  -keystore "$workdir/upload.keystore" \
  -alias "<key-alias>"
```

Confirm:

- the alias matches `ANDROID_KEYSTOREALIAS`;
- the certificate SHA-1/SHA-256 matches the upload certificate expected in Google Play Console;
- the certificate validity has not expired.

### Rotate or Recover Android Signing

Android signing differs from iOS signing:

- Google Play App Signing stores the app signing key for Play-distributed apps.
- CI normally signs uploads with an upload key/keystore.
- If the upload key is lost or compromised, use Google Play Console's upload key reset flow rather
  than inventing a new key and expecting existing app updates to accept it.

References:

- [Android Developers - Sign your app](https://developer.android.com/studio/publish/app-signing)
- [Google Play Help - Use Play App Signing](https://support.google.com/googleplay/android-developer/answer/9842756)
- [Google Play Help - Set up an open, closed, or internal test](https://support.google.com/googleplay/android-developer/answer/9845334)
- [Google Play Help - Prepare and roll out a release](https://support.google.com/googleplay/android-developer/answer/9859348)

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
