using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot
{
    [Serializable]
    public class KioskOrder
    {
        
        public string name { get; set; }
        public int coffeeType { get; set; }
        public int milkVariant { get; set; }
        public int caffeineOption { get; set; }
        public bool extraShot { get; set; }
        public int sugarCount { get; set; }
        public int sweetenerCount { get; set; }
        public int orderFlow { get; set; }
        public int source { get; set; }
        public int orderMechanism { get; set; }

        public void SetCommonOptions()
        {
            this.orderFlow = 0; //NO FACE RECOGNITION SETUP ON CORTANA SPEECH PROCESS SO ONLY NEW ORDER
            this.source = 4; //CORTANA
            this.orderMechanism = 2; //CORTANA
        }

    }

    
}