using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoFrostTemplate.Code.Utils.Objects
{
    public class LevelObject : GameObject
    {
        public int levelStateID = -1;

        protected virtual byte[] GetStateBytes => new byte[0];

        public virtual void OnTrigger(bool silent = false) { }
        public virtual void OnUnTrigger(bool silent = false) { }

        public virtual void ReadLevelState(string levelID) { }
        public void WriteLevelState(string levelID)
        {
            byte[] b = GetStateBytes;
            if (GetStateBytes.Length > 0)
            {
                if (scene.levelStates.ContainsKey((levelID, levelStateID))) scene.levelStates[(levelID, levelStateID)] = b;
                else scene.levelStates.Add((levelID, levelStateID), b);
            }
        }
    }
}
