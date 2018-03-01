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
        public string coffeeOwner { get; set; }

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

        public bool optionsSet { get; set; }

        public override string ToString()
        {
            coffeeOrder = "";
            if (this.SpoonsOfSugar.ToUpper().Contains("ZERO") || this.SpoonsOfSugar.ToUpper().Contains("NO") || this.Sugar.ToUpper().Contains("NO") || this.Sugar.ToUpper().Contains("NONE"))
            {
                coffeeOrder = $"{this.coffeeOwner} requested for a {this.Size} sized, {this.HeatLevel} {this.CoffeeStrength} strength {this.CoffeeType} with {this.MilkType} milk, along with {this.Flavour} flavour and no sugar";
            }
            else
            {
                coffeeOrder = $"{this.coffeeOwner} requested for a {this.Size} sized, {this.HeatLevel} {this.CoffeeStrength} strength {this.CoffeeType} with {this.MilkType} milk, along with {this.Flavour} flavour and {this.SpoonsOfSugar} {this.Sugar}";
            }
            return coffeeOrder;
        }
        public void SetCommonOptions()
        {

            this.Size = "Regular";
            //this.CoffeeStrength = "Regular";
            //this.Flavour = "no flavouring";
            //this.MilkType = "Full Cream";
            //this.SpoonsOfSugar = "No";
            //this.Sugar = "Sugar";
            this.HeatLevel = "Hot";
        }
        
    }  

}