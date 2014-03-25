using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  internal class BaseReader
  {
    TextReader r;
    int line=1, column=0;
    StringBuilder sb=new StringBuilder();
    int peek;
    bool has_peek, prev_lf;

    public BaseReader(TextReader reader)
    {
      if (reader==null) throw new ArgumentNullException("reader");
      this.r=reader;
    }

    public int PeekChar()
    {
      if (!has_peek)
      {
        peek=r.Read();
        has_peek=true;
      }
      return peek;
    }

    public int ReadChar()
    {
      int v=has_peek?peek:r.Read();

      has_peek=false;

      if (prev_lf)
      {
        line++;
        column=0;
        prev_lf=false;
      }

      if (v=='\n') prev_lf=true;
      column++;

      return v;
    }

    public static bool IsWhite(char c)
    {
      return c==' ' || c=='\t' || c=='\r' || c=='\n';
    }

    public void SkipWhite()
    {
      for (; ; )
      {
        if (IsWhite((char)PeekChar())) ReadChar();
        else break;
      }
    }

    // It could return either int, long or decimal, depending on the parsed value.
    public object ReadNumericLiteral()
    {
      bool negative=false;
      if (PeekChar()=='-')
      {
        negative=true;
        ReadChar();
        if (PeekChar()<0) throw ParseError("Invalid JSON numeric literal; extra negation");
      }

      int c;
      decimal val=0;
      int x=0;
      bool zeroStart=PeekChar()=='0';
      for (; ; x++)
      {
        c=PeekChar();
        if (c<'0' || c>'9') break;
        val=val*10+(c-'0');
        ReadChar();
        if (zeroStart && x==1 && c=='0') throw ParseError("leading multiple zeros are not allowed");
      }

      // fraction

      bool hasFrac=false;
      decimal frac=0;
      int fdigits=0;
      if (PeekChar()=='.')
      {
        hasFrac=true;
        ReadChar();
        if (PeekChar()<0) throw ParseError("Invalid JSON numeric literal; extra dot");
        decimal d=10;
        for (;;)
        {
          c=PeekChar();
          if (c<'0' || '9'<c) break;
          ReadChar();
          frac+=(c-'0')/d;
          d*=10;
          fdigits++;
        }
        if (fdigits==0) throw ParseError("Invalid JSON numeric literal; extra dot");
      }
      frac=Decimal.Round(frac, fdigits);

      c=PeekChar();
      if (c!='e' && c!='E')
      {
        if (!hasFrac)
        {
          if (negative && int.MinValue<=-val || !negative && val<=int.MaxValue)
            return (int)(negative?-val:val);
          if (negative && long.MinValue<=-val || !negative && val<=long.MaxValue)
            return (long)(negative?-val:val);
        }
        var v=val+frac;
        return negative?-v:v;
      }

      // exponent

      ReadChar();

      int exp=0;
      if (PeekChar()<0) throw new ArgumentException("Invalid JSON numeric literal; incomplete exponent");

      bool negexp=false;
      c=PeekChar();
      if (c=='-')
      {
        ReadChar();
        negexp=true;
      }
      else if (c=='+') ReadChar();

      if (PeekChar()<0) throw ParseError("Invalid JSON numeric literal; incomplete exponent");

      for (; ; )
      {
        c=PeekChar();
        if (c<'0' || c>'9') break;
        exp=exp*10+(c-'0');
        ReadChar();
      }
      // it is messy to handle exponent, so I just use Decimal.Parse() with assured JSON format.
      if (negexp) return new Decimal((double)(val+frac) / Math.Pow(10, exp));
      int[] bits=Decimal.GetBits(val+frac);
      return new Decimal(bits[0], bits[1], bits[2], negative, (byte)exp);
    }

    public string ReadStringLiteral()
    {
      if (PeekChar()!='"') throw ParseError("Invalid JSON string literal format");

      ReadChar();
      sb.Length=0;
      for (; ; )
      {
        int c=ReadChar();
        if (c<0) throw ParseError("JSON string is not closed");
        if (c=='"') return sb.ToString();
        else if (c!='\\')
        {
          sb.Append((char)c);
          continue;
        }

        // escaped expression
        c=ReadChar();
        if (c<0)
          throw ParseError("Invalid JSON string literal; incomplete escape sequence");
        switch (c)
        {
          case '"':
          case '\\':
          case '/': sb.Append((char)c); break;
          case 'b': sb.Append('\x8'); break;
          case 'f': sb.Append('\f'); break;
          case 'n': sb.Append('\n'); break;
          case 'r': sb.Append('\r'); break;
          case 't': sb.Append('\t'); break;
          case 'u':
            ushort cp=0;
            for (int i=0; i<4; i++)
            {
              cp <<= 4;
              if ((c=ReadChar())<0)
                throw ParseError("Incomplete unicode character escape literal");
              if (c>='0' && c<='9') cp+=(ushort)(c-'0');
              if (c>='A' && c<='F') cp+=(ushort)(c-'A'+10);
              if (c>='a' && c<='f') cp+=(ushort)(c-'a'+10);
            }
            sb.Append((char)cp);
            break;
          default:
            throw ParseError("Invalid JSON string literal; unexpected escape character");
        }
      }
    }

    public void Expect(char expected)
    {
      int c;
      if ((c=ReadChar())!=expected)
        throw ParseError(String.Format("Expected '{0}', got '{1}'", expected, (char)c));
    }

    public void Expect(string expected)
    {
      for (int i=0; i<expected.Length; i++)
        if (ReadChar()!=expected[i])
          throw ParseError(String.Format("Expected '{0}', differed at {1}", expected, i));
    }

    public Exception ParseError(string msg)
    {
      return new ArgumentException(String.Format("{0}. At line {1}, column {2}", msg, line, column));
    }
  }
}