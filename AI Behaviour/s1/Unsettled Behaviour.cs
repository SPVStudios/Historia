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


namespace Historia.AI_Behaviour.s1
{
    public abstract class Unsettled_Behaviour:AIBehaviour//Behaviour for detection Level 1, with a Room as the vague suspicion of location.
    {
        public Unsettled_Behaviour()
        {
            MaxConfirmDistance = 4;
        }

        protected virtual Rectangle ThreatLocation()
        {
            int BiggestValue = 0;
            Vector2 BiggestKey = Vector2.Zero;
           
            foreach(KeyValuePair<Vector2,int> I in Me.CurrentSuspicions)
            {
                if(I.Value > BiggestValue)
                {
                    BiggestValue = I.Value;
                    BiggestKey = I.Key;
                }
            }
            int RoomIndex = RectMethod.FindWhatRoomLocationIsIn(BiggestKey, map, out bool IsInPassage);
            if(RoomIndex == Me.CurrentRoomIn)
            {
                return RectMethod.AddABorder(Me.ImmediatelyAhead,1);
            }
            else
            {
                return CalculateCurrentRoomIn(BiggestKey);
            }

            
           
        }
        
        

        
    }
}
