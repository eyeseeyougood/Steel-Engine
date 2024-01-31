using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine.Common
{
    public static class Extentions
    {
        public static bool ContainsAnInstanceEqualTo<T>(this List<T> list, T example)
        {
            bool found = false;

            foreach (T element in list)
            {
                if (example.Equals(element))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        public static string GetErrorString(ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ErrorCode.NoError:
                    return "NoError";
                case ErrorCode.InvalidEnum:
                    return "InvalidEnum";
                case ErrorCode.InvalidValue:
                    return "InvalidValue";
                case ErrorCode.InvalidOperation:
                    return "InvalidOperation";
                case ErrorCode.StackOverflow:
                    return "StackOverflow";
                case ErrorCode.StackUnderflow:
                    return "StackUnderflow";
                case ErrorCode.OutOfMemory:
                    return "OutOfMemory";
                case ErrorCode.TableTooLarge:
                    return "TableTooLarge";
                default:
                    return $"Unknown OpenGL Error ({errorCode})";
            }
        }
    }
}
