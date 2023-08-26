using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedsunClient.GameSystem
{
    public class GameObjectManager
    {
        public static GameObjectManager Instance = new GameObjectManager();

        private Dictionary<int, PlayerObject> mPlayerList = new Dictionary<int, PlayerObject>();
        private Dictionary<int, MonsterObject> mMonsterList = new Dictionary<int, MonsterObject>();



        public GameObjectManager()
        {
        }


        public PlayerObject? GetPlayer(int seq) => mPlayerList.ContainsKey(seq) ? mPlayerList[seq] : null;
        public MonsterObject? GetMonseter(int seq) => mMonsterList.ContainsKey(seq) ? mMonsterList[seq] : null;


        public bool AddPlayer(MUserInfo info)
        {
            if (mPlayerList.ContainsKey(info.Seq))
                return false;   // 이미 존재

            PlayerObject addData = new PlayerObject();
            if (!addData.SetData(info))
                return false;   // 잘못된 데이터였음

            mPlayerList.Add(info.Seq, addData);
            return true;
        }
        

        public void Reset()
        {
            mPlayerList.Clear();
            mMonsterList.Clear();
        }


        public bool AddMonster(MMonsterInfo info)
        {
            if (mMonsterList.ContainsKey(info.Seq))
                return false;   // 이미 존재

            MonsterObject addData = new MonsterObject();
            if (!addData.SetData(info))
                return false;

            mMonsterList.Add(info.Seq, addData);
            return true;
        }

        public void RemovePlayer(int seq)
        {
            mPlayerList.Remove(seq);
        }

        public void RemoveMonster(int seq)
        {
            mMonsterList.Remove(seq);
        }

        public void Update(float elapsedSec)
        {
            if (elapsedSec <= 0.0f)
                return;

            foreach(var obj in mPlayerList)
            {
                obj.Value?.Update(elapsedSec);
            }

            foreach(var obj in mMonsterList)
            {
                obj.Value?.Update(elapsedSec);
            }
        }






    }
}
