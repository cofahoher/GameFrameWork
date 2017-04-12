using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeModifier : IRecyclable, IDestruct
    {
        public AttributeModifier()
        {
        }

        public void Destruct()
        {
        }

        public void Reset()
        {
        }

        #region GETTER
        public int ID
        {
            get { return 0; }
        }
        #endregion
    }
}