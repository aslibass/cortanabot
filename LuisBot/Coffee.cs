using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot
{
    using System;

    [Serializable]
    public class Coffee
    {
        public string CoffeeType { get; set; }

        public string Size { get; set; }

        public string Flavour { get; set; }

        public string MilkType { get; set; }

        public string Image { get; set; }

        public string Sugar { get; set; }

        public string SpoonsOfSugar { get; set; }

        public string CoffeeStrength { get; set; }

        public string HeatLevel { get; set; }

        public string coffeeOrder = "";

        public override string ToString()
        {
            coffeeOrder = "";
            if (this.Sugar=="No Sugar")
            {
                coffeeOrder = $"Request for a {this.Size} sized, {this.CoffeeStrength} {this.CoffeeType} with {this.MilkType} milk and no sugar]";
            }
            else
            {
                coffeeOrder = $"Request for a {this.Size} sized, {this.CoffeeStrength} {this.CoffeeType} with {this.MilkType} milk and {this.SpoonsOfSugar} sugar(s)]";
            }
            return coffeeOrder;
        }
        public void SetCommonOptions()
        {

            this.Size = "Regular";
            this.CoffeeStrength = "Single Shot";
            this.Flavour = "no flavouring";
            this.MilkType = "Full Cream";
            this.SpoonsOfSugar = "none";
            this.Sugar = "No Sugar";
        }
    }  

}