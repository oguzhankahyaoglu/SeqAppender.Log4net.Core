using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using log4net.Core;

namespace SeqAppender.Log4net.Core
{
      internal static class LoggingEventFormatter
    {
        private static readonly IDictionary<string, string> _levelMap =
            new Dictionary<string, string>
            {
                {
                    "DEBUG",
                    "Debug"
                },
                {
                    "INFO",
                    "Information"
                },
                {
                    "WARN",
                    "Warning"
                },
                {
                    "ERROR",
                    "Error"
                },
                {
                    "FATAL",
                    "Fatal"
                }
            };

        private static readonly IDictionary<Type, Action<object, TextWriter>> _literalWriters =
            new Dictionary<Type, Action<object, TextWriter>>
            {
                {
                    typeof(bool),
                    (v, w) => WriteBoolean((bool) v, w)
                },
                {
                    typeof(char),
                    (v, w) =>
                        WriteString(
                            ((char) v).ToString(CultureInfo.InvariantCulture), w)
                },
                {
                    typeof(byte),
                    WriteToString
                },
                {
                    typeof(sbyte),
                    WriteToString
                },
                {
                    typeof(short),
                    WriteToString
                },
                {
                    typeof(ushort),
                    WriteToString
                },
                {
                    typeof(int),
                    WriteToString
                },
                {
                    typeof(uint),
                    WriteToString
                },
                {
                    typeof(long),
                    WriteToString
                },
                {
                    typeof(ulong),
                    WriteToString
                },
                {
                    typeof(float),
                    WriteToString
                },
                {
                    typeof(double),
                    WriteToString
                },
                {
                    typeof(Decimal),
                    WriteToString
                },
                {
                    typeof(string),
                    (v, w) => WriteString((string) v, w)
                },
                {
                    typeof(DateTime),
                    (v, w) => WriteDateTime((DateTime) v, w)
                },
                {
                    typeof(DateTimeOffset),
                    (v, w) => WriteOffset((DateTimeOffset) v, w)
                }
            };

        private const uint Log4NetEventType = 67145;

        public static void ToJson(
            LoggingEvent[] events,
            StringWriter payload,
            List<AdoNetAppenderParameter> mParameters)
        {
            string str = "";
            foreach (LoggingEvent loggingEvent in events)
            {
                payload.Write(str);
                str = ",";
                StringWriter payload1 = payload;
                List<AdoNetAppenderParameter> mParameters1 = mParameters;
                ToJson(loggingEvent, payload1, mParameters1);
            }
        }

        private static void ToJson(
            LoggingEvent loggingEvent,
            StringWriter payload,
            List<AdoNetAppenderParameter> mParameters)
        {
            string str;
            if (!_levelMap.TryGetValue(loggingEvent.Level.Name, out str))
                str = "Information";
            payload.Write("{");
            string precedingDelimiter1 = "";
            WriteJsonProperty("Timestamp",
                new DateTimeOffset(loggingEvent.TimeStamp, DateTimeOffset.Now.Offset),
                ref precedingDelimiter1, payload);
            WriteJsonProperty("Level", str, ref precedingDelimiter1,
                payload);
            WriteJsonProperty("EventType", 67145U, ref precedingDelimiter1,
                payload);
            WriteJsonProperty("MessageTemplate",
                loggingEvent.RenderedMessage.Replace("{", "{{").Replace("}", "}}"), ref precedingDelimiter1,
                payload);
            if (loggingEvent.ExceptionObject != null)
                WriteJsonProperty("Exception", loggingEvent.ExceptionObject,
                    ref precedingDelimiter1, payload);
            payload.Write(",\"Properties\":{");
            HashSet<string> stringSet = new HashSet<string>();
            string precedingDelimiter2 = "";
            using (List<AdoNetAppenderParameter>.Enumerator enumerator = mParameters.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    AdoNetAppenderParameter current = enumerator.Current;
                    WriteJsonProperty(current.ParameterName,
                        current.Layout.Format(loggingEvent), ref precedingDelimiter2, payload);
                }
            }

            WriteJsonProperty(SanitizeKey("log4net:Logger"),
                loggingEvent.LoggerName, ref precedingDelimiter2, payload);
            foreach (DictionaryEntry property in loggingEvent.GetProperties())
            {
                string name = SanitizeKey(property.Key.ToString());
                if (!stringSet.Contains(name))
                {
                    stringSet.Add(name);
                    WriteJsonProperty(name, property.Value, ref precedingDelimiter2,
                        payload);
                }
            }

            payload.Write("}");
            payload.Write("}");
        }

        private static string SanitizeKey(string key)
        {
            return new string(key.Replace(":", "_").Where(c =>
            {
                if (c != '_')
                    return char.IsLetterOrDigit(c);
                return true;
            }).ToArray());
        }

        private static void WriteJsonProperty(
            string name,
            object value,
            ref string precedingDelimiter,
            TextWriter output)
        {
            output.Write(precedingDelimiter);
            WritePropertyName(name, output);
            WriteLiteral(value, output);
            precedingDelimiter = ",";
        }

        private static void WritePropertyName(string name, TextWriter output)
        {
            output.Write("\"");
            output.Write(name);
            output.Write("\":");
        }

        private static void WriteLiteral(object value, TextWriter output)
        {
            if (value == null)
            {
                output.Write("null");
            }
            else
            {
                value = GetValueAsLiteral(value);
                Action<object, TextWriter> action;
                if (_literalWriters.TryGetValue(value.GetType(), out action))
                    action(value, output);
                else
                    WriteString(value.ToString(), output);
            }
        }

        private static void WriteToString(object number, TextWriter output)
        {
            output.Write(number.ToString());
        }

        private static void WriteBoolean(bool value, TextWriter output)
        {
            output.Write(value ? "true" : "false");
        }

        private static void WriteOffset(DateTimeOffset value, TextWriter output)
        {
            output.Write("\"");
            output.Write(value.ToString("o"));
            output.Write("\"");
        }

        private static void WriteDateTime(DateTime value, TextWriter output)
        {
            output.Write("\"");
            output.Write(value.ToString("o"));
            output.Write("\"");
        }

        private static void WriteString(string value, TextWriter output)
        {
            string str = Escape(value);
            output.Write("\"");
            output.Write(str);
            output.Write("\"");
        }

        private static string Escape(string s)
        {
            if (s == null)
                return null;
            StringBuilder stringBuilder = null;
            int startIndex = 0;
            for (int index = 0; index < s.Length; ++index)
            {
                char ch = s[index];
                if (ch < ' ' || ch == '\\' || ch == '"')
                {
                    if (stringBuilder == null)
                        stringBuilder = new StringBuilder();
                    stringBuilder.Append(s.Substring(startIndex, index - startIndex));
                    startIndex = index + 1;
                    switch (ch)
                    {
                        case '\t':
                            stringBuilder.Append("\\t");
                            continue;
                        case '\n':
                            stringBuilder.Append("\\n");
                            continue;
                        case '\f':
                            stringBuilder.Append("\\f");
                            continue;
                        case '\r':
                            stringBuilder.Append("\\r");
                            continue;
                        case '"':
                            stringBuilder.Append("\\\"");
                            continue;
                        case '\\':
                            stringBuilder.Append("\\\\");
                            continue;
                        default:
                            stringBuilder.Append("\\u");
                            stringBuilder.Append(((int) ch).ToString("X4"));
                            continue;
                    }
                }
            }

            if (stringBuilder == null)
                return s;
            if (startIndex != s.Length)
                stringBuilder.Append(s.Substring(startIndex));
            return stringBuilder.ToString();
        }

        /// <summary>
        /// GetValueAsLiteral attempts to transform the (string) object into a literal type prior to json serialization.
        /// </summary>
        /// <param name="value">The value to be transformed/parsed.</param>
        /// <returns>A translated representation of the literal object type instead of a string.</returns>
        private static object GetValueAsLiteral(object value)
        {
            string s = value as string;
            if (s == null)
                return value;
            Decimal result1;
            if (Decimal.TryParse(s, out result1))
                return result1;
            DateTime result2;
            if (DateTime.TryParse(s, out result2))
                return result2;
            return value;
        }
    }
}