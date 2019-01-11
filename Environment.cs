using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;


namespace Historia
{
    public class Environment
    {
        public string Name;


        public List<string> AcceptedEnemyTypes;

        public Environment(string Name, List<string> AcceptedEnemyTypes)
        {
            this.Name = Name;
            this.AcceptedEnemyTypes = AcceptedEnemyTypes;
        }


        public Environment(bool WillGenerateItself, Random R)
        {
            SelfGenerate(R);
        }

        public Environment(string EnvToFind, Random R)
        {
            if (!FindData(EnvToFind))
            {
                SelfGenerate( R);
            }
        }

        private bool FindData(string Env)
        {
            //load all Behaviours for this AlertLevel
            string FillDictPath = "Load/Gameplay/Environments/Environments.xml";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(FillDictPath);//file path
            var obj = this;
            foreach (XmlNode Option in xmlDoc.DocumentElement.ChildNodes)
            {

                string Name = Option.Attributes["Name"].Value;
                if (Name == Env)
                {
                    List<string> OptionsEnemies = new List<string>();
                    foreach (XmlNode E in Option.ChildNodes)
                    {
                        OptionsEnemies.Add(E.InnerText);
                    }
                    this.Name = Name;
                    this.AcceptedEnemyTypes = OptionsEnemies;
                    return true;
                }

            }
            return false;
            
        }

        private void SelfGenerate(Random R)
        {
            //
            string FillDictPath = "Load/Gameplay/Environments/Environments.xml";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(FillDictPath);//file path
            var obj = this;
            List<Environment> Options = new List<Environment>();
            foreach (XmlNode Option in xmlDoc.DocumentElement.ChildNodes)
            {

                string Name = Option.Attributes["Name"].Value;
                List<string> OptionsEnemies = new List<string>();
                foreach (XmlNode E in Option.ChildNodes)
                {
                    OptionsEnemies.Add(E.InnerText);
                }

                Options.Add(new Environment(Name, OptionsEnemies));

            }
            int PickedEnvironment = R.Next(0, Options.Count);
            this.Name = Options[PickedEnvironment].Name;
            this.AcceptedEnemyTypes = Options[PickedEnvironment].AcceptedEnemyTypes;
        }
    }
}
