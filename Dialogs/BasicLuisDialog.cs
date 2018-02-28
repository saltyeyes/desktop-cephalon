using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

using Microsoft.Bot.Connector;
using Json2CSharp;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using Cephalon;
using Cephalon.Properties;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [LuisModel("1f849047-fa83-48a9-bc60-503780e95a56", "01af16f6ce5d43c0831095d5fc9aeaf7")]
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        protected WorldState CurrentWorldState
        {
            get
            {
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadString("https://ws.warframestat.us/pc");
                    return JsonConvert.DeserializeObject<WorldState>(json);
                }
            }
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            var response = context.MakeMessage();
            response.Text = Utils.RandomPick(Resources.ResourceManager.GetString("ErrorSSML"));

            await context.PostAsync(response);
            //context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task WelcomeIntent(IDialogContext context, LuisResult result)
        {
            var response = context.MakeMessage();
            response.Text = Utils.RandomPick(Resources.ResourceManager.GetString("GreetingSSML"));

            response.InputHint = InputHints.ExpectingInput;
            await context.PostAsync(response);
            //context.Wait(MessageReceived);
        }


        [LuisIntent("CetusTime")]
        public async Task CetusTimeIntent(IDialogContext context, LuisResult result)
        {
            var response = context.MakeMessage();
            
            var worldState = CurrentWorldState;

            TimeSpan timeRemaining = (worldState.cetusCycle.expiry - DateTimeOffset.Now);
            List<String> timeParts = new List<String>();
            
            if (timeRemaining.Hours >= 1) {
                timeParts.Add(string.Format(Resources.ResourceManager.GetString("TimePartHour"), timeRemaining.Hours, timeRemaining.Hours == 1 ? "" : "s"));
            }
            if (timeRemaining.Minutes >= 1) {
                timeParts.Add(string.Format(Resources.ResourceManager.GetString("TimePartMinute"), timeRemaining.Minutes, timeRemaining.Minutes == 1 ? "" : "s"));
            }
            if (timeRemaining.Seconds >= 1) {
                timeParts.Add(string.Format(Resources.ResourceManager.GetString("TimePartSecond"), timeRemaining.Seconds, timeRemaining.Seconds == 1 ? "" : "s"));
            }

            List<String> timePartResources = new List<string>() {
                "TimeReadableOnePart",
                "TimeReadableTwoParts",
                "TimeReadableThreeParts"
            };

            String timePartStringName = timePartResources[timeParts.Count - 1];
            String timePartString = Resources.ResourceManager.GetString(timePartStringName);

            String timeRemainingPretty = string.Format(timePartString, timeParts.ToArray());

            var card = new HeroCard() {
                Title = string.Format("It's {0} on the Plains.", worldState.cetusCycle.isDay ? "daytime" : "night"),
                Images = new List<CardImage>() {
                    new CardImage(worldState.cetusCycle.isDay ? "https://i.imgur.com/kllwm5a.jpg" : "https://i.imgur.com/UNgWSQe.jpg")
                },
                Buttons = new List<CardAction>() {
                    new CardAction(ActionTypes.ImBack, "Set Alarm", value: "SET_ALARM"),
                }
            };

            var message = context.MakeMessage();

            message.Attachments = new List<Attachment>()
            {
                card.ToAttachment()
            };

            var spoken = string.Empty;
            spoken = string.Format(Resources.ResourceManager.GetString("PlainsTimeSSML"), 
                worldState.cetusCycle.isDay ? "daytime" : "night",
                worldState.cetusCycle.isDay ? "set" : "rise",
                timeRemainingPretty
            );

            message.Speak = SSMLHelper.Speak(spoken);

            //response.Text = spoken;

            //response.InputHint = InputHints.ExpectingInput;

            message.InputHint = InputHints.AcceptingInput;


            await context.PostAsync(message);

            context.Done<object>(null);
            //context.Wait(MessageReceived);
        }
    }
}