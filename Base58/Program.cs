using System;
using System.Collections.Generic;
using System.Text;

namespace Base58
{
    class Program
    {
        static void Main(string[] args)
        {
            CRC32 crc = new CRC32();
           
            string mainText = "anonymous@nomail.empty";
            string mainEven = "aoyosnmi.mt";
            string mainOdd =   "nnmu@oalepy";
            byte[] mainData = System.Text.Encoding.UTF8.GetBytes(mainText);

            uint   crc32_uint = crc.CRC32Num(mainData);
            byte[] crc32_data = crc.CRC32Arr(mainData, true);
            ulong  crc32_ulng = crc32_uint;

            Console.WriteLine(mainText);
            Console.WriteLine();
            Console.WriteLine("CRC32  UINT: " + crc32_uint.ToString());
            Console.WriteLine("CRC32  IHEX: " + crc32_uint.ToString("X"));
            Console.WriteLine("CRC32  BYTE: " + BitConverter.ToString(crc32_data).Replace("-", string.Empty));
            Console.WriteLine();

            ulong  crc64_ulng = crc.CRC32mod2Num(mainData);
            byte[] crc64_data = crc.CRC32mod2Arr(mainData, true);
            
            Console.WriteLine("CRC64  ULNG: " + crc64_ulng.ToString());
            Console.WriteLine("CRC64  UHEX: " + crc64_ulng.ToString("X"));
            Console.WriteLine("CRC32  EVEN: " + BitConverter.ToString(crc.CRC32Arr(System.Text.Encoding.UTF8.GetBytes(mainEven), true)).Replace("-", "  "));
            Console.WriteLine("CRC32   ODD:   " + BitConverter.ToString(crc.CRC32Arr(System.Text.Encoding.UTF8.GetBytes(mainOdd), true)).Replace("-", "  "));            
            Console.WriteLine();
            
            Console.WriteLine("CRC64  BYTE: " + BitConverter.ToString(crc64_data).Replace("-", string.Empty));
            Console.WriteLine("       B58A: " + Base58.GetString(crc64_data));
            Console.WriteLine("       B58N: " + Base58.GetString(crc64_ulng));
            Console.WriteLine();
            Console.WriteLine("A -> ABCDEF");
            Console.WriteLine();
            Console.WriteLine("       B58 : " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes("A")));
            Console.WriteLine("       B58 : " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes("AB")));
            Console.WriteLine("       B58 : " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes("ABC")));
            Console.WriteLine("       B58 : " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes("ABCD")));
            Console.WriteLine("       B58 : " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes("ABCDE")));
            Console.WriteLine("       B58 : " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes("ABCDEF")));
            Console.WriteLine();

            Console.WriteLine(mainText + " [Base58]> " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes(mainText)) + " [B58(CRC32mod2]> " + Base58.GetString((new CRC32()).CRC32mod2Num(System.Text.Encoding.UTF8.GetBytes(mainText))));
            mainText = "Hell World";
            Console.WriteLine(mainText + " [Base58]> " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes(mainText)) + " [B58(CRC32mod2]> " + Base58.GetString((new CRC32()).CRC32mod2Num(System.Text.Encoding.UTF8.GetBytes(mainText))));
            mainText = "IDDQD";
            Console.WriteLine(mainText + " [Base58]> " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes(mainText)) + " [B58(CRC32mod2]> " + Base58.GetString((new CRC32()).CRC32mod2Num(System.Text.Encoding.UTF8.GetBytes(mainText))));
            mainText = "CRC32-Mod-2+Base64";
            Console.WriteLine(mainText + " [Base58]> " + Base58.GetString(System.Text.Encoding.ASCII.GetBytes(mainText)) + " [B58(CRC32mod2]> " + Base58.GetString((new CRC32()).CRC32mod2Num(System.Text.Encoding.UTF8.GetBytes(mainText))));
  

            Console.ReadLine();
        }       

        public class Base58
        {
            //
            //  Max 8 bytes (CRC64 or 2xCRC32)
            //  https://ru.wikipedia.org/wiki/Base58
            //  Check: https://incoherency.co.uk/base58/
            //  Check: https://www.browserling.com/tools/base58-encode 
            //
            public static string GetString(byte[] data)
            {
                string digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

                ulong numberValue = 0;
                for (int i = 0; i < data.Length; i++)
                    numberValue = numberValue * 256 + data[i];

                string result = "";
                while (numberValue > 0)
                {
                    int remainder = (int)(numberValue % 58);
                    numberValue /= 58;
                    result = digits[remainder] + result;
                };

                // Append `1` for each leading 0 byte
                for (int i = 0; i < data.Length && data[i] == 0; i++)
                    result = '1' + result;

                return result;
            }

            //
            //  Max 8 bytes (CRC64 or 2xCRC32)
            //  https://ru.wikipedia.org/wiki/Base58
            //  Check: https://incoherency.co.uk/base58/
            //  Check: https://www.browserling.com/tools/base58-encode
            //
            public static string GetString(ulong numberValue)
            {
                string digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";                

                ulong original = numberValue;
                string result = "";
                while (numberValue > 0)
                {
                    int remainder = (int)(numberValue % 58);
                    numberValue /= 58;
                    result = digits[remainder] + result;
                };

                // Append `1` for each leading 0 byte
                for (int i = 0; i < 8 && (((byte)(original >> 56 - 8 * i) & 0xFF) == 0); i++)
                    result = '1' + result;

                return result;
            }     
        }          

        public class CRC32
        {
            private const uint poly = 0xEDB88320;
            private uint[] checksumTable;            

            public CRC32()
            {
                checksumTable = new uint[256];
                for (uint index = 0; index < 256; index++)
                {
                    uint el = index;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if ((el & 1) != 0)
                            el = (poly ^ (el >> 1));
                        else
                            el = (el >> 1);
                    };
                    checksumTable[index] = el;
                };
            }

            public uint CRC32Num(byte[] data)
            {
                uint res = 0xFFFFFFFF;
                for (int i = 0; i < data.Length; i++)
                    res = checksumTable[(res & 0xFF) ^ (byte)data[i]] ^ (res >> 8);
                return ~res;
            }

            public byte[] CRC32Arr(byte[] data, bool isLittleEndian)
            {
                uint res = CRC32Num(data);
                byte[] hash = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    if (isLittleEndian)
                        hash[i] = (byte)((res >> (24 - i * 8)) & 0xFF);
                    else
                        hash[i] = (byte)((res >> (i * 8)) & 0xFF);
                };
                return hash;
            }

            public ulong CRC32mod2Num(byte[] data)
            {
                uint res1 = 0xFFFFFFFF;
                uint res2 = 0xFFFFFFFF;

                for (int i = 0; i < data.Length; i++)
                {
                    if (i % 2 == 0)
                        res1 = checksumTable[(res1 & 0xFF) ^ (byte)data[i]] ^ (res1 >> 8);
                    else
                        res2 = checksumTable[(res2 & 0xFF) ^ (byte)data[i]] ^ (res2 >> 8);
                };

                res1 = ~res1;
                res2 = ~res2;                

                ulong res = 0;
                for (int i = 0; i < 4; i++)
                {
                    ulong u1 = ((res1 >> (24 - i * 8)) & 0xFF);
                    ulong u2 = ((res2 >> (24 - i * 8)) & 0xFF);
                    res += u1 << (56 - i * 16);
                    res += u2 << (56 - i * 16 - 8);
                };

                return res;
            }

            public byte[] CRC32mod2Arr(byte[] data, bool isLittleEndian)
            {
                uint res1 = 0xFFFFFFFF;
                uint res2 = 0xFFFFFFFF;

                for (int i = 0; i < data.Length; i++)
                {
                    if(i % 2 == 0)
                        res1 = checksumTable[(res1 & 0xFF) ^ (byte)data[i]] ^ (res1 >> 8);
                    else
                        res2 = checksumTable[(res2 & 0xFF) ^ (byte)data[i]] ^ (res2 >> 8);
                };

                res1 = ~res1;
                res2 = ~res2;
                
                byte[] hash = new byte[8];
                for (int i = 0; i < 4; i++)
                {
                    if (isLittleEndian)
                    {
                        hash[i * 2] = (byte)((res1 >> (24 - i * 8)) & 0xFF);
                        hash[i * 2 + 1] = (byte)((res2 >> (24 - i * 8)) & 0xFF);
                    }
                    else
                    {
                        hash[7 - i * 2] = (byte)((res1 >> (24 - i * 8)) & 0xFF);
                        hash[7 - i * 2 - 1] = (byte)((res2 >> (24 - i * 8)) & 0xFF);
                    };
                };
                return hash;
            }
        }      
    }
}
