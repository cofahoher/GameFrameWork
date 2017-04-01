using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class TextBuffer : IRecyclable, IDestruct
    {
        #region Create/Recycle
        public static TextBuffer Create()
        {
            return ResuableObjectPool<IRecyclable>.Instance.Create<TextBuffer>();
        }

        public static void Recycle(TextBuffer instance)
        {
            ResuableObjectPool<IRecyclable>.Instance.Recycle(instance);
        }
        #endregion

        string m_buffer;
        int m_index = -1;

        public void Construct(string buffer)
        {
            m_buffer = buffer;
            m_index = 0;
        }

        public void Destruct()
        {
        }

        public void Reset()
        {
            m_buffer = null;
            m_index = -1;
        }

        public int CurrentIndex
        {
            get { return m_index; }
        }

        public bool Eof()
        {
            return m_index >= m_buffer.Length || m_buffer[m_index] == 0;
        }

        public char Char() 
	    { 
		    if (Eof())
			    return '\0';
            return m_buffer[m_index]; 
	    }

        public char NextChar()
        {
            ++m_index;
		    if (Eof())
                return '\0';
            return m_buffer[m_index];
	    }

        public string SubString(int start_index, int length)
        {
            return m_buffer.Substring(start_index, length);
        }
    }
}