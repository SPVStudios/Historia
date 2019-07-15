using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace Historia.AI_Behaviour.s0
{
    class Mill_Around : Oblivious_Behaviour
    {

        public override void Scheme()
        {
            int newFacing = map.D.Next(0, 3);
            AddPlanStep(new FacingStep(finalStep.Location, newFacing));
            AddWaitStep(new WaitStep(finalStep.Location, 800));
        }
    }
}
