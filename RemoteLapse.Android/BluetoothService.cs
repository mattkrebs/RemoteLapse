using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using Java.Util;
using System.Diagnostics;
using RemoteLapse.Data;
using System.IO;

namespace RemoteLapse.Droid
{
    enum BluetoothStatus{
        Disconnected,
        Connected,
        Connecting,
        Listening
    }
    enum BluetoothRequestType
    {
        REQUEST_CONNECTED_DEVICE = 1,
        REQUEST_ENABLE_BT = 2
    }
    public class BluetoothManager
    {
        private BluetoothAdapter mBluetoothAdapter = null;
        private BluetoothSocket btSocket = null;
        private BluetoothStatus btStatus = BluetoothStatus.Disconnected;

        private Stream outStream = null;
        private Stream inStream = null;
        //private static string address = "98:D3:31:80:1B:39";
        //Id Unico de comunicacion
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        public BluetoothManager()
        {
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (!mBluetoothAdapter.Enable())
            {
                //failed to connect
            }
            if (mBluetoothAdapter == null)
            {
                //Bluetooth Does Not Exist or is Busy
            }
        }


        private static BluetoothManager instance;
        public static BluetoothManager Instance
       {
          get 
          {
             if (instance == null)
             {
                 instance = new BluetoothManager();
             }
             return instance;
          }
       }


        public bool IsEnabled
        {
            get
            {
                return mBluetoothAdapter.IsEnabled;
            }
        }
       

        public void Connect(string address)
        {
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
            mBluetoothAdapter.CancelDiscovery();

            try
            {
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                btSocket.Connect();
                btStatus = BluetoothStatus.Connected;
                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                try
                {
                    btSocket.Close();
                }
                catch (System.Exception)
                {
                    System.Console.WriteLine("Unable to Connect");
                }
                System.Console.WriteLine("Socket Closed");
                
            }
        }


        public void UploadSettings()
        {

            try 
	        {	        
		        outStream = btSocket.OutputStream;
	        }
	        catch (Exception ex)
	        {
		        System.Console.WriteLine("Error " + ex.Message);
		        throw;
	        }


            Java.Lang.String message = new Java.Lang.String(LapseSettings.Instance.SettingsString()); 

            byte[] msgBuffer = message.GetBytes();

            try
            {
                //Escribimos en el buffer el arreglo que acabamos de generar
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Error" + e.Message);
            }
        }
    }

    
}