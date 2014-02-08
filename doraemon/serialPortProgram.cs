//------------------------------------------------------------------------------
// <copyright file="speechRecognizer.cs" organisation="NebulaX">
//     Copyright (c) NebulaX.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


using System;
using System.IO;
using System.IO.Ports;
// using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doraemon
{
    class serialPortProgram
    {
        static string comNo = "COM1";
        static private SerialPort port = new SerialPort(comNo, 9600, Parity.None, 8, StopBits.One);

        static bool _continue;
        static void Main(string[] args)
        {
            // Instatiate this class 


            _continue = true;
            // Enter an application loop to keep this thread alive

            port.Open();

            port_SendData();
            Console.WriteLine("Incoming Data:"); // Attach a method to be called when there
            //while (_continue)
            //{
            //    new serialPortProgram();
            //}
        }
        /*
        private serialPortProgram() 
        {
                // akshesh : shall we use "=" instead of "+="               
                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived); // Begin communications 
              
                //Thread.Sleep(1000);             

                // port1.DataReceived += new SerialDataReceivedEventHandler(port1_DataReceived); // Begin communications 
                      
        }
        */

        private void port_SendData(object sender, EventArgs e)
        {
            if (port.IsOpen)
            {
                port.Write(DataOut.Text + "\n\r");   // akshesh : send DataOut from main program..it will have values like f,r,l,b (forward, right,...)
            }
        }
        /*
        private void port_DataReceived(object sender,
                                    SerialDataReceivedEventArgs e) 
        {
            byte[] b = new byte[20];
            
            int i;

            
            //string msg;
            // Show all the incoming data in the port's buffer 
            for (i = 0; i < 20; i++)
            {
                b[ i ] = (byte)port.ReadByte();

                if (b[i] == 0x0D)                   // akshesh : check if this is required. I dont know what it is..
                {
                    port.Write(b, 0, i+1);
                    break;
                    
                }
                Console.Write(b[i]);
            }
            Console.WriteLine("Port finished");            
        }*/


        /*private void port1_DataReceived(object sender,
                                    SerialDataReceivedEventArgs e)
        {
            byte[] b1 = new byte[20];

            int i;

            string msg;
            // Show all the incoming data in the port's buffer 
            for (i = 0; i < 20; i++)
            {
                b1[i] = (byte)port1.ReadByte();

                if (b1[i] == 0xFD)
                {
                    port.Write(b1, 0, i+1);                    
                    break;
                }
                Console.Write(b1[i]);
               
            }

            
            Console.WriteLine("Port1 finished");
            
        } */
    }
}
