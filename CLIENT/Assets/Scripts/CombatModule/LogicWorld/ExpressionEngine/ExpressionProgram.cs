using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public class ExpressionProgram : IRecyclable
    {
        protected bool m_error_occurred = false;
        protected List<ExpressionVariable> m_variables = new List<ExpressionVariable>();
        protected List<long> m_instructions = new List<long>();

        #region Compile的临时变量，Reset不用关心
        protected Tokenizer m_tokenizer;
        protected Token m_token;
        protected TokenType m_token_type;
        protected List<string> m_raw_variable = new List<string>();
        #endregion

        public virtual void Reset()
        {
            m_error_occurred = false;
            RecycleVariable();
            m_instructions.Clear();
        }

        public void CopyFrom(ExpressionProgram rhs)
        {
            m_error_occurred = rhs.m_error_occurred;
            RecycleVariable();
            for (int i = 0; i < rhs.m_variables.Count; ++i)
            {
                ExpressionVariable variable = RecyclableObject.Create<ExpressionVariable>();
                variable.CopyFrom(rhs.m_variables[i]);
                m_variables.Add(variable);
            }
            m_instructions.Clear();
            for (int i = 0; i < rhs.m_instructions.Count; ++i)
                m_instructions.Add(rhs.m_instructions[i]);
        }

        void RecycleVariable()
        {
            for (int i = 0; i < m_variables.Count; ++i)
                RecyclableObject.Recycle(m_variables[i]);
            m_variables.Clear();
        }

        public virtual bool Compile(string formula_string)
        {
            m_error_occurred = false;
            m_tokenizer = RecyclableObject.Create<Tokenizer>();
            m_tokenizer.Construct(formula_string);
            m_token = null;
            m_token_type = TokenType.ERROR;
            GetToken();
            ParseExpression();
            RecyclableObject.Recycle(m_tokenizer);
            m_tokenizer = null;
            m_token = null;
            m_token_type = TokenType.ERROR;
            m_raw_variable.Clear();
            return !m_error_occurred;
        }

        public List<ExpressionVariable> GetAllVariables()
        {
            return m_variables;
        }

        public bool IsConstant()
        {
            return m_variables.Count == 0;
        }

        public virtual FixPoint Evaluate(IExpressionVariableProvider variable_provider)
        {
            Stack<FixPoint> stack = new Stack<FixPoint>();
            FixPoint var1, var2, var3;
            int index = 0;
            int total_count = m_instructions.Count;
            while (index < total_count)
            {
                OperationCode op_code = (OperationCode)m_instructions[index];
                ++index;
                switch (op_code)
                {
                case OperationCode.PUSH_NUMBER:
                    stack.Push(FixPoint.CreateFromRaw(m_instructions[index]));
                    ++index;
                    break;
                case OperationCode.PUSH_VARIABLE:
                    if (variable_provider != null)
                    {
                        var1 = variable_provider.GetVariable(m_variables[(int)m_instructions[index]], 0);
                        stack.Push(var1);
                    }
                    else
                    {
                        stack.Push(FixPoint.Zero);
                    }
                    ++index;
                    break;
                case OperationCode.NEGATE:
                    var1 = stack.Pop();
                    stack.Push(-var1);
                    break;
                case OperationCode.ADD:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(var1 + var2);
                    break;
                case OperationCode.SUBTRACT:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(var1 - var2);
                    break;
                case OperationCode.MULTIPLY:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(var1 * var2);
                    break;
                case OperationCode.DIVIDE:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(var1 / var2);
                    break;
                case OperationCode.SIN:
                    var1 = stack.Pop();
                    stack.Push(FixPoint.Sin(var1));
                    break;
                case OperationCode.COS:
                    var1 = stack.Pop();
                    stack.Push(FixPoint.Cos(var1));
                    break;
                case OperationCode.TAN:
                    var1 = stack.Pop();
                    stack.Push(FixPoint.Tan(var1));
                    break;
                case OperationCode.SQRT:
                    var1 = stack.Pop();
                    stack.Push(FixPoint.Sqrt(var1));
                    break;
                case OperationCode.MIN:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(FixPoint.Min(var1, var2));
                    break;
                case OperationCode.MAX:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(FixPoint.Max(var1, var2));
                    break;
                case OperationCode.CLAMP:
                    var3 = stack.Pop();
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(FixPoint.Clamp(var1, var2, var3));
                    break;
                case OperationCode.EQUAL:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 == var2)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.NOT_EQUAL:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 != var2)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.GREATER:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 > var2)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.GREATER_EQUAL:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 >= var2)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.LESS:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 < var2)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.LESS_EQUAL:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 <= var2)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.AND:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 != FixPoint.Zero && var2 != FixPoint.Zero)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.OR:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 != FixPoint.Zero || var2 != FixPoint.Zero)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.NOT:
                    var1 = stack.Pop();
                    if (var1 == FixPoint.Zero)
                        stack.Push(FixPoint.One);
                    else
                        stack.Push(FixPoint.Zero);
                    break;
                case OperationCode.AND_BITWISE:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(var1 & var2);
                    break;
                case OperationCode.OR_BITWISE:
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    stack.Push(var1 | var2);
                    break;
                case OperationCode.CONDITIONAL_EXPRESSION:
                    var3 = stack.Pop();
                    var2 = stack.Pop();
                    var1 = stack.Pop();
                    if (var1 != FixPoint.Zero)
                        stack.Push(var2);
                    else
                        stack.Push(var3);
                    break;
                default:
                    break;
                }
            }
            return stack.Pop();
        }

        #region Compile
        enum OperationCode
        {
            END = 0,
            NEGATE,
            ADD,
            SUBTRACT,
            MULTIPLY,
            DIVIDE,

            EQUAL,
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

            CONDITIONAL_EXPRESSION,

            SIN,
            COS,
            TAN,
            SQRT,
            MIN,
            MAX,
            CLAMP,

            PUSH_NUMBER,
            PUSH_VARIABLE,
        };

        void GetToken()
        {
            m_token = m_tokenizer.GetNextToken();
            m_token_type = m_token.Type;
            if (m_token_type == TokenType.ERROR)
                m_error_occurred = true;
        }

        void ParseExpression()
        {
            ParseSimpleExpression();
            OperationCode comparison_op = OperationCode.END;
            switch (m_token_type)
            {
            case TokenType.EQUAL:
            case TokenType.EQUAL_EQUAL:
                comparison_op = OperationCode.EQUAL;
                break;
            case TokenType.NOT_EQUAL:
                comparison_op = OperationCode.NOT_EQUAL;
                break;
            case TokenType.GREATER:
                comparison_op = OperationCode.GREATER;
                break;
            case TokenType.GREATER_EQUAL:
                comparison_op = OperationCode.GREATER_EQUAL;
                break;
            case TokenType.LESS:
                comparison_op = OperationCode.LESS;
                break;
            case TokenType.LESS_EQUAL:
                comparison_op = OperationCode.LESS_EQUAL;
                break;
            default:
                break;
            }
            if (comparison_op != OperationCode.END)
            {
                GetToken();
                ParseSimpleExpression();
                AppendOperation(comparison_op);
            }
        }

        void ParseSimpleExpression()
        {
            OperationCode unary_op = OperationCode.END;
            if (m_token_type == TokenType.PLUS)
            {
                GetToken();
            }
            else if (m_token_type == TokenType.MINUS)
            {
                unary_op = OperationCode.NEGATE;
                GetToken();
            }
            ParseTerm();
            if (unary_op != OperationCode.END)
                AppendOperation(unary_op);

            while (true)
            {
                OperationCode op_code = OperationCode.END;
                switch (m_token_type)
                {
                case TokenType.PLUS:
                    op_code = OperationCode.ADD;
                    break;
                case TokenType.MINUS:
                    op_code = OperationCode.SUBTRACT;
                    break;
                case TokenType.OR:
                    op_code = OperationCode.OR;
                    break;
                case TokenType.OR_BITWISE:
                    op_code = OperationCode.OR_BITWISE;
                    break;
                default:
                    break;
                }
                if (op_code == OperationCode.END)
                    break;
                GetToken();
                ParseTerm();
                AppendOperation(op_code);
            }
        }

        void ParseTerm()
        {
            ParseFactor();
            while (true)
            {
                OperationCode op_code = OperationCode.END;
                switch (m_token_type)
                {
                case TokenType.STAR:
                    op_code = OperationCode.MULTIPLY;
                    break;
                case TokenType.SLASH:
                    op_code = OperationCode.DIVIDE;
                    break;
                case TokenType.AND:
                    op_code = OperationCode.AND;
                    break;
                case TokenType.AND_BITWISE:
                    op_code = OperationCode.AND_BITWISE;
                    break;
                default:
                    break;
                }
                if (op_code == OperationCode.END)
                    break;
                GetToken();
                ParseFactor();
                AppendOperation(op_code);
            }

            if (m_token_type == TokenType.QUESTION)
            {
                GetToken();
                ParseFactor();
                if (m_token_type != TokenType.SEMICOLON)
                    LogWrapper.LogError("Expression: ParseTerm(), TokenType.SEMICOLON expected");
                GetToken();
                ParseFactor();
                AppendOperation(OperationCode.CONDITIONAL_EXPRESSION);
            }
        }

        void ParseFactor()
        {
            switch (m_token_type)
            {
            case TokenType.NUMBER:
                AppendNumber(m_token.GetNumber());
                GetToken();
                break;
            case TokenType.LEFT_PAREN:
                GetToken();
		        ParseExpression();
                if (m_token_type != TokenType.RIGHT_PAREN)
                {
                    m_error_occurred = true;
                    LogWrapper.LogError("Expression: ParseFactor(), ')' expected");
                    return;
                }
                GetToken();
                break;
            case TokenType.IDENTIFIER:
                m_raw_variable.Clear();
                m_raw_variable.Add(m_token.GetRawString());
                GetToken();
                while (m_token_type == TokenType.PERIOD)
                {
                    GetToken();
                    if (m_token_type != TokenType.IDENTIFIER)
                    {
                        m_error_occurred = true;
                        LogWrapper.LogError("Expression: ParseFactor(), TokenType.Identifier expected");
                        return;
                    }
                    m_raw_variable.Add(m_token.GetRawString());
                    GetToken();
                }
                AppendVariable(m_raw_variable);
                m_raw_variable.Clear();
                break;
            case TokenType.NOT:
		        GetToken();
		        ParseFactor();
                AppendOperation(OperationCode.NOT);
                break;
            case TokenType.SINE:
                ParseBuildInFunction(OperationCode.SIN, 1);
                break;
            case TokenType.COSINE:
                ParseBuildInFunction(OperationCode.COS, 1);
                break;
            case TokenType.TANGENT:
                ParseBuildInFunction(OperationCode.TAN, 1);
                break;
            case TokenType.SQUARE_ROOT:
                ParseBuildInFunction(OperationCode.SQRT, 1);
                break;
            case TokenType.MINIMUM:
                ParseBuildInFunction(OperationCode.MIN, 2);
                break;
            case TokenType.MAXIMUM:
                ParseBuildInFunction(OperationCode.MAX, 2);
                break;
            case TokenType.CLAMP:
                ParseBuildInFunction(OperationCode.CLAMP, 3);
                break;
            default:
                break;
            }
        }

        void ParseBuildInFunction(OperationCode opcode, int param_count)
        {
            GetToken();
            if (m_token_type != TokenType.LEFT_PAREN)
                LogWrapper.LogError("Expression: ParseBuildInFunction, '(' expected");
            if (param_count > 0)
            {
                GetToken();
                ParseExpression();
                --param_count;
                while (param_count > 0)
                {
                    if (m_token_type != TokenType.COMMA)
                    {
                        m_error_occurred = true;
                        LogWrapper.LogError("Expression: ParseBuildInFunction, ',' expected");
                        return;
                    }
                    GetToken();
                    ParseExpression();
                    --param_count;
                }
            }
            if (m_token_type != TokenType.RIGHT_PAREN)
            {
                m_error_occurred = true;
                LogWrapper.LogError("Expression: ParseBuildInFunction, ')' expected");
                return;
            }
            AppendOperation(opcode);
            GetToken();
        }

        void AppendOperation(OperationCode op_code)
        {
            m_instructions.Add((long)op_code);
        }

        void AppendNumber(FixPoint number)
        {
            AppendOperation(OperationCode.PUSH_NUMBER);
            m_instructions.Add(number.RawValue);
        }

        void AppendVariable(List<string> variable)
        {
            AppendOperation(OperationCode.PUSH_VARIABLE);
            int index = AddVariable(variable);
            m_instructions.Add(index);
        }

        int AddVariable(List<string> raw_variable)
        {
            ExpressionVariable variable = RecyclableObject.Create<ExpressionVariable>();
            variable.Construct(raw_variable);
            int index = m_variables.Count;
            m_variables.Add(variable);
            return index;
        }
        #endregion
    }
}