using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour
{
    public class FacingStep : PlanStep
    {
        public Vector2 Target;
        public int Facing
        {
            get; private set;
        }

        public FacingStep(Vector2 PersonLoc,Vector2 Target) : base(PersonLoc)
        {
            
        }

        public FacingStep(Vector2 PersonLoc, int Facing) : base(PersonLoc)
        {
            Target = PersonLoc + Being.GetDirection(Facing);

        }

        public override void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget)
        {
            NextTarget = Target;
            Action = 0;
            Completed = true;
        }

        public override bool IsValid(Vector2 tilePositionBeforeStart)
        {
            return tilePositionBeforeStart == Location;
        }

        public override void End()
        {
            //no need
        }

        //From Being.GetFacing:

        //protected static int GetFacing(Vector2 Direction)//reverses the process that gives the Bing's Direction value to fin facing based on a vector direction.
        //{
        //    if (Direction == new Vector2(0, 1))
        //    {
        //        return 0;
        //    }
        //    if (Direction == new Vector2(-1, 0))
        //    {
        //        return 1;
        //    }
        //    if (Direction == new Vector2(1, 0))
        //    {
        //        return 2;
        //    }
        //    if (Direction == new Vector2(0, -1))
        //    {
        //        return 3;
        //    }
        //    else
        //    {
        //        throw new Exception("given invalid Direction - must only have a value for X OR Y");
        //    }
        //}
    }
}
