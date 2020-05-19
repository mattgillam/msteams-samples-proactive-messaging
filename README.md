---
page_type: sample
languages:
- csharp
products:
- dotnet
description: "Two samples to highlight solutions to two challenges with building proactive messaging apps in Microsoft Teams"
urlFragment: "update-this-to-unique-url-stub"
---

# Official Microsoft Sample

<!-- 
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

Two samples to highlight solutions to two challenges with building proactive messaging apps in Microsoft Teams. 

## Contents

| File/folder          | Description                                                                 |
|----------------------|-----------------------------------------------------------------------------|
| `/coordinate-logger` | Sample of getting conversation coordinates using BotFramework Events & SDK. |
| `/proactive-cmd`     | Sample of sending proactive messages with throttling policies.              |
| `/teamsAppManifest`  | App manifest for the teams app.                                             |
| `.gitignore`         | Define what to ignore at commit time.                                       |
| `CHANGELOG.md`       | List of changes to the sample.                                              |
| `CONTRIBUTING.md`    | Guidelines for contributing to the sample.                                  |
| `README.md`          | This README file.                                                           |
| `LICENSE`            | The license for the sample.                                                 |

## Prerequisites
- Microsoft Teams account
- [.NET Core 3.x SDK](https://dotnet.microsoft.com/download)
- Publicly addressable https url or tunnel such as [ngrok](https://ngrok.com/) or [Tunnel Relay](https://github.com/OfficeDev/microsoft-teams-tunnelrelay) 

## Setup
1. Configure public url to point to http port 5000
```bash
# ngrok http -host-header=rewrite 5000
```

2. Create a Bot Registration
Either through [App Studio](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/app-studio-overview) or the Azure portal, create a [Bot Framework registration resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration). (The 'Bot' tab in App Studio).

3. Modify the `manifest.json` in the `/teamsAppManifest` folder and replace the `<<BOT-ID>>` with the id from step 2.

4. Zip the contents of `teamsAppManifest` into a `manifest.zip`.

5. Modify the `/coordinate-logger/appsettings.local.json` and fill in the `{{ Bot Id }}` and `{{ Bot Password }}` with the id from step 2.

## Running the samples

### Coordinate Logger
***Note this is a noisy application, it is recommended you not run this in shared teams where you would disturb other users!***

1. Start the application

    ```bash
    # dotnet run
    ```
    It is running when the following output is displayed.
    ```
    Hosting environment: Development
    Content root path: C:\msteams-samples-proactive-messaging\coordinate-logger
    Now listening on: https://localhost:5001
    Now listening on: http://localhost:5000
    Application started. Press Ctrl+C to shut down.
    ```

2. Install the app personally in Teams
    - Go to the `Apps` in the left rail
    - Select `Upload a custom app` and select the manifest.json file
    - Click the `Add` Button on the consent screen

    The coordinates of the user should be in the console window

3. Install the app to a Team
    - Go to the `Apps` in the left rail
    - Select `Upload a custom app` and select the manifest.json file
    - Select the down arrow next to the `Add` Button on the consent screen
    - Select a Team to install to. 

    The app will send a message to each channel in the Team & log the coordinates
    for each new thread to the console window.

## Proactive CMD

1. Send a message to a user
    Using the values from the Coordinate Logger for a User's conversation coordinates & Bot registration fill in the parameters to the following command.

    ```bash
    # dotnet run -- sendUserMessage --app-id="{{Bot Id}}" --app-password="{{Bot Password}}" --service-url="{{ServiceUrl}}" --conversation-id="{{Conversation Id}}" --message="Send Message to a User"
    ```

    This will send a message to the 1-on-1 conversation with the user

2. Send a message to a thread
    Using the values from the Coordinate Logger for a Channel Thread's conversation coordinates & Bot registration fill in the parameters to the following command.

    ```bash
    # dotnet run -- sendChannelThread --app-id="{{Bot Id}}" --app-password="{{Bot Password}}" --service-url="{{ServiceUrl}}" --conversation-id="{{Conversation Id}}" --message="Send Message to a Thread"
    ```

    This will send a message to the thread

## Key concepts

The two samples correspond with two of the most common challenges when building proactive messaging apps in Microsoft Teams, getting the conversation coordinates and sending messages reliably. 

The `CoordinateLoggerActivityHandler.cs` file in the Conversation Logger demonstrates three separate ways to obtain or generate conversation coordinates for users or channel threads using the BotBuilder SDK.

The `SendWithRetries` function and policy from the `CreatePolicy` function demonstrate how to reliably send messages when dealing with throttling from Microsoft Teams. 

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
