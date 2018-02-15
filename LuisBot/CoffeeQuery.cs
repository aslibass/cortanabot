namespace LuisBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class CoffeeQuery
    {
        [Prompt("What Type of Coffee Would you like?")]
        [Optional]
        public string CoffeeType { get; set; }
        
        [Prompt("Small, Regular or Large?")]
        [Optional]
        public string Size { get; set; }

        [Prompt("Double Shot, Regular Shot, or Decaf?")]
        [Optional]
        public string CoffeeStrength { get; set; }

        [Prompt("Full Cream Milk, Skim, Almond or Soy?")]
        [Optional]
        public string MilkType { get; set; }

        [Prompt("How many Sugars?")]
        [Optional]
        public string SpoonsOfSugars { get; set; }

        [Prompt("Add Hazelnut or Caramel Flavouring?")]
        [Optional]
        public string Flavour { get; set; }
    }
}