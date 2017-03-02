using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISignalListener
    {
        int GetListenerID();
        void ReceiveSignal(ISignalGenerator generator, SignalType signal_type, Signal signal);
        void OnGeneratorDestroyed(ISignalGenerator generator);
    }
}