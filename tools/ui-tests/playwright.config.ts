import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for SSW Rewards Admin UI tests
 * 
 * Authentication Strategy:
 * 1. Run auth.setup.ts once to authenticate and save session
 * 2. All tests reuse the saved authentication state
 * 3. Credentials stored in environment variables for security
 */

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  maxFailures: 1, // Stop after first failure so you can work on it
  
  reporter: [
    ['html'],
    ['list']
  ],
  
  use: {
    baseURL: 'https://localhost:7137',
    trace: 'on-first-retry',
    ignoreHTTPSErrors: true, // For local development with self-signed certs
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    // Setup project - runs authentication once (run manually: npx playwright test --project=setup)
    { 
      name: 'setup', 
      testMatch: /.*\.setup\.ts/,
    },

    // Test projects - reuse authentication state
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        storageState: '.auth/user.json', // Reuse saved auth state
      },
      // dependencies: ['setup'], // Commented out - run setup manually when needed
    },

    // Optionally test in Firefox
    // {
    //   name: 'firefox',
    //   use: { 
    //     ...devices['Desktop Firefox'],
    //     storageState: '.auth/user.json',
    //   },
    //   dependencies: ['setup'],
    // },
  ],

  // Automatically start the application if not running
  // webServer: {
  //   command: 'docker compose --profile all up',
  //   url: 'https://localhost:7137',
  //   reuseExistingServer: true,
  //   ignoreHTTPSErrors: true,
  //   timeout: 120 * 1000,
  // },
});
