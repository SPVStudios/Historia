using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour.s3
{
    class Pace:Alerted_Behaviour
    {
        public Pace()
        {
            behaviourName = "s3_Pace";
        }

        public override void Scheme()
        {
            if (CollisionCheck.Instance.CheckTargetBool(map.Collision.CMap, map.BC.BC, Me.TilePosition+Me.Direction))
            {
                
                AddPlanStep(new MoveStep(Me.TilePosition+Me.Direction));
                return;
            }
            else if (CollisionCheck.Instance.CheckTargetBool(map.Collision.CMap, map.BC.BC, Me.TilePosition - Me.Direction))//behind
            {
                AddPlanStep(new MoveStep(Me.TilePosition - Me.Direction));
                
                return;
            }
            else
            {

                AddEmptyStep();
            }
        }
    }
}
