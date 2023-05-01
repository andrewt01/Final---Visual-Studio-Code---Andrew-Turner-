using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSHW4_UR1
{
    public class robot
    {
        public const byte STOP = 0x7F;
        public const byte FLOAT = 0x0F;
        public const byte FORWARD = 0x6f;
        public const byte BACKWARD = 0x5F;
        SerialPort _serialPort;
        public bool Online { get; private set; }

        public robot() { }

        public robot(String port)
        {
            SetupSerialComms(port);
        }

        public void SetupSerialComms(String port)
        {
            try
            {
                _serialPort = new SerialPort(port);
                _serialPort.BaudRate = 9600; //set to 9600 -- rate of bits transfered 
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Open();
                Online = true;
            }
            catch
            {
                Online = false;
            }
        }

        //moving serial data "down the tube" --> sending char to correspond to motor direction 
        public void Move(char serialCommand)
        {
            try
            {
                if (Online)
                {
                    byte[] buffer = { Convert.ToByte(serialCommand)};
                    _serialPort.Write(buffer, 0, 1); //set to 1 to send 1 byte at a time 
                }
            }
            catch
            {
                Online = false;
            }
        }

        public void Close()
        {
            _serialPort.Close();
        }

    }
}

