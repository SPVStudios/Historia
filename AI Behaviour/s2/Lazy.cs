using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour.s2
{
    class Lazy:Suspicious_Behaviour
    {
        public Lazy()
        {
            behaviourName = "s2_Lazy";
        }

        public override void Scheme()
        {
            AddEmptyStep();
            
        }
    }
}
