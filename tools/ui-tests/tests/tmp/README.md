# Disposable Tests Directory

This folder (`tests/tmp/`) is **gitignored** and intended for temporary, experimental, or one-off tests.

## 🎯 Purpose

Use this folder for:

- **Experimentation**: Try new Playwright features or test approaches
- **Debugging**: Create quick tests to debug specific UI issues
- **Learning**: Practice Playwright syntax without cluttering the main test suite
- **One-off verification**: Test specific scenarios during development
- **Screenshots**: Capture UI states for visual debugging
- **Temporary tests**: Tests you'll delete after the issue is fixed

## ✅ When to Use This Folder

```bash
# Quick debugging test
cd /Users/jk/Developer/git/SSW.Rewards.Mobile/tools/ui-tests
npx playwright test tests/tmp/my-debug.spec.ts --headed

# Capture screenshots for a specific page
npx playwright test tests/tmp/screenshot-test.spec.ts

# Experiment with new selectors
npx playwright test tests/tmp/selector-experiment.spec.ts --debug
```

## 📁 File Organization

```
tests/tmp/
├── example-disposable.spec.ts    # Example file (you can delete this)
├── my-debug-test.spec.ts         # Your temporary test
└── screenshot-capture.spec.ts    # One-off screenshot test
```

## 🚀 Quick Start

1. **Create a new test file** in this folder:

   ```bash
   touch tests/tmp/my-test.spec.ts
   ```

2. **Use the screenshot helper** for consistent naming:

   ```typescript
   import { test } from "@playwright/test";
   import { takeResponsiveScreenshots } from "../../utils/screenshot-helper";

   test.use({ storageState: ".auth/user.json" });

   test("my quick test", async ({ page }) => {
     await page.goto("https://localhost:7137/your-page");
     await page.waitForLoadState("networkidle");

     // Take responsive screenshots (mobile, tablet, desktop)
     await takeResponsiveScreenshots(page, "screenshots/tmp", "my-test", {
       collapseSidebar: true,
     });
   });
   ```

3. **Run your test** (uses list reporter by default):

   ```bash
   npx playwright test tests/tmp/my-test.spec.ts --headed
   ```

4. **When done**, either:
   - Delete the file (it's disposable!)
   - Move it to `tests/` if it's valuable for the project

## 📸 Screenshot Helper Utility

**Always use** `takeResponsiveScreenshots()` for consistent naming:

```typescript
import { takeResponsiveScreenshots } from "../../utils/screenshot-helper";

await takeResponsiveScreenshots(
  page,
  "screenshots/tmp", // Base path
  "feature-name", // Name
  {
    collapseSidebar: true, // Auto-collapse on mobile/tablet
    waitForNetwork: false, // Optional network wait
    fullPage: true, // Full page screenshot
  }
);
```

**Generates**:

- `mobile-375x667-feature-name.png`
- `tablet-768x1024-feature-name.png`
- `desktop-1280x720-feature-name.png`

## 📝 Best Practices

### ✅ DO Use This Folder For:

- Testing specific selectors before adding to main tests
- Debugging form interactions
- Capturing screenshots for bug reports
- Learning Playwright API
- Temporary verification during feature development

### ❌ DON'T Use This Folder For:

- Long-term regression tests (use `tests/` instead)
- Tests that verify critical functionality
- Tests needed for CI/CD pipeline
- Tests other team members need to see

## 🔄 Workflow

```
1. Create test in tests/tmp/
   ↓
2. Experiment and debug
   ↓
3. Test works? → Move to tests/
   ↓
4. Test done? → Delete from tmp/
```

## 📸 Screenshots

Screenshots from disposable tests should go in the main `screenshots/` folder (also gitignored).

## 🤖 AI-Friendly

This folder is perfect for AI-assisted development:

```
"Create a quick test in tests/tmp/ to verify the new date picker behavior"
"Add a disposable test with takeResponsiveScreenshots to capture all form states"
"Write a tmp test to debug why the autocomplete isn't working"
```

## 🎓 Example Use Cases

### Debugging with Responsive Screenshots

```typescript
// tests/tmp/debug-autocomplete.spec.ts
import { test } from "@playwright/test";
import { takeResponsiveScreenshots } from "../../utils/screenshot-helper";

test.use({ storageState: ".auth/user.json" });

test("debug autocomplete dropdown", async ({ page }) => {
  await page.goto("https://localhost:7137/send-notification");
  const input = page.getByTestId("target-achievement");
  await input.click();
  await input.fill("test");

  // Take responsive screenshots
  await takeResponsiveScreenshots(
    page,
    "screenshots/tmp",
    "autocomplete-debug",
    { collapseSidebar: true }
  );
});
```

### Capturing Multiple Pages

```typescript
// tests/tmp/screenshot-all-pages.spec.ts
import { test } from "@playwright/test";
import { takeResponsiveScreenshots } from "../../utils/screenshot-helper";

test.use({ storageState: ".auth/user.json" });

test("capture all admin pages", async ({ page }) => {
  const pages = ["/users", "/achievements", "/notifications"];
  for (const route of pages) {
    await page.goto(`https://localhost:7137${route}`);
    await takeResponsiveScreenshots(
      page,
      "screenshots/tmp",
      route.slice(1), // Remove leading slash
      { collapseSidebar: true }
    );
  }
});
```

### Testing New Selector

```typescript
// tests/tmp/test-new-selector.spec.ts
test("find the right selector", async ({ page }) => {
  await page.goto("https://localhost:7137/send-notification");

  // Try different selectors
  const selector1 = page.locator(".mud-input-slot");
  const selector2 = page.getByTestId("notification-title");
  const selector3 = page.locator('input[aria-label*="title"]');

  console.log("Selector 1 count:", await selector1.count());
  console.log("Selector 2 count:", await selector2.count());
  console.log("Selector 3 count:", await selector3.count());
});
```

---

**Remember**: This folder is gitignored! Your tests here won't be committed to the repository.

**Pro Tip**: Always use `takeResponsiveScreenshots()` for consistent naming across all devices.

**Last Updated**: November 2025
