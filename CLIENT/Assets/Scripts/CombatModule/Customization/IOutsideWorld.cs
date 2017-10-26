using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IOutsideWorld : IDestruct
    {
        //配置
        IConfigProvider GetConfigProvider();
        //关卡配置
        LevelData GetLevelData();
        //返回当前时间（单位毫秒）
        int GetCurrentTime();
        //游戏开始，不用驱动逻辑世界，让渲染世界开始就好了（对服务器，这就无视了）
        void OnGameStart();
        //这个其实是逻辑世界往外通知的，不同的游戏可能会不一样吧
        void OnGameOver(GameResult game_result);
        // 获取本地玩家
        long GetLocalPlayerPstid();
        //暂停
        void Suspend(FixPoint suspending_time);
        //恢复
        void Resume();
    }
}