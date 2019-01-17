using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
namespace Historia
{
    public class Dungeon
    {




        public Vector2 Location;

        public string Name;

        public Environment Env;

        public bool UsedATMForQuest;

        public Dungeon(Vector2 Location)
        {
            this.Location = Location;
        }

        public void Populate(string Name, string EnvName, Random R)
        {
            this.Name = Name;
            this.Env = new Environment(EnvName, R);
        }

    }
}
