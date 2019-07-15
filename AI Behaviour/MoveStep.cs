using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour
{
    public class MoveStep : PlanStep
    {
        public MoveStep(Vector2 TargetLocation):base(TargetLocation)
        {
            
        }


        public override void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget)
        {
            Action = 1;
            NextTarget = Location;
            Completed = true;
        }
        public override bool IsValid(Vector2 tilePositionBeforeStart)
        {
            return RectMethod.TwoTilesAreAdjacent(Location, tilePositionBeforeStart);
        }

        public override void End()
        {
            //no action needed
        }
    }
}
