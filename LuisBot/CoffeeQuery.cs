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

    //    [Prompt("How Hot did you want it? Say Warm, Hot or Extra Hot.")]
     //   [Optional]
      //  public string HeatLevel { get; set; }

        [Prompt("How strong did you want it? Say Double Shot, Regular, or Decaf.")]
        [Optional]
        public string CoffeeStrength { get; set; }

        [Prompt("What Milk did you want? Say either Full Cream , Skim, Almond or Soy.")]
        [Optional]
        public string MilkType { get; set; }

        // [Prompt("Did you want Sugar or Artificial Sweetner? Say Sugar or Artificial Sweetner or None")]
        // [Optional]
        // public string Sugar { get; set; }

        [Prompt("Did you want any Additional Flavouring? Say, Hazelnut, Caramel or None.")]
        [Optional]
        public string Flavour { get; set; }

        [Prompt("How many Sugars should i put? Say None if you did not want any.")]
        [Optional]
        public string SpoonsOfSugar { get; set; }

     
    }
}