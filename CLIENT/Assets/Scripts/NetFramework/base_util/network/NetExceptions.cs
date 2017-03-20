using System;


namespace BaseUtil
{
    [Serializable]
    class NetRuntimeException : SystemException
    {
        public NetRuntimeException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    class NetStreamException : NetRuntimeException
    {
        public NetStreamException(string message, object witch)
            : base(message)
        {
            this.witch = witch;
        }

        public object witch { get; private set; }
    }

    //[Serializable]
    //class NetStreamException : NetRuntimeException
    //{
    //    public NetStreamException(string message, object witch)
    //        : base(message)
    //    {
    //        this.witch = witch;
    //    }

    //    public object witch { get; private set; }
    //}
}
