using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.Xml;

namespace Historia.AI_Behaviour.s2
{
    public class Search : Suspicious_Behaviour
    {
        public Search()
        {
            behaviourName = "s2_Search";
        }

        public override void Scheme()
        {
            Rectangle NextSearch = SuggestThreatLocation();

            PlanWalkToLocation(NextSearch);
            AddSearchStep();
        }
    }
}
