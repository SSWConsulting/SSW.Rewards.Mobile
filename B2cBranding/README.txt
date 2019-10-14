This is the SSW branding for the B2C Login user flow
The branding changes the following:
	- sign up and login pages 
	- verification code email that is sent to the user

--- Update the Styles ---

1) Upload the files
- Ensure the files are uploaded to the b2csigninsignout storage container
	Resource groups > SSW.Consulting.B2C > sswconsultingb2c > b2csigninsignout
	
	- bootstrap.min.css
	- exception.html
	- selfAsserted.html
	- signupsignin.html
	- ssw-logo.png

2) Update the B2C User Flow
- Update the B2C_1_SignUpAndSignIn User flow
	Home > Azure AD B2C - User flows (policies) > B2C_1_SignUpAndSignIn
	
	- Under "Page layouts"
	1. Unified sign up or sign in page
		- Select "Use custom page"
		- Enter the URL https://sswconsultingb2c.blob.core.windows.net/b2csigninsignout/signupsignin.html
	2. Local account sign up page
		- Select "Use custom page"
		- Enter the URL https://sswconsultingb2c.blob.core.windows.net/b2csigninsignout/selfAsserted.html
	3. Social account sign up page
		- Select "Use custom page"
		- Enter the URL https://sswconsultingb2c.blob.core.windows.net/b2csigninsignout/selfAsserted.html
	4. Error page
		- Select "Use custom page"
		- Enter the URL https://sswconsultingb2c.blob.core.windows.net/b2csigninsignout/exception.html

3) Ensure the Background colour is set for the branding
	Home > SSW.Consulting - Company branding > Default | Advanced settings	

	- Set the "Sign-in page background color" to #666666