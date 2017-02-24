using System.Collections;
namespace Combat
{
    public class FactionComponent : PlayerComponent
    {
        int m_faction_id = 0;

        #region GETTER
        public int FactionID
        {
            get { return m_faction_id; }
        }
        #endregion
    }
}