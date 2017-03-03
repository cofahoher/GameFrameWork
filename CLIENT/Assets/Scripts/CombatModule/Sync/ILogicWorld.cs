using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ILogicWorld : IDestruct
    {
        /*
         * 逻辑世界实现这个接口
         */
        //逻辑世界真正开始
        void OnStart();
        //更新，返回游戏是否结束（true结束）
        bool OnUpdate(int delta_ms);
        //游戏是否结束
        bool IsGameOver();
        //处理各玩家的操作命令
        void HandleCommand(Command command);
        //世界拷贝，不用实现
        void CopyFrom(ILogicWorld parallel_world);
    }

    public class DummyLogicWorld : ILogicWorld
    {
        public void Destruct()
        {
        }
        public void OnStart()
        {
        }
        public bool OnUpdate(int delta_ms)
        {
            return false;
        }
        public bool IsGameOver()
        {
            return false;
        }
        public void HandleCommand(Command command)
        {
        }
        public void CopyFrom(ILogicWorld parallel_world)
        {
        }
    }
}