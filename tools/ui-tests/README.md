# SSW Rewards AdminUI - UI Testing Suite

Comprehensive Playwright-based UI testing suite for verifying AdminUI functionality, DOM structure, CSS styling, and form interactions.

## 🎯 Purpose

This testing suite is designed for:

- **AI-Driven UI Verification**: Quick validation of UI changes and styling
- **Regression Testing**: Ensure UI components work correctly after updates
- **DOM Inspection**: Understand component structure for debugging
- **Form Behavior Testing**: Verify form interactions without data modification

## 📁 Structure

```
tools/ui-tests/
├── tests/
│   ├── auth.setup.ts              # Authentication setup (run once)
│   ├── auth.verify.spec.ts        # Verify authentication is working
│   ├── dom-inspection.spec.ts     # DOM structure and CSS verification
│   ├── form-interactions.spec.ts  # Non-destructive form testing
│   └── tmp/                       # Disposable tests (gitignored)
│       ├── README.md              # Guide for disposable tests
│       └── example-disposable.spec.ts  # Example temporary test
├── screenshots/                    # Test screenshots (gitignored)
├── .auth/                         # Authentication state (gitignored)
├── .env                           # Environment variables (gitignored)
├── .env.example                   # Example environment variables
├── playwright.config.ts           # Playwright configuration
├── package.json                   # Dependencies
└── README.md                      # This file
```

## 🚀 Quick Start

### 1. Install Dependencies

```bash
cd tools/ui-tests
npm install
```

### 2. Setup Environment

Copy `.env.example` to `.env` and fill in your credentials:

```bash
cp .env.example .env
```

Edit `.env`:

```env
TEST_USERNAME=your.email@example.com
TEST_PASSWORD=your_password
```

### 3. Start the Application

Ensure the AdminUI is running (Docker or local):

```bash
# From project root
docker compose --profile all up -d

# OR run locally
cd src/AdminUI
dotnet run
```

### 4. Run Authentication Setup

First time only (or when auth expires):

```bash
npx playwright test auth.setup.ts --headed
```

This saves your authenticated session to `.auth/user.json`.

### 5. Run Tests

```bash
# Run all tests
npx playwright test

# Run specific test file
npx playwright test auth.verify.spec.ts

# Run with UI (headed mode)
npx playwright test --headed

# Run with debug mode
npx playwright test --debug
```

## 📋 Test Suites

### Authentication Tests (`auth.*.ts`)

**Purpose**: Verify authentication is working correctly.

```bash
# Setup authentication (save session)
npx playwright test auth.setup.ts

# Verify authentication works
npx playwright test auth.verify.spec.ts
```

**What it tests**:

- ✅ Login flow to SSW Identity
- ✅ Session persistence
- ✅ Access to protected pages
- ✅ Cookie validity

### DOM Inspection Tests (`dom-inspection.spec.ts`)

**Purpose**: Inspect and verify DOM structure and CSS styling.

```bash
npx playwright test dom-inspection.spec.ts
```

**What it tests**:

- 🔍 Page structure and sections
- 🔍 Component hierarchy (MudBlazor)
- 🎨 CSS variable definitions
- 🎨 Focused input styling (smokey white background)
- 📦 Input field class structure

**Use cases**:

- Debugging CSS selector issues
- Verifying styling changes
- Understanding MudBlazor component structure

### Form Interaction Tests (`form-interactions.spec.ts`)

**Purpose**: Test form behavior without creating data.

```bash
npx playwright test form-interactions.spec.ts
```

**What it tests**:

- 📝 Form field population
- ✅ Validation behavior
- 🔘 Radio button interactions
- 👁️ Conditional field visibility
- 🎯 Autocomplete dropdowns

**Non-destructive**: These tests fill forms but **never submit** them.

## 🗑️ Disposable Tests (`tests/tmp/`)

**Purpose**: Temporary tests for experimentation and debugging.

The `tests/tmp/` folder is gitignored for disposable/temporary tests:

```bash
# Create a quick debugging test
npx playwright test tests/tmp/my-debug.spec.ts --headed

# Run example disposable test
npx playwright test tests/tmp/example-disposable.spec.ts
```

**Use cases**:

- 🔬 Experimenting with new test approaches
- 🐛 Debugging specific UI issues
- 📸 Capturing screenshots for bug reports
- 📚 Learning Playwright syntax
- ⚡ Quick one-off verifications

**See**: `tests/tmp/README.md` for detailed guide on disposable tests

## 🤖 AI-Driven UI Verification Guide

### Quick CSS Verification

After making CSS changes:

```bash
# 1. Rebuild Docker container (if using Docker)
docker compose --profile all restart rewards-adminui

# 2. Run DOM inspection test
npx playwright test dom-inspection.spec.ts --headed

# 3. Check console output for CSS values
# Expected: background: rgb(247, 247, 247), color: rgb(0, 0, 0)
```

### Create Disposable Test for Quick Debugging

```bash
# Create a temporary test file
cat > tests/tmp/debug-issue.spec.ts << 'EOF'
import { test } from '@playwright/test';
test.use({ storageState: '.auth/user.json' });

test('debug specific issue', async ({ page }) => {
  await page.goto('https://localhost:7137/send-notification');
  await page.screenshot({ path: 'screenshots/debug.png' });
  console.log('Debug screenshot captured');
});
EOF

# Run it
npx playwright test tests/tmp/debug-issue.spec.ts --headed

# Delete when done (it's gitignored anyway!)
rm tests/tmp/debug-issue.spec.ts
```

### Verify Form Behavior

After UI changes:

```bash
npx playwright test form-interactions.spec.ts --headed
```

Watch the test interact with the form automatically.

### Debug Specific Element

1. Create a disposable test in `tests/tmp/` or open an existing test file
2. Add inspection code for your element:

```typescript
const myElement = page.locator("your-selector");
const classes = await myElement.getAttribute("class");
const styles = await myElement.evaluate((el) => ({
  background: window.getComputedStyle(el).backgroundColor,
  color: window.getComputedStyle(el).color,
}));
console.log("Classes:", classes);
console.log("Styles:", styles);
```

3. Run with `--headed` to see it live:

```bash
npx playwright test dom-inspection.spec.ts --headed
```

## 📸 Screenshots

Screenshots are automatically saved to `screenshots/` during test runs:

- `form-filled.png` - Fully populated notification form
- Test failure screenshots - Automatically captured on errors

## 🔧 Configuration

### Playwright Config (`playwright.config.ts`)

Key settings:

- **Base URL**: `https://localhost:7137`
- **Timeout**: 30 seconds
- **Retries**: 2 (for flaky tests)
- **Screenshot on failure**: Enabled
- **Video on failure**: Enabled

### Environment Variables (`.env`)

```env
TEST_USERNAME=your.email@example.com  # Required for auth
TEST_PASSWORD=your_password           # Required for auth
```

## 🎭 Test Patterns & Best Practices

### 1. Always Use Saved Authentication

```typescript
test.use({ storageState: ".auth/user.json" });
```

This avoids re-authenticating for every test.

### 2. Wait for Page Load

```typescript
await page.goto("https://localhost:7137/your-page");
await page.waitForLoadState("networkidle");
```

### 3. Use Specific Selectors

```typescript
// ✅ Good: Use data-testid
await page.getByTestId("target-achievement").click();

// ✅ Good: Use aria-label
const input = page.locator('input[aria-label*="notification title"]');

// ❌ Avoid: Generic selectors
const input = page.locator("input").first();
```

### 4. Add Wait Time for Animations

```typescript
await page.click("button");
await page.waitForTimeout(300); // Wait for MudBlazor animations
```

### 5. Console Logging for AI Verification

```typescript
console.log("✅ Verification passed");
console.log(`📊 Found ${count} elements`);
console.log(`🎨 Background color: ${bgColor}`);
```

## 🐛 Troubleshooting

### Authentication Expired

```bash
# Re-run auth setup
npx playwright test auth.setup.ts --headed
```

### Tests Failing

```bash
# Run with debug mode to step through
npx playwright test --debug

# Run specific test with UI visible
npx playwright test form-interactions.spec.ts --headed
```

### CSS Changes Not Reflecting

```bash
# Force rebuild Docker container
cd ../..
docker compose --profile all down
docker compose --profile all build --no-cache rewards-adminui
docker compose --profile all up -d

# Wait for container to start
sleep 10

# Re-run tests
cd tools/ui-tests
npx playwright test dom-inspection.spec.ts
```

### Can't Find Element

1. Run test with `--headed` to see the page
2. Use browser DevTools to inspect element
3. Check the actual selector:

```typescript
// Debug selector
const element = page.locator("your-selector");
const count = await element.count();
console.log(`Found ${count} elements matching selector`);
```

## 📚 Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [MudBlazor Components](https://mudblazor.com/)
- [SSW Rewards Architecture](../../_docs/Technologies-and-Architecture.md)

## 🎯 Common Use Cases

### Verify New CSS Styling

```bash
npx playwright test dom-inspection.spec.ts --grep "verify CSS styling"
```

### Test Form Conditional Logic

```bash
npx playwright test form-interactions.spec.ts --grep "conditional fields"
```

### Quick Full Verification

```bash
# Run all non-destructive tests
npx playwright test --grep-invert "setup"
```

## 📝 Notes

- **Non-Destructive**: These tests never create or modify data
- **Fast Feedback**: Get immediate verification of UI changes
- **AI-Friendly**: Clear console output for automated analysis
- **Maintainable**: Tests focus on behavior, not implementation details

---

**Last Updated**: November 2025  
**Playwright Version**: Latest  
**Target**: AdminUI on https://localhost:7137
