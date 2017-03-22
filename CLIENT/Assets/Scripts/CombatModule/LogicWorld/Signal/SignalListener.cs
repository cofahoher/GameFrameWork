using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISignalListener
    {
        void ReceiveSignal(ISignalGenerator generator, int signal_type, Signal signal = null);
        void OnGeneratorDestroyed(ISignalGenerator generator);
    }
}