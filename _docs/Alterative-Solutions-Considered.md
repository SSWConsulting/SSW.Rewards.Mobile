## Alterative Solutions Considered

### Admin Portal

The original solution was built using React (Create-react-app) template, however over time this had become an older and unmaintainable solution. As such, we have decided to move the portal over to use Blazor with the new Authentication system. This also buds nicely with our developers being quite confident in their .NET skills and a lake of internal Blazor projects.

- Option 1. **React (Create-react-app)** (This was the original choice)

  - Security – Done - Proof of concept Azure AD B2C ✅
  - Developers on the project were more familiar with React ✅
  - Wanted an internal project that was built using React ✅

- Option 2. **React (NextJs)**

  - Due to having to update and change to a new auth system. Upgrading the current solution would require more time. Upgrading to a modern React framework would require less development time ✅
  - A prototype has been made ✅
  - Able to keep current look and feel of current App ✅
  - Its still a rewrite ❌
  - We have more developers available that enjoy working with .NET projects ❌

- Option 3. **Blazor** (As of 30/5/22 - this is the option going forward)
  - Since this is an admin portal, as a line of business application, it makes the Blazor argument stronger ✅
  - Our developers like working with C# over React ✅
  - Rewriting in Blazor doesn’t help the interns learn towards their coding challenge ❌
  - Adam prefers React as it is the most mainstream front-end technology ❌

### Mobile App

- Option 1. **Xamarin Forms**

  - More experience with Xamarin ✅
  - Allows devs to work on a Xamarin project ✅
  - Cross platform ✅
  - Able to share a lot of code around the multiple projects ✅
  - Not light-weight vs React Native (XAML vs Typescript) ❌

- Option 2. **React native**
  - Notifications = Good on iOS and Android ✅
  - Homepage icon automatic ✅
  - May help us change the Xamarin hack day to a mobile hack day ✅
  - App Store: 2 Day Deployment = BAD (Once off) ❌

### Authentication

The original solution was to use Azure AD B2C, however due to a authentication but and not happy with the overall architecture of the B2C implementation we have since upgraded to use Identity server.

- Option 1. **Completely new Identity Database + Azure AD B2C** (Chosen solution)

  - Separate to existing customer base (on CRM) ✅
  - If necessary, we can sync this to CRM ✅

- Option 2. **Use existing on-premise user database from ssw.com.au**

  - Keep consistent customer identity ✅
  - Will require extra effort to integrate = BAD ❌

- Option 3. **Identity Server**
  - Fixes Authentication issue with the B2C solution ✅
  - Keep consistent with other internal projects ✅
  - Rewrite of Every current Projects auth (Api,Mobile,Portal) ❌

### Database

- Option 1. **Azure Cosmos DB**

  - No migration headaches (Flexible schema) ✅
  - SQL-Like query language ✅
  - Take advantage of existing users ✅
  - Scalable ✅

- Option 2. **On Prem SQL Server**

  - Existing customer database (called SSWData2005) for consistent sign-in ✅
  - Slow = BAD ❌

- Option 3. **Azure SQL Server** (Chosen solution)

  - Relational model ✅
  - Devs are familiar with a relational model ✅
