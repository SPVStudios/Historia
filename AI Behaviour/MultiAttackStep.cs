using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace Historia.AI_Behaviour
{
    /// <summary>
    /// This class mostly exists to demonstrate that PlanSteps can give special sequences of actions
    ///  more securely without risk of the sections being split up. This could be used by enraged 
    ///  enemies or something though.
    /// </summary>
    public class MultiAttackStep : AttackStep
    {
        int timesLeft;
        public MultiAttackStep(Vector2 StandLoc, Vector2 TargetLoc) : base(StandLoc, TargetLoc)
        {
            timesLeft = 3;
        }
        public MultiAttackStep(Vector2 StandLoc, Vector2 TargetLoc, int NumTimes) : base(StandLoc, TargetLoc)
        {
            if(NumTimes > 1)
            {
                timesLeft = NumTimes;
            }
            else
            {
                timesLeft = 1;
            }
        }

        public override void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget)
        {
            
            base.UsePlanStep(gameTime, Me, out Action, out NextTarget);
            
            if (timesLeft > 1)
            {
                timesLeft -= 1;
                Completed = false;
            }
        }


    }
}
