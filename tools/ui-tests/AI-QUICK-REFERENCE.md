# AI Quick Reference - UI Testing

Quick commands for AI agents to verify UI changes in SSW.Rewards AdminUI.

## 🚀 Common Verification Tasks

### 1. Verify CSS Styling Changes

```bash
cd tools/ui-tests
npx playwright test dom-inspection.spec.ts --grep "CSS"
```

**Output**: Shows background/text colors of focused inputs  
**Expected**: `rgb(247, 247, 247)` background, `rgb(0, 0, 0)` text

### 2. Inspect DOM Structure

```bash
cd tools/ui-tests
npx playwright test dom-inspection.spec.ts --grep "structure"
```

**Output**: Lists MudBlazor component class hierarchy  
**Use for**: Debugging CSS selectors, understanding component structure

### 3. Test Form Validation

```bash
cd tools/ui-tests
npx playwright test form-interactions.spec.ts --grep "validation"
```

**Output**: Confirms empty form shows validation errors  
**Use for**: Verifying form rules work

### 4. Verify Authentication

```bash
cd tools/ui-tests
npx playwright test auth.verify.spec.ts
```

**Output**: Checks session cookies and protected page access  
**Use for**: Ensuring auth is working before other tests

### 5. Full Page Structure Inspection

```bash
cd tools/ui-tests
npx playwright test dom-inspection.spec.ts --grep "page structure"
```

**Output**: Lists all sections, inputs, buttons on SendNotification page  
**Use for**: Getting page overview

### 6. Create Disposable Test for Quick Debugging

```bash
cd tools/ui-tests
npx playwright test tests/tmp/example-disposable.spec.ts --headed
```

**Output**: Runs temporary test examples  
**Use for**: Learning, experimentation, one-off debugging

## 🧪 Disposable Tests (Recommended for AI)

**Folder**: `tests/tmp/` (gitignored - won't be committed)

```bash
# Create quick debugging test
cat > tests/tmp/ai-debug.spec.ts << 'EOF'
import { test } from '@playwright/test';
test.use({ storageState: '.auth/user.json' });

test('AI quick verification', async ({ page }) => {
  await page.goto('https://localhost:7137/send-notification');
  await page.waitForLoadState('networkidle');

  // Your debugging code here
  await page.screenshot({ path: 'screenshots/ai-debug.png', fullPage: true });
  console.log('✅ Screenshot captured');
});
EOF

# Run it
npx playwright test tests/tmp/ai-debug.spec.ts --headed

# Clean up (optional - it's gitignored anyway)
rm tests/tmp/ai-debug.spec.ts
```

**Benefits**:

- ✅ No git pollution (tests aren't committed)
- ✅ Fast iteration (create, test, delete)
- ✅ Perfect for AI-generated tests
- ✅ Ideal for one-off verifications

**See**: `tests/tmp/README.md` for more examples

## 🐛 Debugging Commands

### See Tests Run Live

```bash
cd tools/ui-tests
npx playwright test dom-inspection.spec.ts --headed
```

**Note**: Opens browser window to watch test execution

### Step Through Test

```bash
cd tools/ui-tests
npx playwright test dom-inspection.spec.ts --debug
```

**Note**: Opens Playwright Inspector for step-by-step debugging

### Run Single Test

```bash
cd tools/ui-tests
npx playwright test -g "verify CSS styling on focused inputs"
```

## 📋 Test Files Quick Reference

| File                        | Purpose           | Pass Rate | Runtime |
| --------------------------- | ----------------- | --------- | ------- |
| `auth.setup.ts`             | Save auth session | ✅ 100%   | 5s      |
| `auth.verify.spec.ts`       | Verify auth works | ✅ 100%   | 6s      |
| `dom-inspection.spec.ts`    | Inspect DOM/CSS   | ✅ 100%   | 7s      |
| `form-interactions.spec.ts` | Test forms        | ⚠️ 50%    | 7-30s   |

## 🎯 Expected Test Output

### CSS Verification (dom-inspection.spec.ts)

```
📝 Title input (focused):
   Background: rgb(247, 247, 247)
   Text color: rgb(0, 0, 0)
✅ Smokey white styling verified!

🔎 Autocomplete input (focused):
   Background: rgb(247, 247, 247)
   Text color: rgb(0, 0, 0)
✅ Autocomplete styling verified!

🎉 CSS verification complete!
```

### DOM Structure (dom-inspection.spec.ts)

```
📦 Title Input DOM Structure:
input.mud-input-slot.mud-input-root.mud-input-root-text
  div.mud-input.mud-input-text.mud-input-underline
    div.mud-input-control-input-container
      div.mud-input-control.mud-input-required

🎨 CSS Variable --smokey-white: #F7F7F7

🎉 Structure inspection complete!
```

### Form Validation (form-interactions.spec.ts)

```
✅ Testing form validation...
✅ Form validation prevented empty submission
Validation errors visible: true

🎉 Validation test complete!
```

## 🔧 Troubleshooting

### If tests fail with "Session expired":

```bash
cd tools/ui-tests
npx playwright test auth.setup.ts --headed
# Login manually when prompted
```

### If Docker container not running:

```bash
cd ../..
docker compose --profile all up -d
# Wait 10 seconds for startup
cd tools/ui-tests
```

### If CSS changes not reflecting:

```bash
cd ../..
docker compose --profile all restart rewards-adminui
# Wait 5 seconds
cd tools/ui-tests
npx playwright test dom-inspection.spec.ts --grep "CSS"
```

### If need to rebuild Docker:

```bash
cd ../..
docker compose --profile all down
docker compose --profile all build --no-cache rewards-adminui
docker compose --profile all up -d
sleep 10
cd tools/ui-tests
```

## 📸 Screenshots

Tests automatically save screenshots to `screenshots/` on failure.

To manually capture screenshots:

```typescript
await page.screenshot({ path: "screenshots/my-test.png" });
```

## 🎨 CSS Values Reference

| Variable         | Value                      | Usage                     |
| ---------------- | -------------------------- | ------------------------- |
| `--smokey-white` | `#F7F7F7`                  | Input background on focus |
| RGB equivalent   | `rgb(247, 247, 247)`       | Test verification value   |
| Text color       | `#000000` / `rgb(0, 0, 0)` | Input text on focus       |

## 🤖 AI Prompt Templates

### "Verify CSS changes"

```
Run: npx playwright test dom-inspection.spec.ts --grep "CSS"
Check output for rgb(247, 247, 247) background
```

### "Inspect new component"

1. Add inspection code to `dom-inspection.spec.ts`
2. Run: `npx playwright test dom-inspection.spec.ts --headed`
3. Check console output

### "Test form behavior"

```
Run: npx playwright test form-interactions.spec.ts --grep "validation"
Verify validation errors appear
```

## ⚡ Performance Tips

- Auth tests: ~5-6s (with saved session)
- DOM tests: ~6-7s (no data modification)
- Form tests: ~7s (non-destructive)

Total test suite: ~36s for all tests

## 📞 Support

See full documentation: `README.md`  
Test results: `SUMMARY.md`  
Project docs: `../../_docs/`

---

**Quick Start**: `cd tools/ui-tests && npx playwright test`
