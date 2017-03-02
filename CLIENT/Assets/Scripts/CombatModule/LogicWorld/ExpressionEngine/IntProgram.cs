using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class IntProgram : Program, IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static IntProgram Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<IntProgram>();
        }

        public static void Recycle(IntProgram instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        public IntProgram()
        {
        }

        public void Reset()
        {
        }

        public void Destruct()
        {
        }
    }
}