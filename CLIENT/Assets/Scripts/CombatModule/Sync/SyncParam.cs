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
        public const int FRAME_TIME = 30;                                        //逻辑的更新间隔
        public const int FRAME_COUNT_PER_SYNCTURN = 1;                           //一个syncturn含几个frame
        public const int SYNCTURN_TIME = FRAME_TIME * FRAME_COUNT_PER_SYNCTURN;  //同步回合的间隔
        public const int SYNCTURN_COUNT_TO_SEND = 100;                           //单机情况每隔多少个syncturn向服务器发一个SyncTurnDone
        public const int MAX_LATENCY = 200;                                      //ZZWTODO，多玩家时，提供一个经测试的能容忍的最大延迟
        public const int MNLP_MAX_COMMAND_DELAY_SYNCTURN = 5;                    //多玩家不进行世界拷贝的情况下，玩家的命令延迟多久就无视
        public const int COMMAND_DELAY_SYNCTURN = 1;
    }
}