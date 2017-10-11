using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class BTContextKey
    {
        public static readonly int CurrentTargetID = (int)CRC.Calculate("CurrentTargetID");
    }
}