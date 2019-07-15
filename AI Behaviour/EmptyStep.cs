using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace Historia.AI_Behaviour
{
    public class EmptyStep : PlanStep
    {
        public EmptyStep(Vector2 CurrentTilePos):base(CurrentTilePos)
        {
            
        }

        public override void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget)
        {
            Action = 0;
            NextTarget = Me.TilePosition;
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
