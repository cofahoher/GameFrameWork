using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ILogicNeedUpdateEveryFrame
    {
        void Update(FixPoint delta_time);
        //void SetUpdateID(int update_id);
        //void GetUpdateID(int update_id);
    }

    public class LogicWorldEveryFrameUpdater : GeneralComponent<LogicWorld, FixPoint>
    {
        List<ILogicNeedUpdateEveryFrame> m_all_iupdates = new List<ILogicNeedUpdateEveryFrame>();

        public override void Destruct()
        {
            m_all_iupdates.Clear();
        }

        public void Register(ILogicNeedUpdateEveryFrame iupdate)
        {
            m_all_iupdates.Add(iupdate);
        }

        public void Unregister(ILogicNeedUpdateEveryFrame iupdate)
        {
            m_all_iupdates.Remove(iupdate);
        }

        public override void Update(FixPoint delta_time, FixPoint total_time)
        {
            for (int i = 0; i < m_all_iupdates.Count; ++i)
                m_all_iupdates[i].Update(delta_time);
        }
    }
}