using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class SyncParam
    {
        /* 
         * 同步的配置
         * 
         * frame和syncturn都是0起的
         * 发送的command中包含的syncturn表示这个命令在哪个syncturn中发生，SyncTurnDone表示哪个syncturn结束
         * 
         * 客户端：对自己的command是立即发送的，但不立即处理，等到一个syncturn结束时，再处理该syncturn的中的command
         * LogicWorld的处理顺序是先更新frame，等到一个syncturn结束时再处理command
         */

        //逻辑的更新间隔
        public const int FRAME_TIME = 33;
        //一个syncturn含几个frame
        public const int FRAME_COUNT_PER_SYNCTURN = 1;
        //同步回合的间隔
        public const int SYNCTURN_TIME = FRAME_TIME * FRAME_COUNT_PER_SYNCTURN;

        //单机多久向服务器汇报一次
        public const int SP_SYNC_INTERVAL = 10000;
        //SP_SYNC_INTERVAL转成SyncTurn
        public const int SP_SYNC_INTERVAL_TURNCOUNT = SP_SYNC_INTERVAL / SYNCTURN_TIME;

        //ZZWTODO，多玩家时，提供一个经测试的能容忍的最大延迟
        public const int MAX_LATENCY = 100;
        //多玩家不进行世界拷贝的情况下，玩家的命令延迟多久就无视
        public const int MNLP_MAX_COMMAND_DELAY_SYNCTURN = 5;
        //
        public const int COMMAND_DELAY_SYNCTURN = 1;
    }
}