using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour
{
    public class AttackStep : PlanStep
    {
        public bool IsHeavy;//if true, order a heavy attack (4). otherwise, use a regular attack (3)
        public Vector2 Target;
        public AttackStep(Vector2 StandingLoc, Vector2 TargetLoc):base(StandingLoc)
        {
            Target = TargetLoc;
        }

        public override void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget)
        {
            if (IsHeavy)
            {
                Action = 4;
            }
            else
            {
                Action = 3;
            }
            NextTarget = Target;
            Completed = true;
        }

        public override bool IsValid(Vector2 tilePositionBeforeStart)
        {
            return tilePositionBeforeStart == Location;//since no movement, they should be the same.
        }

        public override void End()
        {
            
        }
    }
}
