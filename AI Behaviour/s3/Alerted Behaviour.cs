using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.Xml;

namespace Historia.AI_Behaviour.s3
{
    public abstract class Alerted_Behaviour:AIBehaviour//Detection Level 3+ - Has quite concrete belief in a particular Co-ordinate.
        //would normally pathfind to your location and attack you.

    //Alerted_Behaviours consist of a response behaviour to the threat level, usually just trying to get to you to attack you - 
    // and also a specific attack strategy behaviour be mentioned if it overrides the combat system specified by basic AI, be it zealous and use-up-stamina-quick, or sneak-and-dodge-y, or Parry intensive etc.
    {
        public Alerted_Behaviour()
        {
            MaxConfirmDistance = 10;
        }


        protected virtual Rectangle SuggestThreatLocation()
        {
            Vector2 BiggestKey = Vector2.Zero;
            int BiggestValue = 0;
            //step 1: Create 3x3 clusters using the values for all tiles in that cluster added together
            foreach (KeyValuePair<Vector2, int> I in Me.CurrentSuspicions)
            {
                if(I.Value > BiggestValue)
                {
                    BiggestValue = I.Value;
                    BiggestKey = I.Key;
                }
            }
            return new Rectangle((int)BiggestKey.X, (int)BiggestKey.Y,1,1);//the most suspicious tile.
        }

        protected PlanStep AttackAdjacentEnemy()
        {
            AttackStep aStep = new AttackStep(Me.TilePosition, StealthManager.Instance.PlayerLoc);
            /// as based on previous call of Attack:
            ///if(map.D.Next(0,100) > 65)//35% chance of them using a heavy attack
            ///{
            ///    Action = 4;
            ///}
            ///else
            ///{ //65% chance of a regular attack
            ///    Action = 3;
            ///}
            aStep.IsHeavy = map.D.Next(0, 100) > 65;
            return aStep;
        }

        public override PlanStep DecideWhatToDo()
        {
            //Vector2 PlayerLoc = StealthManager.Instance.PlayerLoc;
            if (HaveConfirmedTile && !WaitingforGoAhead && !HasAPlan)
            {//plan could make enemy face different direction in navigation to player, so shouldn't unconfirm the tile
                if (StealthMethod.CheckLOS(map, Me.TilePosition, StealthManager.Instance.PlayerLoc)
                    && StealthMethod.CheckFOV(Me.TilePosition, StealthManager.Instance.PlayerLoc, Me.Direction))
                {
                    //player in sight, so move confirmed location to him
                    MoveConfirmedPosition(StealthManager.Instance.PlayerLoc);

                    //All behaviour for when seeing the enemy

                    if (RectMethod.DistanceBetweenLocations
                        (Me.TilePosition, StealthManager.Instance.PlayerLoc) == 1)
                    {
                        return AttackAdjacentEnemy();
                        
                    }
                    else
                    {
                        return base.DecideWhatToDo();//act using the usual protocol.
                        
                    }
                }
                //if not in sight, then UNCONFIRM
                else
                {
                    CancelConfirmedPosition();

                    // behaviour for not 
                }
            }
            // no confirmed tile, but on alert
            return base.DecideWhatToDo();
        }

        

       
        /// <summary>
        /// Overrides the normal searching waits to take half as long as normal. useful through dynamic polymorphism
        /// </summary>
        protected override void AddSearchStep()
        {
            SearchStep searchStep = new SearchStep(finalStep.Location,SearchStep.defaultTime/2, this,map);
            plannedActions.Add(searchStep);
        }
    }
}
