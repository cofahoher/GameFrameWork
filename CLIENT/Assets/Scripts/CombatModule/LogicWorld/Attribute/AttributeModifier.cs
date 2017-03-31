using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class AttributeModifier : IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static AttributeModifier Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<AttributeModifier>();
        }

        public static void Recycle(AttributeModifier instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

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