using CyUSB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SmartUSKit.SmartUSKit
{
    public class USUsbManager:IDisposable
    {
        private Dispatcher theContext;
        private static USUsbManager instance;
        private USBDeviceList usbDevices;

        private ConnectedUsbDevice theConnectUsbDevice;

        private List<UsbID> usbIDList = new List<UsbID>();

        private String theUsbKey;

        private bool enableConnectZeroDevice = true;

        private USUsbManager(Dispatcher context)
        {
            theContext = context;
            scanUsbDevice();
        }

        public static USUsbManager getInstance(Dispatcher context)
        {
            if (instance == null)
            {
                instance = new USUsbManager(context);
            }
            return instance;
        }

        //扫描USB设备
        public int scanUsbDevice()
        {
            theContext.Invoke(()=>
            { 
                usbDevices?.Dispose();
                usbDevices = null;
                if (usbDevices==null)
                {
                    usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
                }
            });
            int count = 0;
            if (usbDevices!=null)
            {
                count = usbDevices.Count;
            }
            return count;
        }
        //连接USB设备
        //private PendingIntent pendingIntent = null;
        public bool connectUsbDevice(String usbKey)
        {
            if (usbKey == null)
            {
                return false;
            }

            foreach (CyUSBDevice key in usbDevices)
            {
                if (key.Name.Contains(usbKey)
                    ||key.ProductID==0xbcff)
                {
                    theUsbKey = usbKey;
                     
                    if (theConnectUsbDevice!=null)
                    {
                        theConnectUsbDevice.Dispose();
                        theConnectUsbDevice=null;
                    }

                    theConnectUsbDevice = new ConnectedUsbDevice();
                    theConnectUsbDevice.usbKey = key.Name;
                    theConnectUsbDevice.device = key;
                    return true;
                }
            }
            return false;
        }
        
        //断开USB连接
        public void disConnectUsbDevice()
        {
            theConnectUsbDevice?.Dispose();
            theConnectUsbDevice = null;
            theUsbKey = null;
        }
        public ConnectedUsbDevice getUsbConnection(string usbkey)
        {
            theUsbKey = usbkey;
            scanUsbDevice();//theUsbKey = "Ultrasound UL-1C";
            
            if (usbDevices.Count < 1)
            {
                theConnectUsbDevice?.Dispose();
                theConnectUsbDevice = null;
            }
            else
            {
                if (theConnectUsbDevice == null)
                {
                    bool contains = false;
                    foreach (USBDevice item in usbDevices)
                    {
                        if (item.Name.Contains(theUsbKey)
                            ||item.ProductID==0xbcff)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (contains)
                    {
                        connectUsbDevice(theUsbKey);
                    }
                    else
                    {
                        if (enableConnectZeroDevice)
                        {
                        }
                    }
                }
            }
            return theConnectUsbDevice;
        }
       
        public void Dispose()
        {
            if (usbDevices!=null)
            {
                usbDevices.Dispose();
                usbDevices = null;
            }
            if (theConnectUsbDevice!=null)
            {
                theConnectUsbDevice.Dispose();
                theConnectUsbDevice = null;
            }
        }

    }
    public class ConnectedUsbDevice : IDisposable
    {
        public CyUSBDevice device;
        public String usbKey;

        public void Dispose()
        {
            try
            {
                device?.Dispose();
            }
            catch (Exception eee)
            {
                Debug.WriteLine(eee.ToString());
            }
            
        }
    }

    public class UsbID
    {
        public int vid;
        public int pid;
    }

}
