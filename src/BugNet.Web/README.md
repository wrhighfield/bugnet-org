# Developer Notes

Below are some developer notes for this project, this project is ongoing and may not be finished. This is an ongoing pet project for me as I used to work on the original BugNet [here](https://github.com/dubeaud/bugnet). I forked the original and have been playing around with it off and on for the last few years.  Not sure how far I will get with it.

Just an FYI I know that the solution is a mess right now, I am keeping the old code around just for reference and once I feel this is stable enough I will be doing a major cleanup.

* I decided not to use Blazor as I am not comfortable if it will be come vapor ware in the future. This is only a personal opinion and probably won't change anytime soon. Your just lucky as I may still write the UI in Angular or maybe even Svelt.
* Some of the decisions I make around architecture, features I keep (or remove) and libraries are just what I feel would be a good fit to replace existing code and such.
* I am using EFCore with a custom Identity (see BugNet.Data).
  * I separated the Identity Context and BugNet's context
  * Identity uses a custom User impl (ApplicationUser) which uses a Guid data type as the key
* BugNet tables are in their own schema.
* Swapped out Log4Net for Serilog as it offers more options for storage than Log4Net
  * Logs are still stored in the database, however Microsoft logs are configured to only log warnings and above
  * The default for other logs are Warning (running locally is Information), however I configured and stored a LoggingLevelSwitch in DI if you want to dynamically change the level at any time at runtime. This feature will probably behind some admin UI in the future.
  * There is a known issue with getting the username for the logging context, probably due to what context Serilog passes to it's middleware.
* The EmailDataProtectorTokenProvider has been overridden to make the token lifespan only 3 hours, the default is 5 days.
* The default UserManager has been overridden and the ApplicationUserManager is the new impl, the reason for this was to override the Authenticator key and token creation and validation to allow the keys and the codes to be encrypted in the database.  It uses the settings:
  * ```BugNet:TwoFactorAuthentication:EncryptionKey```
  * ```BugNet:TwoFactorAuthentication:EncryptionEnabled```
* 2FA (Two-Factor Authentication) has been configured with a QR Code using the library QRCoder and QRCoder-ImageSharp, this because the Microsoft Recommend solution uses System.Drawing and a JS library, however System.Drawing is not cross-platform so it may not work outside of windows.
  
## General

* I have removed jQuery, the validation is replaced with [aspnet-client-validation](https://github.com/haacked/aspnet-client-validation).
* The Identity pages have been scaffolded out and the style's of them are being updated with a more modern consistent look and feel.
* Text is being moved into resource files for locality support

## Running Locally

Notes around running locally

### Configuration

* The project by default set to run under the "Local" environment, this means that you will need to create an appSettings.Local.json in the project root and copy the contents of appSettings.json to it and then update the config with your settings.  You can also bypass this and create user secrets on your local machine for the settings.
  * NOTE the appSettings.Local.json file is ignored by git in the .gitignore file
  
#### Authentication

* One external login provider (Microsoft) is configured by default.  If you want to remove this you can simply remove the registration "AddMicrosoftAccount" in the Program.cs file.
  * If you want to try it then you will need to follow the instructions [here](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-6.0) and configure your own Application in Azure for it.
* Encryption of the Authenticator Key and Codes can be enabled with the following settings to test:
  * ```BugNet:TwoFactorAuthentication:EncryptionKey```
    * You will need to generate a 32 byte key
  * ```BugNet:TwoFactorAuthentication:EncryptionEnabled```
    * You will need to set to true
  