using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour.s3
{
    class Lazy:Alerted_Behaviour
    {
        public Lazy()
        {
            behaviourName = "s3_Lazy";
        }

        public override void Scheme()
        {
            AddEmptyStep();
        }
    }
}
