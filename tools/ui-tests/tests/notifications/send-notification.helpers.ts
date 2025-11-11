import { Page, expect } from '@playwright/test';

/**
 * Common test utilities for SendNotification page
 */

/** Navigation */
export async function navigateToSendNotification(page: Page) {
  await page.goto('https://localhost:7137/send-notification');
  await page.waitForLoadState('networkidle');
}

/** Form element selectors - centralized for easy maintenance */
export const Selectors = {
  // Delivery options
  deliveryNow: '[data-testid="delivery-now"]',
  deliverySchedule: '[data-testid="delivery-schedule"]',
  
  // Targeting options
  targetEveryone: '[data-testid="target-everyone"]',
  targetAchievement: '[data-testid="target-achievement"]',
  targetRole: '[data-testid="target-role"]',
  
  // Form fields
  notificationTitle: '[data-testid="notification-title"]',
  notificationBody: '[data-testid="notification-body"]',
  notificationImageUrl: '[data-testid="notification-image-url"]',
  achievementAutocomplete: '[data-testid="achievement-autocomplete"]',
  roleAutocomplete: '[data-testid="role-autocomplete"]',
  
  // iPhone preview elements
  iphonePreview: '.iphone-preview',
  iphoneScreen: '.iphone-screen',
  iphoneNotch: '.iphone-notch',
  iphoneTime: '.iphone-time',
  notificationPreviewContainer: '.notification-preview',
  notificationHeader: '.notification-header',
  notificationIcon: '.notification-icon',
  notificationAppName: '.notification-app-name',
  notificationTimeBadge: '.notification-time-badge',
  previewTitle: '.notification-title',
  previewBody: '.notification-body',
  previewImage: '.notification-image',
  
  // Submit
  submitButton: 'button[type="submit"]'
};

/** Verification helpers */
export async function verifyDeliveryOption(page: Page, option: 'now' | 'schedule') {
  const selector = option === 'now' ? Selectors.deliveryNow : Selectors.deliverySchedule;
  await expect(page.locator(selector)).toHaveAttribute('aria-checked', 'true');
}

export async function verifyTargetOption(page: Page, option: 'everyone' | 'achievement' | 'role') {
  const selectorMap = {
    everyone: Selectors.targetEveryone,
    achievement: Selectors.targetAchievement,
    role: Selectors.targetRole
  };
  await expect(page.locator(selectorMap[option])).toHaveAttribute('aria-checked', 'true');
}

export async function verifyPreviewText(page: Page, title?: string, body?: string) {
  if (title) {
    await expect(page.locator(Selectors.previewTitle)).toContainText(title);
  }
  if (body) {
    await expect(page.locator(Selectors.previewBody)).toContainText(body);
  }
}

export async function verifyPreviewVisible(page: Page, shouldBeVisible: boolean = true) {
  const preview = page.locator(Selectors.iphonePreview);
  if (shouldBeVisible) {
    await expect(preview).toBeVisible();
  } else {
    await expect(preview).not.toBeVisible();
  }
}

/** Form interaction helpers */
export async function fillNotificationForm(page: Page, title: string, body: string, imageUrl?: string) {
  await page.fill(Selectors.notificationTitle, title);
  await page.fill(Selectors.notificationBody, body);
  if (imageUrl) {
    await page.fill(Selectors.notificationImageUrl, imageUrl);
  }
}

export async function selectDeliveryOption(page: Page, option: 'now' | 'schedule') {
  const selector = option === 'now' ? Selectors.deliveryNow : Selectors.deliverySchedule;
  await page.click(selector);
  await page.waitForTimeout(500);
}

export async function selectTargetOption(page: Page, option: 'everyone' | 'achievement' | 'role') {
  const selectorMap = {
    everyone: Selectors.targetEveryone,
    achievement: Selectors.targetAchievement,
    role: Selectors.targetRole
  };
  await page.click(selectorMap[option]);
  await page.waitForTimeout(500);
}

export async function selectAchievementFromDropdown(page: Page, searchText: string = 'test') {
  const autocomplete = page.locator(Selectors.achievementAutocomplete);
  await autocomplete.waitFor({ state: 'visible', timeout: 10000 });
  await autocomplete.click();
  await page.keyboard.type(searchText);
  await page.waitForTimeout(1500);
  
  const firstResult = page.locator('.mud-list-item').first();
  await firstResult.waitFor({ state: 'visible', timeout: 5000 });
  await firstResult.click();
}

/** Responsive/overflow checks */
export async function checkHorizontalOverflow(page: Page): Promise<{ hasOverflow: boolean; scrollWidth: number; clientWidth: number }> {
  const { hasOverflow, scrollWidth, clientWidth } = await page.evaluate(() => {
    return {
      hasOverflow: document.documentElement.scrollWidth > document.documentElement.clientWidth,
      scrollWidth: document.documentElement.scrollWidth,
      clientWidth: document.documentElement.clientWidth
    };
  });
  
  return { hasOverflow, scrollWidth, clientWidth };
}

export async function checkVerticalScroll(page: Page): Promise<{ isScrollable: boolean; scrollHeight: number; viewportHeight: number }> {
  const { isScrollable, scrollHeight, viewportHeight } = await page.evaluate(() => {
    return {
      isScrollable: document.documentElement.scrollHeight > document.documentElement.clientHeight,
      scrollHeight: document.documentElement.scrollHeight,
      viewportHeight: document.documentElement.clientHeight
    };
  });
  
  return { isScrollable, scrollHeight, viewportHeight };
}

/** Console logging helpers for consistent output */
export function logTestHeader(testName: string) {
  console.log(`\n${testName}\n`);
  console.log('='.repeat(70));
}

export function logTestComplete(testName: string) {
  console.log(`\n✅ ${testName} - Verified`);
  console.log('='.repeat(70));
}

export function logViewport(width: number, height: number) {
  console.log(`📐 Viewport: ${width}x${height}`);
}

export function logSuccess(message: string) {
  console.log(`✅ ${message}`);
}

export function logWarning(message: string) {
  console.log(`⚠️  ${message}`);
}

export function logInfo(message: string) {
  console.log(`ℹ️  ${message}`);
}
