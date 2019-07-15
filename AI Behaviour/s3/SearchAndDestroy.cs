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

namespace Historia.AI_Behaviour.s3
{
    public class SearchAndDestroy:Alerted_Behaviour
    {
        public SearchAndDestroy()
        {
            behaviourName = "s3_SearchAndDestroy";
        }


        public override void Scheme()
         {
            if (HaveConfirmedTile)
            {
                if (PlanWalkToLocation(ConfirmedTile,false))
                {
                    plannedActions.RemoveAt(plannedActions.Count - 1);//remove the last tile as we don't actually want to walk into that tile.
                    AddPlanStep(new AttackStep(finalStep.Location, ConfirmedTile));
                }
                else
                {

                }
               
            }
            else
            {
                Rectangle NextSearch = SuggestThreatLocation();
                PlanWalkToLocation(NextSearch);
                AddSearchStep();
            }
            
        }
    }
}
