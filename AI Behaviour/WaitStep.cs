using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour
{
    public class WaitStep : PlanStep
    {
        int WaitTime;

        public WaitStep(Vector2 Loc, int WaitTime) : base(Loc)
        {
            this.WaitTime = WaitTime;
        }

        public override void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget)
        {
            Me.MakeWait(WaitTime);
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
