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
    public abstract class PlanStep
    {
        /// <summary>
        /// The Location the enemy will be in when this step is completed.
        /// MUST be correct to correctly inform AI_Behaviour, regardless of whether anything actualy moves.
        /// 
        /// Stored other Booleans like Targets should be stored on a case-by-case basis.
        /// 
        /// </summary>
        public Vector2 Location;//
       
            public PlanStep(Vector2 Loc)
        {
            Location = Loc;
        }

        public bool Completed
        {
            get;protected set;
        }
        /// <summary>
        /// Call this as the enemy that wishes to use it. the PlanStep will call the correct action to start.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="Me"></param>
        /// <param name="Action"></param>
        /// <param name="NextTarget"></param>
       
        public abstract void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget);

        public abstract bool IsValid(Vector2 tilePositionBeforeStart);

        /// <summary>
        /// complete any actions necessary before deletion, as this action is marked complete by Enemy, just before being discarded.
        /// </summary>
        public abstract void End();

        /// Actions 
    }
}
