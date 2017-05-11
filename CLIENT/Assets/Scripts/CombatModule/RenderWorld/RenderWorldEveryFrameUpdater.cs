using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface IRenderNeedUpdateEveryFrame
    {
        void Update(int delta_time);
    }

    public class RenderWorldEveryFrameUpdater : GeneralComponent<RenderWorld, int>
    {
        List<IRenderNeedUpdateEveryFrame> m_all_iupdates = new List<IRenderNeedUpdateEveryFrame>();

        public override void Destruct()
        {
            m_all_iupdates.Clear();
        }

        public void Register(IRenderNeedUpdateEveryFrame iupdate)
        {
            m_all_iupdates.Add(iupdate);
        }

        public void Unregister(IRenderNeedUpdateEveryFrame iupdate)
        {
            m_all_iupdates.Remove(iupdate);
        }

        public override void Update(int delta_time, int total_time)
        {
            for (int i = 0; i < m_all_iupdates.Count; ++i)
                m_all_iupdates[i].Update(delta_time);
        }
    }
}