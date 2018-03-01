namespace LuisBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class CoffeeQuery
    {
        [Prompt("Under what name shall i put this order through?")]
        public string CoffeeOwner { get; set; }

        [Prompt("What Type of Coffee Would you like?")]
        public string CoffeeType { get; set; }

        [Prompt("How strong did you want it? Say Extra Shot, Regular, or Decaf.")]
        public string CoffeeStrength { get; set; }

        [Prompt("What Milk did you want? Say either Full Cream , Skim, Almond or Soy.")]
        public string MilkType { get; set; }

       /* public enum SugarType
        {
            Sugar,
            ArtificialSweetner,
            None
        }
        */
        [Prompt("Did you want Sugar or Artificial Sweetner? Say Sugar or Artificial Sweetner or None")]
        public string SugarType { get; set; }

        [Prompt("How many Spoons of Sugar/Sweetner should i put? Say None if you did not want any.")]
        public string SpoonsOfSugar { get; set; }

        [Prompt("Did you want any Additional Flavouring? Say, Hazelnut, Caramel or None.")]
        public string Flavour { get; set; }

        public override string ToString()
        {
            String coffeeOrder = "";
            if (this.SpoonsOfSugar.ToUpper().Contains("ZERO") || this.SpoonsOfSugar.ToUpper().Contains("NO") )
            {
                coffeeOrder = $"{this.CoffeeOwner} requested for a Regular sized, hot, {this.CoffeeStrength} strength {this.CoffeeType} with {this.MilkType} milk, along with {this.Flavour} flavour and no sugar";
            }
            else
            {
                coffeeOrder = $"{this.CoffeeOwner} requested for a Regular sized, hot {this.CoffeeStrength} strength {this.CoffeeType} with {this.MilkType} milk, along with {this.Flavour} flavour and {this.SpoonsOfSugar} of {this.SugarType.ToString()}";
            }
            return coffeeOrder;
        }

    }
}