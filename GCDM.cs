using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;

namespace GCDM
{
    public class GCDM
    {
        const int SEND_PACKET_LENGTH = 21;
        const int SEND_CMD_LENGTH = 1;

        public class CMD
        {
            public const byte STX = 0x02;
            public const byte ETX = 0x03;
            public const byte LI = 0x10;
            public const byte DEVICE_ID = 0x81;
            public const byte DISPENSE = 0x44;
            public const byte CARD_1 = 0x30;
            public const byte CARD_MORE = 0x31;
            public const byte BCC = 0x67;

            public const byte ACK = 0x06;
            public const byte NAK = 0x15;
            public const byte ENQ = 0x05;
        }

        class POS
        {
            public const int STX = 0;
            public const int LI = 1;
            public const int DEVICE_ID = 3;
            public const int MI = 4;
            public const int CARD_COUNT = 5;
            public const int PARAM1 = 6;
            public const int ETX = 19;
            public const int BCC = 20;

        }
        const byte DEV_ID = 0x81;

        private SerialPort serialPort;

        public GCDM(string comPortName, int speed)
        {
            serialPort = new SerialPort();
            serialPort.PortName = comPortName;
            serialPort.BaudRate = speed;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;

            serialPort.Open();
        }

        public bool dispense()
        {
            if (!serialPort.IsOpen)
            {
                serialPort.Open();
            }

            byte[] sendData = new byte[SEND_PACKET_LENGTH];
            sendData[POS.STX] = CMD.STX;
            sendData[POS.LI] = CMD.LI;
            sendData[POS.DEVICE_ID] = CMD.DEVICE_ID;
            sendData[POS.MI] = CMD.DISPENSE;
            sendData[POS.CARD_COUNT] = CMD.CARD_1;
            sendData[POS.PARAM1] = CMD.DEVICE_ID;
            sendData[POS.ETX] = CMD.ETX;
            sendData[POS.BCC] = CMD.BCC;

            serialPort.Write(sendData, 0, SEND_PACKET_LENGTH);
            Thread.Sleep(50);

            int response = serialPort.ReadByte();
            switch (response)
            {
                case CMD.ACK:
                    break;
                case CMD.NAK:
                default:
                    return false;
            }

            sendData = new byte[SEND_CMD_LENGTH];
            sendData[0] = CMD.ENQ;
            serialPort.Write(sendData, 0, SEND_CMD_LENGTH);
            Thread.Sleep(50);

            response = serialPort.ReadByte();
            switch (response)
            {
                case CMD.ACK:
                    break;
                case CMD.NAK:
                default:
                    return false;
            }

            serialPort.Write(sendData, 0, SEND_CMD_LENGTH);
            Thread.Sleep(50);

            // 장비 ID CHECK

            sendData[0] = CMD.ACK;
            serialPort.Write(sendData, 0, SEND_CMD_LENGTH);

            return true;
        }

        public string[] getSerialPort()
        {
            return SerialPort.GetPortNames();
        }

        public void close()
        {
            serialPort.Close();
        }
    }
}
