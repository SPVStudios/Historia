using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour.s1
{
    public class LookAround : Unsettled_Behaviour
    {

        public LookAround()
        {
            behaviourName = "s1_LookAround";
        }

        public override void Scheme()
        {
            //bool WillWalkATile;
            if (map.D.Next(0, 100) > 50)
            {
                // WillWalkATile = true;
                
                //Pick a nearby tile 
                if (PlanWalkToLocation(ThreatLocation()))
                {//on end of wlak plan:
                    AddSearchStep();
                }
                else
                {//with no walk plan, just return:
                    AddEmptyStep();
                }

            }
            else
            {
                //WillWalkATile = false;
                
                AddPlanStep(new FacingStep(Me.TilePosition, map.D.Next(0, 3)));
                AddSearchStep();

            }
        }
    }
}
