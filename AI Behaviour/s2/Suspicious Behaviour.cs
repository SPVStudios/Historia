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

namespace Historia.AI_Behaviour.s2
{
    public abstract class Suspicious_Behaviour:AIBehaviour//Detection Level 2 - is looking for you, with a rectangular area of reasonable confidence in your location (could be wrong though - think distractions)
    {
        public Suspicious_Behaviour()
        {
            MaxConfirmDistance = 6;
        }
        
        protected virtual Rectangle SuggestThreatLocation()
        {
            Vector2 BiggestKey = Vector2.Zero;
            int BiggestValue = 0;
            //step 1: Create 3x3 clusters using the values for all tiles in that cluster added together
            foreach (KeyValuePair<Vector2, int> I in Me.CurrentSuspicions)
            {

                int ClusterSuspicion = 0;
                for(int X = -1; X <= 1; X++)
                {
                    for(int Y = -1; Y <= 1; Y++)
                    {
                        if(Me.CurrentSuspicions.ContainsKey(new Vector2(I.Key.X + X, I.Key.Y + Y)))
                        ClusterSuspicion += Me.CurrentSuspicions[new Vector2(I.Key.X + X, I.Key.Y + Y)];
                    }
                }
                //step 2: apply a +-100 randomiser to every cluster
                ClusterSuspicion += map.D.Next(-100, 100);
                //step 3a: keep track of the largest cluster
                if (ClusterSuspicion > BiggestValue)
                {
                    BiggestValue = ClusterSuspicion;
                    BiggestKey = I.Key;
                }
               
            }
            return new Rectangle((int)BiggestKey.X - 1, (int)BiggestKey.Y - 1, 3, 3);
        }
        
        //very similar to the above, since it's the same criteria to attempt a confirmation.
        
        

    }
}
