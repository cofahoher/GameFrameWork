using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public enum TokenType
    {
	    ERROR = 0,
        EOF,

        NUMBER,

        PLUS,
        MINUS,
        STAR,
        SLASH,
        LEFT_PAREN,
        RIGHT_PAREN,
        LEFT_BRACKET,
        RIGHT_BRACKET,
        PERIOD,
        COMMA,
        QUESTION,
        SEMICOLON,

        EQUAL,
        EQUAL_EQUAL,
        NOT_EQUAL,
        GREATER,
        GREATER_EQUAL,
        LESS,
        LESS_EQUAL,

        NOT,
        AND,
        OR,
        AND_BITWISE,
        OR_BITWISE,
	    
	    SINE,
	    COSINE,
	    TANGENT,
	    SQUARE_ROOT,	    
	    MINIMUM,
	    MAXIMUM,
        CLAMP,
        IDENTIFIER,

        STRING_TYPE,
    }

    public abstract class Token
    {
        protected TokenType m_type;

        public TokenType Type
        {
            get { return m_type; }
        }

        public abstract bool Get(TextBuffer text_buffer);

        public virtual FixPoint GetNumber() 
        {
            return FixPoint.Zero;
        }

        public virtual string GetRawString()
        {
            return "";
        }
    }

    public class ErrorToken : Token
    {
        public ErrorToken()
        {
            m_type = TokenType.ERROR;
        }

        public override bool Get(TextBuffer text_buffer)
        {
            return true;
        }
    }

    public class EofToken : Token
    {
        public EofToken()
        {
            m_type = TokenType.EOF;
        }

        public override bool Get(TextBuffer text_buffer)
        {
            return true;
        }
    }

    public class NumberToken : Token
    {
        FixPoint m_value;

        public NumberToken()
        {
            m_type = TokenType.NUMBER;
        }

        public override bool Get(TextBuffer text_buffer)
        {
            int start_index = text_buffer.CurrentIndex;
            int length = 1;
            while (true)
            {
                char ch = text_buffer.NextChar();
                if (ch == '.' || Tokenizer.GetCode(ch) == Tokenizer.Digit)
                    ++length;
                else
                    break;
            }
            string str = text_buffer.SubString(start_index, length);
            m_value = FixPoint.Parse(str);
            return true;
        }

        public override FixPoint GetNumber()
        {
            return m_value;
        }
    }

    public class SymbolToken : Token
    {
        public override bool Get(TextBuffer text_buffer)
        {
            char ch = text_buffer.Char();
            switch (ch)
            {
            case '+':
                m_type = TokenType.PLUS;
                text_buffer.NextChar();
                break;
            case '-':
                m_type = TokenType.MINUS;
                text_buffer.NextChar();
                break;
            case '*':
                m_type = TokenType.STAR;
                text_buffer.NextChar();
                break;
            case '/':
                m_type = TokenType.SLASH;
                text_buffer.NextChar();
                break;
            case '(':
                m_type = TokenType.LEFT_PAREN;
                text_buffer.NextChar();
                break;
            case ')':
                m_type = TokenType.RIGHT_PAREN;
                text_buffer.NextChar();
                break;
            case '[':
                m_type = TokenType.LEFT_BRACKET;
                text_buffer.NextChar();
                break;
            case ']':
                m_type = TokenType.RIGHT_BRACKET;
                text_buffer.NextChar();
                break;
            case '.':
                m_type = TokenType.PERIOD;
                text_buffer.NextChar();
                break;
            case ',':
                m_type = TokenType.COMMA;
                text_buffer.NextChar();
                break;
            case '?':
                m_type = TokenType.QUESTION;
                text_buffer.NextChar();
                break;
            case ':':
                m_type = TokenType.SEMICOLON;
                text_buffer.NextChar();
                break;
            case '=':
                m_type = TokenType.EQUAL;
                ch = text_buffer.NextChar();
                if (ch == '=')
                {
                    m_type = TokenType.EQUAL_EQUAL;
                    text_buffer.NextChar();
                }
                break;
            case '!':
                m_type = TokenType.NOT;
                ch = text_buffer.NextChar();
                if (ch == '=')
                {
                    m_type = TokenType.NOT_EQUAL;
                    text_buffer.NextChar();
                }
                break;
            case '>':
                m_type = TokenType.GREATER;
                ch = text_buffer.NextChar();
                if (ch == '=')
                {
                    m_type = TokenType.GREATER_EQUAL;
                    text_buffer.NextChar();
                }
                break;
            case '<':
                m_type = TokenType.LESS;
                ch = text_buffer.NextChar();
                if (ch == '=')
                {
                    m_type = TokenType.LESS_EQUAL;
                    text_buffer.NextChar();
                }
                break;
            case '&':
                m_type = TokenType.AND_BITWISE;
                ch = text_buffer.NextChar();
                if (ch == '&')
                {
                    m_type = TokenType.AND;
                    text_buffer.NextChar();
                }
                break;
            case '|':
                m_type = TokenType.OR_BITWISE;
                ch = text_buffer.NextChar();
                if (ch == '|')
                {
                    m_type = TokenType.OR;
                    text_buffer.NextChar();
                }
                break;
            default:
                LogWrapper.LogError("Expression: SymbolToken.Get(), illegal symbol, index = ", text_buffer.CurrentIndex, ", char = ", text_buffer.Char());
                m_type = TokenType.ERROR;
                return false;
            }
            return true;
        }
    }

    public class WordToken : Token
    {
        static Dictionary<string, TokenType> ms_reserved_words = new Dictionary<string, TokenType>();
        string m_raw_string;

        static WordToken()
        {
            ms_reserved_words["Sin"] = TokenType.SINE;
            ms_reserved_words["sin"] = TokenType.SINE;
            ms_reserved_words["Cos"] = TokenType.COSINE;
            ms_reserved_words["cos"] = TokenType.COSINE;
            ms_reserved_words["Tan"] = TokenType.TANGENT;
            ms_reserved_words["tan"] = TokenType.TANGENT;
            ms_reserved_words["Sqrt"] = TokenType.SQUARE_ROOT;
            ms_reserved_words["sqrt"] = TokenType.SQUARE_ROOT;
            ms_reserved_words["Min"] = TokenType.MINIMUM;
            ms_reserved_words["min"] = TokenType.MINIMUM;
            ms_reserved_words["Max"] = TokenType.MAXIMUM;
            ms_reserved_words["max"] = TokenType.MAXIMUM;
            ms_reserved_words["Clamp"] = TokenType.CLAMP;
            ms_reserved_words["clamp"] = TokenType.CLAMP;
        }
    
        public override bool Get(TextBuffer text_buffer)
        {
            int start_index = text_buffer.CurrentIndex;
            int length = 1;
            while (true)
            {
                char ch = text_buffer.NextChar();
                char code = Tokenizer.GetCode(ch);
                if (code == Tokenizer.Letter || code == Tokenizer.Digit)
                    ++length;
                else
                    break;
            }
            string str = text_buffer.SubString(start_index, length);
            if (!ms_reserved_words.TryGetValue(str, out m_type))
            {
                m_type = TokenType.IDENTIFIER;
                m_raw_string = str;
            }
            return true;
        }

        public override string GetRawString()
        {
            return m_raw_string;
        }
    }

    public class StringToken : Token
    {
        string m_raw_string;

        public StringToken()
        {
            m_type = TokenType.STRING_TYPE;
        }

        public override bool Get(TextBuffer text_buffer)
        {
            int start_index = text_buffer.CurrentIndex + 1;
            int length = 0;
            while (true)
            {
                char ch = text_buffer.NextChar();
                if (text_buffer.Eof())
                    break;
                if (Tokenizer.GetCode(ch) == Tokenizer.Quote)
                    break;
                ++length;
            }
            m_raw_string = text_buffer.SubString(start_index, length);
            return true;
        }

        public override string GetRawString()
        {
            return m_raw_string;
        }
    }
}