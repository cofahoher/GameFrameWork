using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class Tokenizer : IRecyclable, IDestruct
    {
        public const char Eof        = (char)0;
        public const char WhiteSpace = (char)1;
        public const char Error      = (char)2;
        public const char Digit      = (char)3;
        public const char Symbol     = (char)4;
        public const char Letter     = (char)5;
        public const char Quote      = (char)6;

        #region CodeMap
        public const char Error_____ = Error;
        public const char Digit_____ = Digit;
        public const char Symbol____ = Symbol;
        public const char Letter____ = Letter;
        public const char Quote_____ = Quote;

        public static readonly char[] CodeMap = new[]{
            /*     0          1          2          3          4          5          6          7          8          9               */
            /*  0*/WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,WhiteSpace,/*  0*/
            /* 10*/WhiteSpace,Error_____,Error_____,WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 10*/
            /* 20*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 20*/
            /* 30*/Error_____,Error_____,WhiteSpace,Error_____,Quote_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 30*/
            /* 40*/Symbol____,Symbol____,Symbol____,Symbol____,Symbol____,Symbol____,Symbol____,Symbol____,Digit_____,Digit_____,/* 40*/
            /* 50*/Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Error_____,Error_____,/* 50*/
            /* 60*/Error_____,Error_____,Error_____,Error_____,Error_____,Letter____,Letter____,Letter____,Letter____,Letter____,/* 60*/
            /* 70*/Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,/* 70*/
            /* 80*/Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,/* 80*/
            /* 90*/Letter____,Symbol____,Error_____,Symbol____,Error_____,Letter____,Error_____,Letter____,Letter____,Letter____,/* 90*/
            /*100*/Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,/*100*/
            /*110*/Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,Letter____,/*110*/
            /*120*/Letter____,Letter____,Letter____,Error_____,Error_____,Error_____,Error_____,Error_____/*127*/
        };

        static public char GetCode(char ch)
        {
            if (ch > 127)
                return Letter;
            else
                return CodeMap[ch];
        }
        #endregion
        
        TextBuffer m_text_buffer;
        bool m_error = false;
        Token m_current_token;
        ErrorToken m_error_token = new ErrorToken();
        EofToken m_eof_token = new EofToken();
        NumberToken m_number_token = new NumberToken();
        SymbolToken m_special_token = new SymbolToken();
        WordToken m_word_token = new WordToken();
        StringToken m_string_token = new StringToken();

        public void Construct(string expression)
        {
            m_text_buffer = RecyclableObject.Create<TextBuffer>();
            m_text_buffer.Construct(expression);
        }

        public void Destruct()
        {
            if (m_text_buffer != null)
                RecyclableObject.Recycle(m_text_buffer);
            m_text_buffer = null;
        }

        public void Reset()
        {
            if (m_text_buffer != null)
                RecyclableObject.Recycle(m_text_buffer);
            m_text_buffer = null;
        }

        public Token GetCurrentToken()
        {
            return m_current_token;
        }

        public Token GetNextToken()
        {
            if (m_error)
            {
                m_current_token = m_error_token;
            }
            else if (m_text_buffer == null)
            {
                m_current_token = m_eof_token;
            }
            else
            {
                char code = SkipWhiteSpace();
                if (m_error)
                {
                    m_current_token = m_error_token;
                }
                else if (m_text_buffer == null)
                {
                    m_current_token = m_eof_token;
                }
                else
                {
                    switch (code)
                    {
                    case Digit:
                        m_current_token = m_number_token;
                        break;
                    case Symbol:
                        m_current_token = m_special_token;
                        break;
                    case Letter:
                        m_current_token = m_word_token;
                        break;
                    case Quote:
                        m_current_token = m_string_token;
                        break;
                    default:
                        m_error = true;
                        m_current_token = m_error_token;
                        break;
                    }
                    if (!m_current_token.Get(m_text_buffer))
                    {
                        m_error = true;
                        m_current_token = m_error_token;
                    }
                }
            }
            return m_current_token;
        }

        char SkipWhiteSpace()
        {
            while (!m_text_buffer.Eof())
            {
                char code = Tokenizer.GetCode(m_text_buffer.Char());
                switch (code)
                {
                case WhiteSpace:
                    break;
                case Error:
                    m_error = true;
                    LogWrapper.LogError("Expression: Tokenizer.SkipWhiteSpace(), illegal character in the buffer, index = ", m_text_buffer.CurrentIndex, ", char = ", m_text_buffer.Char());
                    return code;
                default:
                    return code;
                }
                m_text_buffer.NextChar();
            }
            RecyclableObject.Recycle(m_text_buffer);
            m_text_buffer = null;
            return Eof;
        }
    }
}