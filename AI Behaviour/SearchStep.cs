using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace Historia.AI_Behaviour
{
    /// <summary>
    /// SearchStep is simply a label of WaitStep that invokes the AIBehaviour AttemptPlayerConfirmation() as well. 
    /// </summary>
    public class SearchStep:WaitStep
    {
        Enemy Me;

        public const int defaultTime = 400;
        AIBehaviour signalreturn;
        Map mapRef;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Loc"></param>
        /// <param name="signalreturn">the AIBehaviour that created this, so the step feeds back on End with a possible call to ConfirmLocation</param>
        public  SearchStep(Vector2 Loc, AIBehaviour signalreturn, Map mapRef) : base(Loc, defaultTime)//all searches w
        {
            this.signalreturn = signalreturn;
            this.mapRef = mapRef;
        }

        public SearchStep(Vector2 Loc, int WaitTime, AIBehaviour signalreturn, Map mapRef) : base(Loc, WaitTime)
        {
            this.signalreturn = signalreturn;
            this.mapRef = mapRef;
        }

        public override void UsePlanStep(GameTime gameTime, Enemy Me, out int Action, out Vector2 NextTarget)
        {
            this.Me = Me;//for use in End(). A little strange to obtain this like this rather than a separate call or override, but it complicates the external system.
            base.UsePlanStep(gameTime, Me, out Action, out NextTarget);
        }

        public override void End()
        {
            // run the ConfirmationCheck now!
            if (signalreturn.AttemptPlayerConfirmation(out Vector2 playerLoc))
            {
                signalreturn.ConfirmEnemyLocation(playerLoc);
            }
            else
            {
                //disperse every suspicious tile you can currently see.
                Dictionary<Vector2, int> CandidatesToDisperse = Me.CurrentSuspicions;
                CandidatesToDisperse = ListMethod.FilterFarAway(CandidatesToDisperse, signalreturn.MaxConfirmDistance,Me.TilePosition);
                CandidatesToDisperse = StealthMethod.MassCheckLineOfSight(mapRef, Me.TilePosition, Me.Direction, CandidatesToDisperse);
                ///for those tiles in range, dissipate suspicion
                foreach(Vector2 LocToDisperse in CandidatesToDisperse.Keys)
                {
                    signalreturn.DissipateSuspicion(LocToDisperse);
                }
                
            }
        }


    }
}
