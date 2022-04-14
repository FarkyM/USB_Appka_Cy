using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using CyUSB;

namespace USB_Appka_Cy
{

    public partial class Form1 : Form
    {
        USBDeviceList usbDevices;
        CyUSBDevice myDevice;
        public static CyBulkEndPoint myBulkIn = null;

        // string FilePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"log.txt");
        public static DateTime t1, t2;
        public static TimeSpan elapsed;
        public static double XferBytes;
        public static long xferRate;
        public static ConcurrentQueue<byte[][]> DataQueue = new ConcurrentQueue<byte[][]>();
        public static Thread tListen;
        public static bool bRunning;

        public static Thread tFileWrite;
        AutoResetEvent ThreadEvent = new AutoResetEvent(false);


        public static int PaketsPerXfer;//udelat do boxiku?
        public static int XfersToQueue = 16;
        public static int BufSz;
        public static int Successes =0;
        public static int Failures =0;

        // These are needed to close the app from the Thread exception(exception handling)
        delegate void ExceptionCallback();
        ExceptionCallback handleException;

        // These are  needed for Thread to update the UI
        delegate void UpdateUICallback();
        UpdateUICallback updateUI;

        public Form1()
        {
            InitializeComponent();
            usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
            usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);
            SelectDevice();
 

            // Setup the callback routine for updating the UI
            updateUI = new UpdateUICallback(StatusUpdate);



        }


        void usbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            USBEventArgs usbEvent = e as USBEventArgs;
            lbl_Status.Text = usbEvent.FriendlyName + " removed.";
        }
        void usbDevices_DeviceAttached(object sender, EventArgs e)
        {
            USBEventArgs usbEvent = e as USBEventArgs;
            lbl_Status.Text = usbEvent.Device.FriendlyName + " connected.";
        }

        private void btn_Info_Click(object sender, EventArgs e)
        {
            if (myDevice.bSuperSpeed)
            {
                log.Text = "USB3.0 Device";
            }
            else
            {
                log.Text = "Nejsem USB3.0 device";
            }

        }

        private void fX3SPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //mozna do budoucna
        }

        private void bOSInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string usb3_BOS = myDevice.USBBos.ToString();
            log.Text = usb3_BOS;
        }

        private void btn_recive_Click(object sender, EventArgs e)
        {
            if (myDevice == null)
                return;

            /*  if (QueueBox.Text == "")
              {
                  MessageBox.Show("Please Select Xfers to Queue", "Invalid Input");
                  return;
              }*/

            btn_close.Enabled = true;
            PaketsPerXfer = 256; //udelat do boxiku?
            XfersToQueue = 16;
            BufSz = myBulkIn.MaxPktSize * PaketsPerXfer;


            myBulkIn.XferSize = BufSz;

            bRunning = true;
            FreeQueue(DataQueue);
            
            tListen = new Thread(ListenThread.XferThread);
            tListen.IsBackground = true;
            tListen.Priority = ThreadPriority.Highest;

            tFileWrite = new Thread(WriteToFile.StartWriteThread); 
           // tFileWrite = new Thread(StartWrite);
            tFileWrite.IsBackground = true;
            tFileWrite.Priority = ThreadPriority.AboveNormal;


            tFileWrite.Start(DataQueue);
            tListen.Start();          
            
        }

        private void FreeQueue(ConcurrentQueue<byte[][]> dataQueue)
        {
            while(dataQueue.Count > 0)
            dataQueue.TryDequeue(out var data); //throu out data
        }

        /*
* Veme si strukturu device infa a rozepise vlastnosti jednotlive node.
*/
        private void treeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceTreeView.Nodes.Clear();
            log.Text = "";
            foreach (USBDevice dev in usbDevices)
                DeviceTreeView.Nodes.Add(dev.Tree);
        }

        private void btn_close_Click(object sender, EventArgs e)
        {

            btn_close.Enabled = false;
            if (tListen.IsAlive)
            {

                bRunning = false;

                t2 = DateTime.Now;
                elapsed = t2 - t1;
                xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                xferRate = xferRate / (int)100 * (int)100;

                if (tListen.Join(5000) == false)
                    tListen.Abort();

                tListen = null;

                btn_close.Enabled = false;
            }
        /*    string FilePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "logy\\log.hex");
            FileStream fs = new FileStream(FilePath, FileMode.Create);
            using (BinaryWriter logfile = new BinaryWriter(fs))
            {
               while(DataQueue.Count > 0)
                {
                    DataQueue.TryDequeue(out byte[][] Data);
                    foreach (byte[] Paket in Data)
                    {
                        logfile.Write(Paket);
                    }
                }
            }
            */
        }

        private void DeviceTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selNode = DeviceTreeView.SelectedNode;
            log.Text = selNode.Tag.ToString();
        }

        /*Summary
          The callback routine delegated to updateUI.
        */
        public void StatusUpdate()
        {
            if (bRunning == false) return;

            lbl_Throughout.Text = xferRate.ToString();
            log.Text = DataQueue.Count.ToString();
        }

        public void SelectDevice()
        {
            // Get the first device having VendorID == 0x04B4 and ProductID == 0x00F1 -> Streamer example
            //myDevice = usbDevices[0x04B4, 0x00F1] as CyUSBDevice;
            myDevice = usbDevices[0] as CyUSBDevice; //první usb pouzivajici cyUSB3.sys -> dat pak napevno VID/PID
            if (myDevice != null)
            {
                lbl_Status.Text = myDevice.FriendlyName + " connected.";
                foreach (CyUSBEndPoint endpt in myDevice.EndPoints)
                {
                    if (endpt.bIn && (endpt.Attributes == 2)) //Attributes -> 2 je bulk
                        myBulkIn = endpt as CyBulkEndPoint;
                }


                if (myDevice.bSuperSpeed)
                {
                    lbl_led.Text = "USB 3.0 Compatible Device";
                    led_SS.BackColor = Color.PaleGreen;
                }
                else
                {
                    lbl_led.Text = "Device is not USB3 compatible";
                    led_SS.BackColor = Color.Red;
                }

            }
            else
            {
                lbl_Status.Text = "No FX3 device connected";
                btn_Info.Enabled = false;
                //Exit,Refresh nebo tak neco
            }

        }

    }
}
    
