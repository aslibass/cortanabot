namespace LuisBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class CoffeeQuery
    {
        [Prompt("Under what name shall i put this order through?")]
        [Optional]
        public string CoffeeOwner { get; set; }

        [Prompt("What Type of Coffee Would you like?")]
        [Optional]
        public string CoffeeType { get; set; }

        [Prompt("What Size did you want?")]
        [Optional]
        public string HeatLevel { get; set; }

        [Prompt("Double Shot, Regular Shot, or Decaf?")]
        [Optional]
        public string CoffeeStrength { get; set; }

        [Prompt("Full Cream Milk, Skim, Almond or Soy?")]
        [Optional]
        public string MilkType { get; set; }

        [Prompt("How many Sugars?")]
        [Optional]
        public string SpoonsOfSugar { get; set; }

        [Prompt("Add Hazelnut or Caramel Flavouring?")]
        [Optional]
        public string Flavour { get; set; }

        [Prompt("Did you want Sugar?")]
        [Optional]
        public string Sugar { get; set; }
    }
}