namespace LuisBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using System.Text;

    [Serializable]
    [LuisModel("b6399087-dea1-480b-8dcb-d56a007340ce", "5c226b04f1044ae88ad7442ccde33eea")]
    public class RootLuisDialog : LuisDialog<object>
    {
        //Entities from the CoffeeLuis LUIS APP
        private const string EntityCoffeeStrength = "coffeeStrength";
        private const string EntityCoffeeType = "coffeeType";
        private const string EntityFlavouring = "flavouring";
        private const string EntityHeatLevel = "heatLevel";
        private const string EntityMilkOption = "milkoption";
        private const string EntitySize = "size";
        private const string EntitySpoonsOfSugar = "spoonsOfSugar";
        private const string EntitySugar = "sugar";

        //Intents from the CoffeeLuis LUIS APP
        public const string IntentOrder = "Order";
        public const string IntentHelp = "Help";
        /*public const string IntentCappuccino = "Cappuccino";
        public const string IntentChaiLatte = "ChaiLatte";
        public const string IntentEspressos = "Espressos";
        public const string IntentFlatWhite = "FlatWhite";
        public const string IntentLatte = "Latte";
        public const string IntentLongBlack = "LongBlack";
        public const string IntentMocha = "Mocha";
        */

        private const string EntityGeographyCity = "builtin.geography.city";
        private const string EntityHotelName = "Hotel";
        private const string EntityAirportCode = "AirportCode";
        private IList<string> titleOptions = new List<string> { "“Very stylish, great stay, great staff”", "“good hotel, awful meals”", "“Needs more attention to little things”", "“Lovely small hotel ideally situated to explore the area.”", "“Positive surprise”", "“Beautiful suite and resort”" };

        /// <summary>
        /// Need to override the LuisDialog.MessageReceived method so that we can detect when the user invokes the skill without
        /// specifying a phrase, for example: "Open Hotel Finder", or "Ask Hotel Finder". In these cases, the message received will be an empty string
        /// </summary>
        /// <param name="context"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            // Check for empty query
            var message = await item;
            if (message.Text == null)
            {
                // Return the Help/Welcome
                await Help(context, null);
            }
            else
            {
                await base.MessageReceived(context, item);
            }
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            var response = context.MakeMessage();
            response.Text = $"Sorry, I did not understand '{result.Query}'. Use 'help' if you need assistance.";
            response.Speak = $"Sorry, I did not understand '{result.Query}'. Say 'help' if you need assistance.";
            response.InputHint = InputHints.AcceptingInput;

            await context.PostAsync(response);

            context.Wait(this.MessageReceived);
        }
        

        //The user want a Cappuccino
        [LuisIntent(IntentOrder)]
        public async Task OnIntent(IDialogContext context, LuisResult result)
        {
            //TEST QUERY IS 
            // I would like to order a double shot cappuccino with two sugars and a shot of hazelnut
            var progressMessage = context.MakeMessage();
            progressMessage.Summary = progressMessage.Speak = $"Ready to setup your Order";
            progressMessage.InputHint = InputHints.IgnoringInput;
            await context.PostAsync(progressMessage);
            
            //set up coffee object from the entities
            //Coffee mycoffee = this.BotEntityRecognition(result.Intents[0].Intent, result);

            // Use a FormDialog to query the user for missing destination, if necessary
            
            CoffeeQuery coffeeQuery = this.CoffeeEntityRecognition(result.Intents[0].Intent, result);

            var coffeeFormDialog = new FormDialog<CoffeeQuery>(coffeeQuery, this.BuildCoffeeForm, FormOptions.PromptInStart, result.Entities);
            context.Call(coffeeFormDialog, this.ResumeAfterCoffeeFormDialog);

            
        }

        //Show the result of the LUIS Intent
        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            // get recognized entities
            //string entities = this.BotEntityRecognition(result.Intents[0].Intent, result);

            // round number
            // string roundedScore = result.Intents[0].Score != null ? (Math.Round(result.Intents[0].Score.Value, 2).ToString()) : "0";

            //await context.PostAsync($"**Query**: {result.Query}, **Intent**: {result.Intents[0].Intent}, **Score**: {roundedScore}. **Entities**: {entities}");
            await context.PostAsync($"**Query**: {result.Query}");
            context.Wait(MessageReceived);
        }

        public CoffeeQuery CoffeeEntityRecognition(string intentName, LuisResult result)
        {
            IList<EntityRecommendation> listOfEntitiesFound = result.Entities;
            //new cup of coffee
            CoffeeQuery mycoffee = new CoffeeQuery();
            //mycoffee.SetCommonOptions();
            mycoffee.CoffeeType = result.Intents[0].Intent;

            //initialize to most common options


            foreach (EntityRecommendation item in listOfEntitiesFound)
            {
                if (item.Type == EntityCoffeeStrength)
                {
                    mycoffee.CoffeeStrength = item.Entity;
                }
                else if (item.Type == EntityCoffeeType)
                {
                    mycoffee.CoffeeType = item.Entity;
                }
                else if (item.Type == EntityFlavouring)
                {
                    mycoffee.Flavour = item.Entity;
                }
                else if (item.Type == EntityHeatLevel)
                {
                    mycoffee.HeatLevel = item.Entity;
                }
                else if (item.Type == EntityMilkOption)
                {
                    mycoffee.MilkType = item.Entity;
                }
                else if (item.Type == EntitySize)
                {
                   // mycoffee.Size= item.Entity;
                }
                else if (item.Type == EntitySpoonsOfSugar)
                {
                    mycoffee.SpoonsOfSugar = item.Entity;
                }
                else if (item.Type == EntitySugar)
                {
                    mycoffee.Sugar = item.Entity;
                }
                else
                {
                    //add error handling code
                }
            }

            return mycoffee;
            //return mycoffee.ToString();
        }


        //Collect the entities under the Intent that define the sugars, size, flavours etc related to the coffee
        public Coffee BotEntityRecognition(string intentName, LuisResult result)
        {
            IList<EntityRecommendation> listOfEntitiesFound = result.Entities;
            //new cup of coffee
            var mycoffee = new Coffee();
            //mycoffee.SetCommonOptions();
            mycoffee.CoffeeType = result.Intents[0].Intent;

            //initialize to most common options


            foreach (EntityRecommendation item in listOfEntitiesFound)
            {
                if (item.Type == EntityCoffeeStrength)
                {
                    mycoffee.CoffeeStrength = item.Entity;
                }
                else if (item.Type == EntityCoffeeType)
                {
                    mycoffee.CoffeeType = item.Entity;
                }
                else if (item.Type == EntityFlavouring)
                {
                    mycoffee.Flavour = item.Entity;
                }
                else if (item.Type == EntityHeatLevel)
                {
                    mycoffee.HeatLevel = item.Entity;
                }
                else if (item.Type == EntityMilkOption)
                {
                    mycoffee.MilkType = item.Entity;
                }
                else if (item.Type == EntitySize)
                {
                    mycoffee.Size = item.Entity;
                }
                else if (item.Type == EntitySpoonsOfSugar)
                {
                    mycoffee.SpoonsOfSugar = item.Entity;
                }
                else if (item.Type == EntitySugar)
                {
                    mycoffee.Sugar = item.Entity;
                }
                else
                {
                    //add error handling code
                }
            }

            return mycoffee;
            //return mycoffee.ToString();
        }

                    
        

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            var response = context.MakeMessage();
            response.Summary = "Try asking me things like 'order a cappuccino with one sugar' or 'I want a decaf mocha with a shot of caramel' ";
            response.Speak = @"<speak version=""1.0"" xml:lang=""en-US"">Hi! Hi! Try asking me things like 'order a cappuccino with one sugar', "
                + @" <break time=""200ms""/>', or 'I want a decaf mocha with a shot of caramel'</speak>";
            response.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(response);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Goodbye")]
        public async Task Goodbye(IDialogContext context, LuisResult result)
        {
            var goodByeMessage = context.MakeMessage();
            goodByeMessage.Summary = goodByeMessage.Speak = "Thanks for using the Smart Coffee Cart!";
            goodByeMessage.InputHint = InputHints.IgnoringInput;
            await context.PostAsync(goodByeMessage);

            var completeMessage = context.MakeMessage();
            completeMessage.Type = ActivityTypes.EndOfConversation;
            completeMessage.AsEndOfConversationActivity().Code = EndOfConversationCodes.CompletedSuccessfully;

            await context.PostAsync(completeMessage);

            context.Done(default(object));
        }

        private IForm<CoffeeQuery> BuildCoffeeForm()
        {
            OnCompletionAsyncDelegate<CoffeeQuery> processCoffeeOrder = async (context, state) =>
            {
                var message = "Making sure I got all your requirements";
                var speech = @"<speak version=""1.0"" xml:lang=""en-US"">Making sure I got all your requirements";
                if (!string.IsNullOrEmpty(state.CoffeeOwner))
                {
                    message += $" in { state.CoffeeOwner}...";
                    speech += $" in { state.CoffeeOwner}...";
                }
                else if (!string.IsNullOrEmpty(state.CoffeeType))
                {
                    message += $" in { state.CoffeeType}...";
                    speech += $" in { state.CoffeeType}...";
                }
                else if (!string.IsNullOrEmpty(state.CoffeeStrength))
                {
                    message += $" in { state.CoffeeStrength}...";
                    speech += $" in { state.CoffeeStrength}...";
                }
                else if (!string.IsNullOrEmpty(state.MilkType))
                {
                    message += $" in { state.MilkType}...";
                    speech += $" in { state.MilkType}...";
                }
                else if (!string.IsNullOrEmpty(state.SpoonsOfSugar))
                {
                    message += $" in { state.SpoonsOfSugar} Sugars...";
                    speech += $" in { state.SpoonsOfSugar} Sugars...";
                }
                speech += "</speak>";

                var response = context.MakeMessage();
                response.Summary = message;
                response.Speak = speech;
                response.InputHint = InputHints.IgnoringInput;
                await context.PostAsync(response);
            };
            return new FormBuilder<CoffeeQuery>()
                .AddRemainingFields()
                .OnCompletion(processCoffeeOrder)
                .Build();

            
            /*return new FormBuilder<CoffeeQuery>()
                .Field(nameof(CoffeeQuery.CoffeeOwner))
                .Field(nameof(CoffeeQuery.CoffeeStrength))
                .Field(nameof(CoffeeQuery.MilkType))
                .Field(nameof(CoffeeQuery.Flavour))
                .Field(nameof(CoffeeQuery.SpoonsOfSugar))
                .OnCompletion(processCoffeeOrder)
                .Build();
                */


        }

        private async Task ResumeAfterCoffeeFormDialog(IDialogContext context, IAwaitable<CoffeeQuery> result)
        {
            try
            {
                Coffee mycoffee = await this.GetCoffeeAsync(result);

                // We show results differently depending on whether this is a Voice-only, or Voice+screen client
                bool HasDisplay = true;
                var messageActivity = context.Activity.AsMessageActivity();
                if (messageActivity.Entities != null)
                {
                    foreach (var entity in messageActivity.Entities)
                    {
                        if (entity.Type == "DeviceInfo")
                        {
                            dynamic deviceInfo = entity.Properties;
                            HasDisplay = (bool)deviceInfo.supportsDisplay;
                        }
                    }
                }
                if (HasDisplay)
                {
                    await PresentCoffeeResultsVisual(context, mycoffee);
                }
                else
                {
                   // await PresentResultsVoiceOnly(context, mycoffee);
                }
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }
                var errorMessage = context.MakeMessage();
                errorMessage.Text = errorMessage.Speak = reply;
                errorMessage.InputHint = InputHints.IgnoringInput;
                await context.PostAsync(errorMessage);
            }
        }

        private async Task<Coffee> GetCoffeeAsync(IAwaitable<CoffeeQuery> result)
        {
            var coffeeQuery = await result;
            var mycoffee = new Coffee();
            //mycoffee.SetCommonOptions();
            mycoffee.coffeeOwner = coffeeQuery.CoffeeOwner;
            mycoffee.CoffeeStrength = coffeeQuery.CoffeeStrength;
            mycoffee.CoffeeType = coffeeQuery.CoffeeType;
            mycoffee.Flavour = coffeeQuery.Flavour;
            mycoffee.MilkType = coffeeQuery.MilkType;
            mycoffee.HeatLevel = coffeeQuery.HeatLevel;
            mycoffee.Size = "Regular";
            mycoffee.Sugar = coffeeQuery.Sugar;
            mycoffee.SpoonsOfSugar = coffeeQuery.SpoonsOfSugar;
            if (mycoffee.CoffeeType.ToUpper().Contains("CAPPUCCINO"))
            {
                mycoffee.Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/16/Classic_Cappuccino.jpg/1200px-Classic_Cappuccino.jpg";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("FLAT WHITE"))
            {
                mycoffee.Image = "http://muslimeater.com/wp-content/uploads/2015/09/flat_white_russell_james_smith-1024x683.jpg";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("LONG BLACK"))
            {
                mycoffee.Image = "http://www.perfectcoffeeatwork.com.au/images/Long%20Black.JPG";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("SHORT BLACK"))
            {
                mycoffee.Image = "http://www.perfectcoffeeatwork.com.au/images/Short%20Black%20Glass%203.JPG";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("MOCHA"))
            {
                mycoffee.Image = "http://2.bp.blogspot.com/-I0rdxZj_dwk/UFZQs22fSBI/AAAAAAAAAKw/byN1OWiehWI/s1600/ToffeeMocha.JPG";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("LONG MACCHIATO"))
            {
                mycoffee.Image = "http://teamberkeley.files.wordpress.com/2010/07/img_0695.jpg";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("SHORT MACCHIATO"))
            {
                mycoffee.Image = "http://www.gbcoffee.com.au/shop/images/coffees/macchiato_short.jpg";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("HOT CHOCOLATE"))
            {
                mycoffee.Image = "http://del.h-cdn.co/assets/16/47/1479838584-delish-best-hot-chocolate-pin-1.jpg";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("CHAI"))
            {
                mycoffee.Image = "http://img1.cookinglight.timeinc.net/sites/default/files/image/2016/09/main/1610p12-turmeric-chai-latte.jpg";
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("TEA"))
            {
                mycoffee.Image = "http://www.lebensbaum.com/sites/default/files/5256_pa.png";
            }
            else
            {
                mycoffee.Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/16/Classic_Cappuccino.jpg/1200px-Classic_Cappuccino.jpg";
            }

            return mycoffee;
        }

        private async Task PresentCoffeeResultsVisual(IDialogContext context, Coffee mycoffee)
        {
            var progressMessage = context.MakeMessage();
            progressMessage.Summary = progressMessage.Speak = $"Here is what you have ordered. Click confirm to send your order to the Barista:";
            progressMessage.InputHint = InputHints.IgnoringInput;
            await context.PostAsync(progressMessage);

            var resultMessage = context.MakeMessage();
            resultMessage.InputHint = InputHints.AcceptingInput;
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();
            HeroCard heroCard = new HeroCard()
            {
                Title = mycoffee.CoffeeType,
                Subtitle = mycoffee.ToString(),
                Images = new List<CardImage>()
                       {
                           new CardImage() { Url = mycoffee.Image }

                        },
                Buttons = new List<CardAction>()
                       {
                          new CardAction()
                          {
                              Title = "Place Order",

                                Type = ActionTypes.OpenUrl,

                                Value = $"https://en.wikipedia.org/wiki/Cappuccino"

                            }

                        }

            };
            resultMessage.Attachments.Add(heroCard.ToAttachment());
           
            await context.PostAsync(resultMessage);
        }

        private async Task PresentResultsVisual(IDialogContext context, IEnumerable<Hotel> hotels)
        {
            var progressMessage = context.MakeMessage();
            progressMessage.Summary = progressMessage.Speak = $"I found {hotels.Count()} hotels. Showing them for you now:";
            progressMessage.InputHint = InputHints.IgnoringInput;
            await context.PostAsync(progressMessage);

            var resultMessage = context.MakeMessage();
            resultMessage.InputHint = InputHints.AcceptingInput;
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();

            foreach (var hotel in hotels)
            {
                HeroCard heroCard = new HeroCard()
                {
                    Title = hotel.Name,
                    Subtitle = $"{hotel.Rating} stars. {hotel.NumberOfReviews} reviews. From ${hotel.PriceStarting} per night.",
                    Images = new List<CardImage>()
                        {
                            new CardImage() { Url = hotel.Image }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                            }
                        }
                };

                resultMessage.Attachments.Add(heroCard.ToAttachment());
            }

            await context.PostAsync(resultMessage);
        }


        private async Task PresentResultsVoiceOnly(IDialogContext context, IEnumerable<Hotel> hotels)
        {
            // For voice, we'll limit results to first three otherwise it gets to be too long going through a long list using voice.
            // Aa well designed skill would offer the user the option to hear "Next Results" if the first ones don't interest them.
            // Not implemented in this sample.

            var hotelList = hotels.ToList();
            context.ConversationData.SetValue<List<Hotel>>("Hotels", hotelList);

            // Array of strings for the PromptDialog.Choice buttons - though note these are not spoken, just for debugging use
            var descriptions = new List<string>();

            // Build the spoken prompt listing the results
            var speakText = new StringBuilder();

            speakText.Append($"Here are the first three results: ");
            for (int count = 1; count < 4; count++)
            {
                var hotel = hotelList[count - 1];
                descriptions.Add($"{hotel.Name}");
                //speakText.Append($"{count}: {hotel.Name}, {hotel.Rating} stars, from ${hotel.PriceStarting} per night. ");
                speakText.Append($"{count}, {hotel.Name}, from ${hotel.PriceStarting}. ");
            }
            // Send the spoken message listing the options separately from the PromptDialog
            // Currently, PromptDialog built-in recognizer does not work if you have too long a 'speak' 
            // phrase (bug) before the user speaks their choice, so say most ahead of the choice dialog
            var resultsMessage = context.MakeMessage();
            resultsMessage.Speak = speakText.ToString();
            resultsMessage.InputHint = InputHints.IgnoringInput;
            await context.PostAsync(resultsMessage);

            // Define the choices, plus synonyms for each choice - include the hotel name
            var choices = new Dictionary<string, IReadOnlyList<string>>()
             {
                { "1", new List<string> { "one", hotelList[0].Name, hotelList[0].Name.ToLowerInvariant() } },
                { "2", new List<string> { "two", hotelList[1].Name, hotelList[1].Name.ToLowerInvariant() } },
                { "3", new List<string> { "three", hotelList[2].Name, hotelList[2].Name.ToLowerInvariant() } },
            };

            var promptOptions = new PromptOptionsWithSynonyms<string>(
                prompt: "notused", // prompt is not spoken
                choices: choices,
                descriptions: descriptions,
                speak: SSMLHelper.Speak($"Which one do you want to hear more about?"));

            PromptDialog.Choice(context, HotelChoiceReceivedAsync, promptOptions);
        }

        private async Task HotelChoiceReceivedAsync(IDialogContext context, IAwaitable<string> result)
        {

            int choiceIndex = 0;
            int.TryParse(await result, out choiceIndex);

            List<Hotel> hotelList;
            if (context.ConversationData.TryGetValue<List<Hotel>>("Hotels", out hotelList))
            {
                var hotel = hotelList[choiceIndex - 1];
                var resultsMessage = context.MakeMessage();
                resultsMessage.Speak = $"You chose: {hotel.Name}, {hotel.Rating} stars, from ${hotel.PriceStarting} per night. ";
                resultsMessage.InputHint = InputHints.IgnoringInput;
                await context.PostAsync(resultsMessage);

                StringBuilder bld = new StringBuilder("Here are some recent reviews: ");

                for (int i = 0; i < 3; i++)
                {
                    var random = new Random(i);
                    bld.AppendLine(this.titleOptions[random.Next(0, this.titleOptions.Count - 1)]);
                }
                var endMessage = context.MakeMessage();
                endMessage.Speak = bld.ToString();
                endMessage.InputHint = InputHints.AcceptingInput; // We're basically done, but they could ask another query if they wanted
                await context.PostAsync(endMessage);
            }

            context.Wait(this.MessageReceived);
        }

        private async Task<IEnumerable<Hotel>> GetHotelsAsync(HotelsQuery searchQuery)
        {
            var hotelNames = new List<string>()
                {"Excellent", "Splendid", "Supreme", "Excelsior", "High Class" };
            var hotels = new List<Hotel>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Hotel hotel = new Hotel()
                {
                    //Name = $"{searchQuery.Destination ?? searchQuery.AirportCode} Hotel {i}",
                    Name = $"{hotelNames[i - 1]} Hotel",
                    Location = searchQuery.Destination ?? searchQuery.AirportCode,
                    Rating = random.Next(1, 5),
                    NumberOfReviews = random.Next(0, 5000),
                    PriceStarting = random.Next(80, 450),
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260"
                };

                hotels.Add(hotel);
            }

            hotels.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            // Waste some time to simulate database search
            await Task.Delay(3000);

            return hotels;
        }
    }

       
    
    static class StringExtensions
    {
        public static string Capitalize(this string input)
        {
            var output = string.Empty;
            if (!string.IsNullOrEmpty(input))
            {
                output = input.Substring(0, 1).ToUpper() + input.Substring(1);
            }
            // Strip out periods 
            output = output.Replace(".", "");

            return output;
        }
    }
}
