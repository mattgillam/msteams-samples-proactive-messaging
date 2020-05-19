﻿using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Polly;
using Polly.CircuitBreaker;

namespace Microsoft.Teams.Samples.ProactiveMessageCmd
{
    class Program
    {
        static readonly Random random = new Random();

        static IAsyncPolicy CreatePolicy() {
            var transientRetryPolicy = Policy
                    .Handle<ErrorResponseException>(ex => ex.Message.Contains("429"))
                    .WaitAndRetryAsync(
                        retryCount: 3, 
                        (attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(random.Next(0, 1000)));

            var circuitBreakerPolicy = Policy
                .Handle<ErrorResponseException>(ex => ex.Message.Contains("429"))
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 5, TimeSpan.FromMinutes(10));
            
            var outerRetryPolicy = Policy
                .Handle<BrokenCircuitException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    (_) => TimeSpan.FromMinutes(10));
            return
                outerRetryPolicy.WrapAsync(
                    circuitBreakerPolicy.WrapAsync(
                        transientRetryPolicy));
        }

        static readonly IAsyncPolicy RetryPolicy = CreatePolicy();

        static Task SendWithRetries(Func<Task> callback)
        {
            return RetryPolicy.ExecuteAsync(callback);
        }

        static Task<int> Main(string[] args)
        {
            static string NonNullOrWhitespace(ArgumentResult symbol)
            {
                return symbol.Tokens
                    .Where(token => string.IsNullOrWhiteSpace(token.Value))
                    .Select(token => $"{symbol.Argument.Name} cannot be null or empty")
                    .FirstOrDefault();
            }

            var appIdOption = new Option<string>("--app-id") 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            appIdOption.Argument.AddValidator(NonNullOrWhitespace);

            var appPasswordOption = new Option<string>("--app-password") 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            appPasswordOption.Argument.AddValidator(NonNullOrWhitespace);

            var messageOption = new Option<string>(new string[] { "--message", "-m" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            messageOption.Argument.AddValidator(NonNullOrWhitespace);

            var serviceUrlOption = new Option<string>(new string[] { "--service-url", "-s" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            serviceUrlOption.Argument.AddValidator(NonNullOrWhitespace);

            var conversationIdOption = new Option<string>(new string[] { "--conversation-id",  "-c" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            conversationIdOption.Argument.AddValidator(NonNullOrWhitespace);

            var channelIdOption = new Option<string>(new string[] { "--channel-id", "-c" }) 
            {
                Argument = new Argument<string> { Arity = ArgumentArity.ExactlyOne },
                Required = true
            };
            channelIdOption.Argument.AddValidator(NonNullOrWhitespace);

            var notifyOption = new Option<bool>("--notify")
            {
                Argument = new Argument<bool> { Arity = ArgumentArity.ExactlyOne }
            };

            var sendUserMessageCommand = new Command("sendUserMessage", "Send a message to the conversation coordinates")
            {
                appIdOption,
                appPasswordOption,
                serviceUrlOption,
                conversationIdOption,
                messageOption,
                notifyOption
            };
            sendUserMessageCommand.Handler = CommandHandler.Create<string, string, string, string, string>(SendToUserAsync);

            var createChannelThreadCommand = new Command("createThread", "Create a new thread in a channel")
            {
                appIdOption,
                appPasswordOption,
                serviceUrlOption,
                channelIdOption,
                messageOption
            };
            createChannelThreadCommand.Handler = CommandHandler.Create<string, string, string, string, string>(CreateChannelThreadAsync);

            var sendChannelThreadMessageCommand = new Command("sendChannelThread", "Send a message to a channel thread")
            {
                appIdOption,
                appPasswordOption,
                serviceUrlOption,
                conversationIdOption,
                messageOption,
                notifyOption
            };
            sendChannelThreadMessageCommand.Handler = CommandHandler.Create<string, string, string, string, string>(SendToThreadAsync);

            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                sendUserMessageCommand,
                createChannelThreadCommand,
                sendChannelThreadMessageCommand
            };


            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args);
        }

        public static async Task SendToUserAsync(string appId, string appPassword, string serviceUrl, string conversationId, string message)
        {
            var activity = MessageFactory.Text(message);
            activity.Summary = message;
            activity.TeamsNotifyUser();
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);

            var credentials = new MicrosoftAppCredentials(appId, appPassword);

            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            await SendWithRetries(async () => 
                    await connectorClient.Conversations.SendToConversationAsync(conversationId, activity));
        }

        public static async Task SendToThreadAsync(string appId, string appPassword, string serviceUrl, string conversationId, string message)
        {
            var activity = MessageFactory.Text(message);
            activity.Summary = message;
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);

            var credentials = new MicrosoftAppCredentials(appId, appPassword);

            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);
            await SendWithRetries(async () => 
                    await connectorClient.Conversations.SendToConversationAsync(conversationId, activity));
        }

        public static async Task CreateChannelThreadAsync(string appId, string appPassword, string serviceUrl, string channelId, string message)
        {
            // Create the connector client using the service url & the bot credentials.
            var credentials = new MicrosoftAppCredentials(appId, appPassword);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), credentials);

            // Ensure the service url is marked as "trusted" so the SDK will send auth headers
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);
            
            // Craft an activity from the message
            var activity = MessageFactory.Text(message);

            var conversationParameters = new ConversationParameters
            {
                  IsGroup = true,
                  ChannelData = new TeamsChannelData
                  {
                      Channel = new ChannelInfo(channelId),
                  },
                  Activity = activity
            };
            
            await connectorClient.Conversations.CreateConversationAsync(conversationParameters);
        }
    }
}