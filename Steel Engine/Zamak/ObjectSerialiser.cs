using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zamak
{
    public static class ObjectSerialiser
    {
        // KEY:

        // -- packet --
        // 01011010 == 90
        // Reliability Byte
        // UShort representing the target ID
        // String representing the class instance type
        // UShort representing the class instance iD
        // ... payload bytes
        // 10100101 == 165

        // -- character (takes 2 bytes) --
        // 01010101 == 85
        // character value

        // -- string --
        // 10101010 == 170
        // byte value dictating how many bytes are used to describe the size of the string
        // ... - byte 1
        // ... - byte 2
        // ... - chars
        // ... - chars
        // character value

        // -- UShort (takes 3 bytes) -- 
        // 01100110 == 102
        // ... byte1
        // ... byte2

        // -- Int32 (takes 5 bytes) -- 
        // 10011001 == 153
        // ... byte1
        // ... byte2
        // ... byte3
        // ... byte4

        // -- byte (takes 2 bytes) --
        // 11001100 == 204
        // ... byte

        // -- bool (takes 2 bytes) --
        // 00110011 == 51
        // 00000000 == 0
        // 00000001 == 1

        // -- float --
        // 11000011 == 195
        // byte1 of float length
        // byte2 of float length
        // byte3 of float length
        // byte4 of float length
        // ... bytes

        public static byte[] SerialiseObjects(object[] objectData)
        {
            List<byte> byteData = new List<byte>();

            foreach (object data in objectData)
            {
                switch (data.GetType().ToString())
                {
                    case "System.Char":
                        byteData.AddRange(SerialiseCharacter((char)data));
                        break;
                    case "System.String":
                        byteData.AddRange(SerialiseString((string)data));
                        break;
                    case "System.UInt16":
                        byteData.AddRange(SerialiseUShort((ushort)data));
                        break;
                    case "System.Int32":
                        byteData.AddRange(SerialiseInt32((int)data));
                        break;
                    case "System.Byte":
                        byteData.AddRange(SerialiseByte((byte)data));
                        break;
                    case "System.Boolean":
                        byteData.AddRange(SerialiseBool((bool)data));
                        break;
                    case "System.Single":
                        byteData.AddRange(SerialiseFloat((float)data));
                        break;
                }
            }

            return byteData.ToArray();
        }

        public static List<byte> AppendByteArray(List<byte> byteData, byte[] byteArray)
        {
            List<byte> returnValue = new List<byte>();

            foreach (byte data in byteData)
            {
                returnValue.Add(data);
            }

            foreach (byte data in byteArray)
            {
                returnValue.Add(data);
            }

            return returnValue;
        }

        public static byte[] SerialiseCharacter(char value)
        {
            byte[] returnValue = new byte[2];

            returnValue[0] = 85;
            returnValue[1] = (byte)value;

            return returnValue;
        }

        public static byte[] SerialiseString(string value)
        {
            List<byte> returnValue = new List<byte>();

            returnValue.Add(170);
            returnValue.Add((byte)BitConverter.GetBytes(value.Length).Length);
            foreach (byte data1 in BitConverter.GetBytes(value.Length))
            {
                returnValue.Add(data1);
            }
            foreach (char data2 in value)
            {
                returnValue.Add((byte)data2);
            }

            return returnValue.ToArray();
        }

        public static byte[] SerialiseUShort(ushort value)
        {
            byte[] returnValue = new byte[3];

            byte[] byteData = BitConverter.GetBytes(value);

            returnValue[0] = 102;
            returnValue[1] = byteData[0];
            returnValue[2] = byteData[1];

            return returnValue;
        }

        public static byte[] SerialiseInt32(int value)
        {
            byte[] returnValue = new byte[5];

            byte[] byteData = BitConverter.GetBytes(value);

            returnValue[0] = 153;
            returnValue[1] = byteData[0];
            returnValue[2] = byteData[1];
            returnValue[3] = byteData[2];
            returnValue[4] = byteData[3];

            return returnValue;
        }

        public static byte[] SerialiseByte(byte value)
        {
            byte[] returnValue = new byte[2];

            returnValue[0] = 204;
            returnValue[1] = value;

            return returnValue;
        }

        public static byte[] SerialiseBool(bool value)
        {
            byte[] returnValue = new byte[2];

            returnValue[0] = 51;
            returnValue[1] = value ? (byte)1 : (byte)0;

            return returnValue;
        }

        public static byte[] SerialiseFloat(float value)
        {
            List<byte> returnValue = new List<byte>();

            byte[] lengthBytes = BitConverter.GetBytes(BitConverter.GetBytes(value).Length);

            returnValue.Add(195);
            foreach (byte lengthData in lengthBytes)
            {
                returnValue.Add(lengthData);
            }
            returnValue.AddRange(BitConverter.GetBytes(value));

            return returnValue.ToArray();
        }

        public static object[] DeserialiseBytes(byte[] byteData)
        {
            List<object> returnValue = new List<object>();

            bool deserialising = false;
            string deserialisationType = "";
            List<byte> byteList = new List<byte>();
            List<byte> byteListBuffer = new List<byte>();
            int stage = 0;
            int valueBuffer = 0;
            int incrementer = 0;
            foreach (byte data in byteData)
            {
                if (deserialising)
                {
                    switch (deserialisationType)
                    {
                        case "character":
                            returnValue.Add(DeserialiseCharacter(data));
                            deserialising = false;
                            break;
                        case "string":
                            switch (stage)
                            {
                                case 0:
                                    byteList.Add(data);
                                    if (incrementer == byteList[0])
                                    {
                                        foreach (byte value in byteList)
                                        {
                                            byteListBuffer.Add(value);
                                        }
                                        byteListBuffer.RemoveAt(0);
                                        int length = BitConverter.ToInt32(byteListBuffer.ToArray(), 0);
                                        valueBuffer = length;
                                        stage++;
                                        incrementer = 0;
                                        break;
                                    }
                                    incrementer++;
                                    break;
                                case 1:
                                    byteList.Add(data);
                                    if (incrementer == valueBuffer - 1)
                                    {
                                        byteList.RemoveAt(0); // remove numbers and keep characters
                                        foreach (byte value in byteListBuffer)
                                        {
                                            byteList.Remove(value);
                                        }
                                        returnValue.Add(DeserialiseString(byteList.ToArray())); // deserialise and reset values
                                        deserialising = false;
                                        stage = 0;
                                        valueBuffer = 0;
                                        byteListBuffer = new List<byte>();
                                        byteList = new List<byte>();
                                        incrementer = 0;
                                        break;
                                    }
                                    incrementer++;
                                    break;
                            }
                            break;
                        case "ushort":
                            byteList.Add(data);
                            if (incrementer == 1)
                            {
                                returnValue.Add(DeserialiseUShort(byteList.ToArray()));
                                deserialising = false;
                                incrementer = 0;
                                byteList.Clear();
                                break;
                            }
                            incrementer++;
                            break;
                        case "int32":
                            byteList.Add(data);
                            if (incrementer == 3)
                            {
                                returnValue.Add(DeserialiseInt32(byteList.ToArray()));
                                deserialising = false;
                                incrementer = 0;
                                byteList.Clear();
                                break;
                            }
                            incrementer++;
                            break;
                        case "byte":
                            returnValue.Add(data);
                            deserialising = false;
                            break;
                        case "bool":
                            returnValue.Add(DeserialiseBool(data));
                            deserialising = false;
                            break;
                        case "float":
                            switch (stage)
                            {
                                case 0:
                                    byteListBuffer.Add(data);
                                    if (incrementer == 3)
                                    {
                                        int length = BitConverter.ToInt32(byteListBuffer.ToArray(), 0);
                                        valueBuffer = length;
                                        stage++;
                                        incrementer = 0;
                                        break;
                                    }
                                    incrementer++;
                                    break;
                                case 1:
                                    byteList.Add(data);
                                    if (incrementer == valueBuffer - 1)
                                    {
                                        returnValue.Add(DeserialiseFloat(byteList.ToArray())); // deserialise and reset values
                                        deserialising = false;
                                        stage = 0;
                                        valueBuffer = 0;
                                        byteListBuffer = new List<byte>();
                                        byteList = new List<byte>();
                                        incrementer = 0;
                                        break;
                                    }
                                    incrementer++;
                                    break;
                            }
                            break;
                    }
                }

                if (!deserialising)
                {
                    deserialising = true;
                    switch (data)
                    {
                        default:
                            deserialising = false;
                            break;
                        case 85:
                            deserialisationType = "character";
                            break;
                        case 170:
                            deserialisationType = "string";
                            break;
                        case 102:
                            deserialisationType = "ushort";
                            break;
                        case 153:
                            deserialisationType = "int32";
                            break;
                        case 204:
                            deserialisationType = "byte";
                            break;
                        case 51:
                            deserialisationType = "bool";
                            break;
                        case 195:
                            deserialisationType = "float";
                            break;
                    }
                }
            }

            return returnValue.ToArray();
        }

        public static char DeserialiseCharacter(byte value)
        {
            return (char)value;
        }

        public static string DeserialiseString(byte[] byteData)
        {
            string returnValue = "";

            foreach (byte value in byteData)
            {
                returnValue += DeserialiseCharacter(value);
            }

            return returnValue;
        }

        public static ushort DeserialiseUShort(byte[] byteData)
        {
            return BitConverter.ToUInt16(byteData, 0);
        }

        public static int DeserialiseInt32(byte[] byteData)
        {
            return BitConverter.ToInt32(byteData, 0);
        }

        public static bool DeserialiseBool(byte value)
        {
            return value == 0 ? false : true;
        }

        public static float DeserialiseFloat(byte[] byteData)
        {
            return BitConverter.ToSingle(byteData, 0);
        }

        //public static  Decode
    }
}
