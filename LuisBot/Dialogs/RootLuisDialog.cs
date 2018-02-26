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
    using Newtonsoft.Json;
    using System.Net.Http;

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

        public Coffee myCoffeeOrder;

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
            
            //Setup the global coffee order basics gleaned from the incoming entities.
            myCoffeeOrder = this.BotEntityRecognition(result.Intents[0].Intent, result);

            //Drive a form to get missing information at the first instance
            CoffeeQuery coffeeQuery = this.CoffeeEntityRecognition(result.Intents[0].Intent, result);
            myCoffeeOrder = this.BotEntityRecognition(result.Intents[0].Intent, result);
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
                    //mycoffee.HeatLevel = item.Entity;
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
            mycoffee.SetCommonOptions();
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
            response.Speak = @"<speak version=""1.0"" xml:lang=""en-US"">Hi! Try asking me things like 'order a cappuccino with one sugar', "
                + @" <break time=""200ms""/>', or 'I want a decaf mocha with a shot of caramel'</speak>";
            response.InputHint = InputHints.ExpectingInput;

            await context.PostAsync(response);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("ConfirmOrder")]
        public async Task ConfirmOrder(IDialogContext context, LuisResult result)
        {

            string orderJSON = MakeJSON(myCoffeeOrder);
            
            /*using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://readify-prod-smartcoffee.azurewebsites.net/api/orders",
                     new StringContent(orderJSON, Encoding.UTF8, "application/json"));
                await context.PostAsync($"Sent the order JSON as: {orderJSON}");
            }
            */
            await context.PostAsync($"Code Commented Out to Send the order JSON as: {orderJSON}");

            var goodByeMessage = context.MakeMessage();
            goodByeMessage.Summary = goodByeMessage.Speak = "Your order has been sent. Please check the display at the Coffee Cart for the status of your order. Goodbye.";
            goodByeMessage.InputHint = InputHints.IgnoringInput;
            await context.PostAsync(goodByeMessage);

            var completeMessage = context.MakeMessage();
            completeMessage.Type = ActivityTypes.EndOfConversation;
            completeMessage.AsEndOfConversationActivity().Code = EndOfConversationCodes.CompletedSuccessfully;

            await context.PostAsync(completeMessage);

            context.Done(default(object));
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
                    message += $" for { state.CoffeeOwner}...";
                    speech += $" for { state.CoffeeOwner}...";
                }
                else if (!string.IsNullOrEmpty(state.CoffeeType))
                {
                    message += $" for a cup of { state.CoffeeType}...";
                    speech += $" for a cup of { state.CoffeeType}...";
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
                //myCoffeeOrder = 
                await this.GetCoffeeAsync(result);

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
                    await PresentCoffeeResultsVisual(context, myCoffeeOrder);
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

       
        private string MakeJSON(Coffee mycoffee)
        {

            //initialize string 
           
            string myjson = "";
            string name = mycoffee.coffeeOwner;
            int coffeeType = 0; //FLatWhite
            int milkVariant = 0; //None
            int caffeineOption = 0; //Regular
            bool extraShot = false; //NO EXTRA SHOT
            int sugarCount = 0; //NO SUGAR
            int sweetenerCount = 0;//No Sweetner
            

            //coffeeType
            //0 - FlatWhite,
            //1 - ShortMacchiato,
            //2 - LongMacchiato,
            //3 - Latte,
            //4 - Cappuccino,
            //5 - ShortBlack,
            //6 - LongBlack,
            //7 - Mocha,
            //8 - HotChocolate,
            //9 - ChaiLatte,
            //10 - Tea


            if (mycoffee.CoffeeType.ToUpper().Contains("CAPPUCCINO"))
            {
                coffeeType = 4;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("LATTE") && (!mycoffee.CoffeeType.ToUpper().Contains("CHAI"))) //prevent confusion between chailatte and latte
            {
                coffeeType = 3;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("WHITE"))
            {
                coffeeType = 0;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("LONG") && mycoffee.CoffeeType.ToUpper().Contains("BLACK"))
            {
                coffeeType = 6;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("SHORT") && mycoffee.CoffeeType.ToUpper().Contains("BLACK"))
            {
                coffeeType = 5;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("MOCHA"))
            {
                coffeeType = 7;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("LONG") && mycoffee.CoffeeType.ToUpper().Contains("MACCHIATO"))
            {
                coffeeType = 2;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("SHORT") && mycoffee.CoffeeType.ToUpper().Contains("MACCHIATO"))
            {
                coffeeType = 1;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("CHOCOLATE"))
            {
                coffeeType = 8;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("CHAI"))
            {
                coffeeType = 9;
            }
            else if (mycoffee.CoffeeType.ToUpper().Contains("TEA"))
            {
                coffeeType = 10;
            }
            else
            {
                coffeeType = 11;//DID NOT FIND ANY MATCH - For error check. THis should never happen.
            }



            // milkVariant
            //0 - None,
            //1 - FullCream,
            //2 - Skim,
            //3 - LactoseFree, (not supported by the coffee Mob Menu)
            //4 - Soy,
            //5 - Almond


            if (mycoffee.MilkType.ToUpper().Contains("NONE"))
            {
                milkVariant = 0;
            }
            else if (mycoffee.MilkType.ToUpper().Contains("CREAM"))
            {
                milkVariant = 1;
            }
            else if (mycoffee.MilkType.ToUpper().Contains("SKIM"))
            {
                milkVariant = 2;
            }
            else if (mycoffee.MilkType.ToUpper().Contains("LACTOSE"))
            {
                milkVariant = 3;
            }
            else if (mycoffee.MilkType.ToUpper().Contains("SOY"))
            {
                milkVariant = 4;
            }
            else if (mycoffee.MilkType.ToUpper().Contains("ALMOND"))
            {
                milkVariant = 5;
            }
            else
            {
                milkVariant = 6; //SHOULD NEVER HAPPEN
            }


            //caffeineOption
            //(This is an optional field and can be left out when caffeine doesn’t make sense – e.g. if submitting a Hot Chocolate)
            //0 - Regular,
            //1 - Decaf
            if (mycoffee.CoffeeStrength.ToUpper().Contains("REGULAR"))
            {
                caffeineOption = 0;
            }
            else if (mycoffee.CoffeeStrength.ToUpper().Contains("DECAF"))
            {
                caffeineOption = 1;
            }
            else if (mycoffee.CoffeeStrength.ToUpper().Contains("DOUBLE") || mycoffee.CoffeeStrength.ToUpper().Contains("EXTRA"))
            {
                extraShot = true;
            }
            else
            {
               caffeineOption = 2; //SHOULD NEVER HAPPEN
            }

            //sugarCount
            //0 - 5
            if (mycoffee.SpoonsOfSugar.ToUpper().Contains("ZERO") || mycoffee.SpoonsOfSugar.ToUpper().Contains("NO") || mycoffee.SpoonsOfSugar.ToUpper().Contains("0") || mycoffee.SpoonsOfSugar.ToUpper().Contains("NONE"))
            {
                sugarCount = 0;
            }
            else if ( mycoffee.SpoonsOfSugar.ToUpper().Contains("ONE") || mycoffee.SpoonsOfSugar.ToUpper().Contains("1"))
            {
                sugarCount = 1;
            }
            else if (mycoffee.SpoonsOfSugar.ToUpper().Contains("TWO") || mycoffee.SpoonsOfSugar.ToUpper().Contains("2"))
            {
                sugarCount = 2;
            }
            else if (mycoffee.SpoonsOfSugar.ToUpper().Contains("THREE") || mycoffee.SpoonsOfSugar.ToUpper().Contains("3"))
            {
                sugarCount = 3;
            }
            else if (mycoffee.SpoonsOfSugar.ToUpper().Contains("FOUR") || mycoffee.SpoonsOfSugar.ToUpper().Contains("4"))
            {
                sugarCount = 4;
            }
            else if (mycoffee.SpoonsOfSugar.ToUpper().Contains("FIVE") || mycoffee.SpoonsOfSugar.ToUpper().Contains("5"))
            {
                sugarCount = 5;
            }
            else
            {
                sugarCount = 6;  //SHOULD NEVER HAPPEN
            }

            if (coffeeType < 11 && milkVariant < 6 && caffeineOption < 2 && sugarCount < 6 )
            {
                KioskOrder order = new KioskOrder();
                order.name = name;
                order.coffeeType = coffeeType;
                order.milkVariant = milkVariant;
                order.caffeineOption = caffeineOption;
                order.extraShot = extraShot;
                order.sugarCount = sugarCount;
                order.sweetenerCount = sweetenerCount;
                order.SetCommonOptions(); //sets orderflow, source and ordermechanism variables to cortana defaults.
                myjson = JsonConvert.SerializeObject(order);


            }
            else
            {
                
                myjson = "NOT MATCHED CORRECTLY TO OPTIONS";
            }


        

            return myjson;

    }

    private async Task GetCoffeeAsync(IAwaitable<CoffeeQuery> result)
        {
            var coffeeQuery = await result;
            //Coffee mycoffee = new Coffee();
            //mycoffee.SetCommonOptions();
            if (!string.IsNullOrEmpty(coffeeQuery.CoffeeOwner))
            {
                myCoffeeOrder.coffeeOwner = coffeeQuery.CoffeeOwner;

            }
            if (!string.IsNullOrEmpty(coffeeQuery.CoffeeStrength))
            {
                myCoffeeOrder.CoffeeStrength = coffeeQuery.CoffeeStrength;
            }
            if (!string.IsNullOrEmpty(coffeeQuery.CoffeeType))
            {
                myCoffeeOrder.CoffeeType = coffeeQuery.CoffeeType;
            }
            if (!string.IsNullOrEmpty(coffeeQuery.Flavour))
            {
                myCoffeeOrder.Flavour = coffeeQuery.Flavour;
            }
            if (!string.IsNullOrEmpty(coffeeQuery.MilkType))
            {
                myCoffeeOrder.MilkType = coffeeQuery.MilkType;
            }
            if (!string.IsNullOrEmpty(coffeeQuery.Sugar))
            {
                myCoffeeOrder.Sugar = coffeeQuery.Sugar;
            }
            if (!string.IsNullOrEmpty(coffeeQuery.SpoonsOfSugar))
            {
                myCoffeeOrder.SpoonsOfSugar = coffeeQuery.SpoonsOfSugar;
            }
           
            if (myCoffeeOrder.CoffeeType.ToUpper().Contains("CAPPUCCINO"))
            {
                myCoffeeOrder.Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/16/Classic_Cappuccino.jpg/1200px-Classic_Cappuccino.jpg";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("FLAT WHITE"))
            {
                myCoffeeOrder.Image = "http://muslimeater.com/wp-content/uploads/2015/09/flat_white_russell_james_smith-1024x683.jpg";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("LONG BLACK"))
            {
                myCoffeeOrder.Image = "http://www.perfectcoffeeatwork.com.au/images/Long%20Black.JPG";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("SHORT BLACK"))
            {
                myCoffeeOrder.Image = "http://www.perfectcoffeeatwork.com.au/images/Short%20Black%20Glass%203.JPG";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("MOCHA"))
            {
                myCoffeeOrder.Image = "http://2.bp.blogspot.com/-I0rdxZj_dwk/UFZQs22fSBI/AAAAAAAAAKw/byN1OWiehWI/s1600/ToffeeMocha.JPG";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("LONG MACCHIATO"))
            {
                myCoffeeOrder.Image = "http://teamberkeley.files.wordpress.com/2010/07/img_0695.jpg";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("SHORT MACCHIATO"))
            {
                myCoffeeOrder.Image = "http://www.gbcoffee.com.au/shop/images/coffees/macchiato_short.jpg";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("HOT CHOCOLATE"))
            {
                myCoffeeOrder.Image = "http://del.h-cdn.co/assets/16/47/1479838584-delish-best-hot-chocolate-pin-1.jpg";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("CHAI"))
            {
                myCoffeeOrder.Image = "http://img1.cookinglight.timeinc.net/sites/default/files/image/2016/09/main/1610p12-turmeric-chai-latte.jpg";
            }
            else if (myCoffeeOrder.CoffeeType.ToUpper().Contains("TEA"))
            {
                myCoffeeOrder.Image = "http://www.lebensbaum.com/sites/default/files/5256_pa.png";
            }
            else
            {
                myCoffeeOrder.Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/16/Classic_Cappuccino.jpg/1200px-Classic_Cappuccino.jpg";
            }

            //return mycoffee;
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
                              Title = "Confirm Order",

                                //Type = ActionTypes.OpenUrl,
                                Type = ActionTypes.ImBack,
                                Value = $"Confirm Order"

                            }

                        }

            };
            resultMessage.Attachments.Add(heroCard.ToAttachment());
           
            await context.PostAsync(resultMessage);

            //await confirmCoffeeOrder(context, resultMessage,mycoffee);
        }

        /* private async Task PresentResultsVisual(IDialogContext context, IEnumerable<Hotel> hotels)
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
        }*/

        /*
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
            
        } */

        /*private async Task HotelChoiceReceivedAsync(IDialogContext context, IAwaitable<string> result)
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
        }*/

        /*private async Task<IEnumerable<Hotel>> GetHotelsAsync(HotelsQuery searchQuery)
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
        }*/
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
