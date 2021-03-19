using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Scripting.Engine.CSharp.Tests
{

    public interface IWarrior
    {
        string Fight(string weapon);
    }

    public class Warrior : IWarrior
    {
        public string Fight(string weapon)
        {
            return "$Fighting with {weapon}";
        }
    }

}
