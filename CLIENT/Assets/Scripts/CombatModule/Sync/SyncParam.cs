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
         * FRAME_TIME：是逻辑的更新间隔
         * FRAME_COUNT_PER_SYNCTURN：一个syncturn含几个frame
         * SYNCTURN_TIME：是同步回合的间隔
         * 
         * 客户端：对自己的command是立即发送的，但不立即处理，等到一个syncturn结束时，再处理该syncturn的中的command
         * 单机客户端：不频繁的发送SyncTurnDone，每累计SYNCTURN_COUNT_TO_SEND发送一次
         * LogicWorld的处理顺序是先更新frame，等到一个syncturn结束时再处理command
         */
        public const int FRAME_TIME = 30;
        public const int FRAME_COUNT_PER_SYNCTURN = 1;
        public const int SYNCTURN_TIME = FRAME_TIME * FRAME_COUNT_PER_SYNCTURN;
        public const int SYNCTURN_COUNT_TO_SEND = 30;
        public const int COMMAND_DELAY_SYNCTURN = 1;
        public const int MAX_LATENCY = 1000;
    }
}