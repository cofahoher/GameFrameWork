using System.Collections;
namespace Combat
{
    //每个object的生命周期都应该精确知道，Destruct()就相当于主动析构时的析构函数
    public interface ILogicOwnerInfo
    {
        LogicWorld GetLogicWorld();
        int GetCurrentTime();
        int GetOwnerObjectID();
        Object GetOwnerObject();
        int GetOwnerPlayerID();
        Player GetOwnerPlayer();
        int GetOwnerEntityID();
        Entity GetOwnerEntity();
    }
}