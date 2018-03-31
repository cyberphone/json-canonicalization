// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections.Generic;

// Simple JSON parser for .NET

namespace Org.Webpki.Json
{
    public class JSONReader
    {
        private readonly TextReader _r;
        private int _line = 1, _column = 0;
        private int _peek;
        private bool _has_peek;
        private bool _prev_lf;

        public JSONReader(TextReader reader)
        {
            Debug.Assert(reader != null);

            _r = reader;
        }

        public JSONReader(string jsonData)
            : this(new StringReader(jsonData))
        {
        }

        public JSONReader(byte[] jsonData)
            : this(new UTF8Encoding().GetString(jsonData))
        {

        }

        public object Read()
        {
            object v = ReadCore();
            SkipSpaces();
            if (ReadChar() >= 0)
            {
                throw JsonError("Extra characters");
            }
            return v;
        }

        private object ReadCore()
        {
            SkipSpaces();
            int c = PeekChar();
            if (c < 0)
            {
                throw JsonError("Missing input data");
            }

            switch (c)
            {
                case '[':
                    ReadChar();
                    var list = new List<object>();
                    SkipSpaces();
                    if (PeekChar() == ']')
                    {
                        ReadChar();
                        return list;
                    }

                    while (true)
                    {
                        list.Add(ReadCore());
                        SkipSpaces();
                        c = PeekChar();
                        if (c != ',')
                            break;
                        ReadChar();
                        continue;
                    }

                    if (ReadChar() != ']')
                    {
                        throw JsonError("Expected ']'");
                    }

                    return list;

                case '{':
                    ReadChar();
                    var obj = new OrderedDictionary();
                    SkipSpaces();
                    if (PeekChar() == '}')
                    {
                        ReadChar();
                        return obj;
                    }

                    while (true)
                    {
                        SkipSpaces();
                        if (PeekChar() == '}')
                        {
                            ReadChar();
                            break;
                        }
                        string name = ReadStringLiteral();
                        SkipSpaces();
                        Expect(':');
                        SkipSpaces();
                        obj[name] = ReadCore(); // it does not reject duplicate names.
                        SkipSpaces();
                        c = ReadChar();
                        if (c == ',')
                        {
                            continue;
                        }
                        if (c == '}')
                        {
                            break;
                        }
                    }
                    return obj;

                case 't':
                    Expect("true");
                    return true;

                case 'f':
                    Expect("false");
                    return false;

                case 'n':
                    Expect("null");
                    return null;

                case '"':
                    return ReadStringLiteral();

                default:
                    if ('0' <= c && c <= '9' || c == '-')
                    {
                        return ReadNumericLiteral();
                    }
                    throw JsonError("Unexpected character: " + (char)c);
            }
        }

        private int PeekChar()
        {
            if (!_has_peek)
            {
                _peek = _r.Read();
                _has_peek = true;
            }
            return _peek;
        }

        private int ReadChar()
        {
            int v = _has_peek ? _peek : _r.Read();

            _has_peek = false;

            if (_prev_lf)
            {
                _line++;
                _column = 0;
                _prev_lf = false;
            }

            if (v == '\n')
            {
                _prev_lf = true;
            }

            _column++;

            return v;
        }

        private void SkipSpaces()
        {
            while (true)
            {
                switch (PeekChar())
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        ReadChar();
                        continue;

                    default:
                        return;
                }
            }
        }

        // It could return either int, long, ulong, decimal or double, depending on the parsed value.
        private object ReadNumericLiteral()
        {
            var sb = new StringBuilder();

            if (PeekChar() == '-')
            {
                sb.Append((char)ReadChar());
            }

            int c;
            int x = 0;
            bool zeroStart = PeekChar() == '0';
            for (; ; x++)
            {
                c = PeekChar();
                if (c < '0' || '9' < c)
                {
                    break;
                }

                sb.Append((char)ReadChar());
                if (zeroStart && x == 1)
                {
                    throw JsonError("Leading zeros");
                }
            }

            if (x == 0) // Reached e.g. for "- "
            {
                throw JsonError("Missing digits");
            }

            int fdigits = 0;
            if (PeekChar() == '.')
            {
                sb.Append((char)ReadChar());
                if (PeekChar() < 0)
                {
                    throw JsonError("Syntax error");
                }

                while (true)
                {
                    c = PeekChar();
                    if (c < '0' || '9' < c)
                    {
                        break;
                    }

                    sb.Append((char)ReadChar());
                    fdigits++;
                }
                if (fdigits == 0)
                {
                    throw JsonError("Syntax error");
                }
            }

            c = PeekChar();
            if (c == 'e' || c == 'E')
            {
                // exponent
                sb.Append((char)ReadChar());
                if (PeekChar() < 0)
                {
                    throw JsonError("Incomplete exponent");
                }

                c = PeekChar();
                if (c == '-')
                {
                    sb.Append((char)ReadChar());
                }
                else if (c == '+')
                {
                    sb.Append((char)ReadChar());
                }

                if (PeekChar() < 0)
                {
                    throw JsonError("Incomplete exponent");
                }

                while (true)
                {
                    c = PeekChar();
                    if (c < '0' || '9' < c)
                    {
                        break;
                    }

                    sb.Append((char)ReadChar());
                }
            }

            return double.Parse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private readonly StringBuilder _vb = new StringBuilder();

        private string ReadStringLiteral()
        {
            if (PeekChar() != '"')
            {
                throw JsonError("Invalid string literal");
            }

            ReadChar();
            _vb.Length = 0;
            while (true)
            {
                int c = ReadChar();
                if (c < 0)
                {
                    throw JsonError("String nit closed");
                }

                if (c == '"')
                {
                    return _vb.ToString();
                }
                else if (c != '\\')
                {
                    _vb.Append((char)c);
                    continue;
                }

                // escaped expression
                c = ReadChar();
                if (c < 0)
                {
                    throw JsonError("Incomplete escape");
                }
                switch (c)
                {
                    case '"':
                    case '\\':
                    case '/':
                        _vb.Append((char)c);
                        break;
                    case 'b':
                        _vb.Append('\x8');
                        break;
                    case 'f':
                        _vb.Append('\f');
                        break;
                    case 'n':
                        _vb.Append('\n');
                        break;
                    case 'r':
                        _vb.Append('\r');
                        break;
                    case 't':
                        _vb.Append('\t');
                        break;
                    case 'u':
                        ushort cp = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            cp <<= 4;
                            if ((c = ReadChar()) < 0)
                            {
                                throw JsonError("Incomplete escape");
                            }

                            if ('0' <= c && c <= '9')
                            {
                                cp += (ushort)(c - '0');
                            }
                            if ('A' <= c && c <= 'F')
                            {
                                cp += (ushort)(c - 'A' + 10);
                            }
                            if ('a' <= c && c <= 'f')
                            {
                                cp += (ushort)(c - 'a' + 10);
                            }
                        }
                        _vb.Append((char)cp);
                        break;
                    default:
                        throw JsonError("Unexpected escape character");
                }
            }
        }

        private void Expect(char expected)
        {
            int c;
            if ((c = ReadChar()) != expected)
            {
                throw JsonError("Expected character: " + expected + " got: " + (char)c);
            }
        }

        private void Expect(string expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                if (ReadChar() != expected[i])
                {
                    throw JsonError("Expected string: " + expected);
                }
            }
        }

        private Exception JsonError(string msg)
        {
            return new ArgumentException(msg + " at[" + _line + "," + _column +"]");
        }
    }
}
