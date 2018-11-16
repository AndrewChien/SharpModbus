using System;

namespace DTU_PLC_Test
{
    /// <summary>
    /// AndrewChien - lyo
    /// 2018-11-06 13:12:49
    /// </summary>
    public static class ModBus
    {
        public enum ModBusFunction
        {
            ReadCoils = 0x01,
            ReadInputs = 0x02,
            ReadHoldingRegisters = 0x03,
            ReadInputRegisters = 0x04,
            WriteCoil = 0x05,
            WriteRegister = 0x06,
            Checking = 0x08,
            WriteCoils = 0x15,
            WriteRegisters = 0x16
        }

        /// <summary>
        /// 写
        /// </summary>
        /// <param name="slaveid"></param>
        /// <param name="function"></param>
        /// <param name="addr"></param>
        /// <param name="coildata"></param>
        /// <returns></returns>
        public static byte[] ModBusWrite(int slaveid, ModBusFunction function, int addr, bool coildata)
        {
            var modbus = new byte[8];
            modbus[0] = (byte)slaveid;//从站：06
            modbus[1] = (byte)function;//功能码：05
            modbus[2] = (byte)(addr >> 8);//写入的起始地址
            modbus[3] = (byte)addr;
            modbus[4] = coildata ? (byte)0xFF : (byte)0x00;//写入的值：状态ON：FF 00（本函数只用于开闭状态量）
            modbus[5] = 0x00;
            modbus[6] = CrcChecking(modbus, 0, 6)[0];
            modbus[7] = CrcChecking(modbus, 0, 6)[1];
            var a = HexByteToHexStr(modbus);
            return modbus;
        }

        /// <summary>
        /// 读
        /// </summary>
        /// <param name="slaveid"></param>
        /// <param name="function"></param>
        /// <param name="addr"></param>
        /// <param name="readnum"></param>
        /// <returns></returns>
        public static byte[] ModBusRead(int slaveid, ModBusFunction function, int addr, int readnum)
        {
            var modbus = new byte[8];
            modbus[0] = (byte)slaveid;//从站：06
            modbus[1] = (byte)function;//功能码：05
            modbus[2] = (byte)(addr >> 8);//读取的起始地址
            modbus[3] = (byte)addr;
            modbus[4] = (byte)(readnum >> 8);//读取的个数
            modbus[5] = (byte)readnum;
            modbus[6] = CrcChecking(modbus, 0, 6)[0];
            modbus[7] = CrcChecking(modbus, 0, 6)[1];
            var a = HexByteToHexStr(modbus);
            return modbus;
        }

        private static byte[] CrcChecking(byte[] instructions, uint start, uint length)
        {
            uint i, j;
            uint crc16 = 0xFFFF;
            try
            {
                length = length + start;
                for (i = start; i < length; i++)
                {
                    crc16 ^= instructions[i];
                    for (j = 0; j < 8; j++)
                    {
                        if ((crc16 & 0x01) == 1)
                        {
                            crc16 = (crc16 >> 1) ^ 0xA001;
                        }
                        else
                        {
                            crc16 = crc16 >> 1;
                        }
                    }
                }
                //   UInt16 X = (UInt16)(crc16 *256);
                // UInt16 Y = (UInt16)(crc16/256);
                //crc16 = (UInt16)(X ^ Y);
            }
            catch
            {
            }
            return BitConverter.GetBytes(crc16);
        }

        public static byte[] HexStrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static string HexByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
    }
}
