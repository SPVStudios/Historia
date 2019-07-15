using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Historia.AI_Behaviour.s1
{
   public  class TurnOnSpot:Unsettled_Behaviour
    {
        public override void Scheme()
        {
            int newFacing = map.D.Next(0, 3);
            AddPlanStep(new FacingStep(finalStep.Location, newFacing));
            AddSearchStep();
        }
    }
}
