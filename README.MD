# SSW Rewards Mobile App!

This is a Xamarin Forms app with a .NET Core 3 Api backend.

Use this app to scan SSW QR codes and get rewards!

## Contributing to this repo
1. Clone the repository
2. Create a feature branch
3. Open a Pull Request

## Versioning Strategy

### Client
Android and iOS versions of the app will be released simultaneously and with consistent version numbers. Version numbers will point incremented with new build releases. E.g. 1.9, 1.10, 1.11... etc.

Major new version releases will incur a major number increment, e.g. from 1.x to 2.x. A major new release is considered to provide extensive new design or functionality to the extent that previous versions are no longer compatible.

### API
The API version number will not change. However, the API specification version (provided in the OpenAPI document) will increment when a breaking change is introduced.

A breaking change is defined as removing an endpoint, or changing an endpoint so that current clients can no longer consume it. Adding a new endpoint will not result in a version number increment.

Major new version releases will incur a major number increment, e.g. from 1.0 to 2.0. A major new release is considered one in which all endpoints in this version have either been retired or redesigned to the extent that previous consumers are no longer compatible.

Legacy API specification versions will not be maintained indefinitely. Analytics and telemetry will be used ([see below](#analytics)) to determine when clinet utilisation has dropped to negligible levels, at which point the legacy API version will be retired.

## Best Practices

### Clean Architecture
This project adheres to Jason Taylor (SSW)'s Clean Architecture. For more information see:

* SSW Rules to Better Clean Architecture: https://rules.ssw.com.au/rules-to-better-clean-architecture
* Jason's Clean Architecture template: https://github.com/JasonGT/CleanArchitecture
* Jason's presentation on Clean Architecture from NDC: https://www.youtube.com/watch?v=Zygw4UAxCdg

### Microsoft REST API Guidelines
This application adheres to Microsoft's REST API Guidelines. For more information see: https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md

**NOTE:** Within the guidelines Microsoft capitalise a number of keywords, as explained by [Section 4.3 Requirements Language](https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#43-requirements-language). This section explicitly makes reference to [RFC 2119](https://www.ietf.org/rfc/rfc2119.txt).

This appliction strictly adheres to all guidelines defined with the keywords "MUST", "MUST NOT", "REQUIRED", "SHALL" and "SHALL NOT".
Any deviation only occurs to guidelines defined with the keywords "SHOULD", "SHOULD NOT", "RECOMMENDED", "MAY", and "OPTIONAL". As per the RFC, in these instances the implications have been fully understood and an informed decision made to deviate from the guidelines.


## <a name="analytics"></a>Trends and Analytics

* App Analytics here: https://analytics.itunes.apple.com/#/overview?app=1482994853&interval=r&datesel=d7&pmeasure=units&smeasure=units&tmeasure=units
(You need a login for this. See Adam Cogan for access)

* App sales and trends here: https://reportingitc2.apple.com/insights?pageid=8
(You need a login for this. See Adam Cogan for access)
