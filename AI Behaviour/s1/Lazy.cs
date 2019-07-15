using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour.s1
{
    class Lazy:Unsettled_Behaviour
    {
        public Lazy()
        {
            behaviourName = "s1_Lazy";
        }

        public override void Scheme()
        {
            AddEmptyStep();
            
        }
    }
}
