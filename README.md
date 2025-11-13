# SSW Rewards Mobile App!

This is a .NET MAUI app with a .NET 8 backend.

Use this app to scan SSW QR codes, earn SSW Points ⭐, claim rewards and win prizes!

## Vision

Connect the outside world with SSW at events through awesome rewards!

## Some Current Major Features

- Earn points by scanning company staff (eg. Northwind, SSW)
- Earn points by going to talks and scanning the QR code
- Complete AI-driven quizzes for points
- Redeem points for rewards
- Create a network of company staff you've met
- View leaderboards of highest points-earners
- Link social media such as LinkedIn and view others' profiles
- View an activity feed from friends and others in the app

## Roadmap

- Upgrade to .NET 9
- Push notification support - for prize draws 🥳
- Offline access
- Better support for white labelling to allow companies to put their own branding to the app
- [SSW Rewards Engagement Strategy](https://www.youtube.com/watch?v=qLeeRmg87GY)

## .NET MAUI migration

App was upgraded from Xamarin.Forms to .NET MAUI, using initially the .NET Migration assistant (using the .NET CLI) to produce a reference project, but then most of the work was done manually. You can see the resulting PR, which shows all the changes from the previous Xamarin.Forms version, to the .NET MAUI version here: https://github.com/SSWConsulting/SSW.Rewards.Mobile/pull/451.

## Contributing to this repo

To contribute to this project follow the steps below.
Please ensure you are following the [Developer Guidelines](https://github.com/SSWConsulting/SSW.Consulting/blob/master/Docs/Developer_Guidelines.MD) when submitting a pull request.

1. [F5 Experience](_docs/Instructions-Compile.md)
2. [Getting Started on a PBI](_docs/Definition-of-Ready.md)
3. [Getting Ready to Complete a PBI](_docs/Definition-of-Done.md)
4. [Deployment Steps](_docs/Instructions-Deployment.md)
5. [Overview](_docs/Business.md)
6. [Technologies & Architecture](_docs/Technologies-and-Architecture.md)
7. [Beta Testing (Android & iOS)](_docs/Instructions-Beta-Testing.md)

### UI Testing (AdminUI)

The project includes a comprehensive Playwright test suite for the AdminUI (`tools/ui-tests/`):

```bash
# Quick start
cd tools/ui-tests
npm install
npx playwright test

# Verify CSS changes (fast)
npx playwright test dom-inspection.spec.ts --grep "CSS"

# Run with visible browser
npx playwright test --headed
```

**Features**:

- ✅ Non-destructive (no data created)
- ✅ Fast execution (~10s for 9 tests)
- ✅ Authentication testing
- ✅ CSS/styling verification
- ✅ Form validation and interactions

See [`tools/ui-tests/README.md`](tools/ui-tests/README.md) for complete documentation.

### Trends and Analytics

- [App Analytics](https://analytics.itunes.apple.com/#/overview?app=1482994853&interval=r&datesel=d7&pmeasure=units&smeasure=units&tmeasure=units) - (You need a login for this. See Adam Cogan for access)

- [App sales and trends](https://reportingitc2.apple.com/insights?pageid=8) - (You need a login for this. See Adam Cogan for access)
