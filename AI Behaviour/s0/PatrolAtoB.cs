using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Historia.AI_Behaviour.s0
{
    public class PatrolAtoB:Oblivious_Behaviour
    {
        private Vector2 LastLoc;
        public int Chances_Left;//if the patrolling enemy has been stuck stood somewhere for a while,
        //then one final chance is given tho the Scheme() method to find a route. If on the next call to Scheme you haven't moved, recalculate waypoints.

        const int MaxChances = 100;
        bool FinalChance;

        public PatrolAtoB()
        {
            behaviourName = "s0_Patrol: A to B";
            FinalChance = false;
            Chances_Left = MaxChances;
            
        }

        public override void LoadContent(ref Enemy Me, Map map)
        {
            base.LoadContent(ref Me, map);
            Create_Standard_Patrol(true);
            LastLoc = Me.TilePosition;
        }

        public override void Scheme()
        {
            if(LastLoc == Me.TilePosition)
             {
                Chances_Left--;
                if(Chances_Left <= 0)
                {
                    Create_Standard_Patrol(false);
                    Chances_Left = MaxChances;
                }
            }
            else
            {
                LastLoc = Me.TilePosition;
                Chances_Left++;
            }
            Plan_Journey_ToNextWaypoint(); 
        }
    }
}
