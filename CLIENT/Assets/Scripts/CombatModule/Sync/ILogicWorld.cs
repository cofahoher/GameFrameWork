using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ILogicWorld : IDestruct
    {
        //逻辑世界真正开始
        void OnStart();
        //更新，返回游戏是否结束（true结束）
        bool OnUpdate(int delta_ms);
        //游戏是否结束
        bool IsGameOver();
        //处理各玩家的操作命令
        void HandleCommand(Command command);
        //世界拷贝
        void CopyFrom(ILogicWorld parallel_world);
        //FRAME
        int GetCurrentFrame();
        //CRC
        uint GetCRC();
    }

    public class DummyLogicWorld : ILogicWorld
    {
        int m_current_frame = 0;
        public void Destruct()
        {
        }
        public void OnStart()
        {
        }
        public bool OnUpdate(int delta_ms)
        {
            ++m_current_frame;
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
        public int GetCurrentFrame()
        {
            return m_current_frame;
        }
        public uint GetCRC()
        {
            return 0;
        }
    }
}