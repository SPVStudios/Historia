using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour.s0
{
    class Lazy:Oblivious_Behaviour
    {
        public Lazy()
        {
            behaviourName = "s0_Lazy";
        }
        public override void Scheme()
        {
            AddEmptyStep();
            for(int I = 0; I < 15; I++)
            {
                AddWaitStep(new WaitStep(finalStep.Location, 250));
            }
            //there are a lot here to signal to others the posibility of a collision
        }

        
    }
}
