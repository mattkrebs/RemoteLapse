using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Java.Lang;
using Android.Util;


namespace RemoteLapse.Droid
{
    [Activity(Label = "Remote Lapse", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : Activity
    {
       
        // Member object for the chat services
       
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);


            this.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            AddTab("Control", Android.Resource.Drawable.IcMediaPlay, new ControlFragment());
            AddTab("Length", Android.Resource.Drawable.IcMenuGallery, new LengthFragment());
            AddTab("Duration", Android.Resource.Drawable.IcMenuGallery, new DurationFragment());
            AddTab("Settings", Android.Resource.Drawable.IcMenuGallery, new PreferenceFragment());



            
           
        }
        protected override void OnStart()
        {
            base.OnStart();
           
            // If BT is not on, request that it be enabled.
            // setupChat() will then be called during onActivityResult
            if (!BluetoothManager.Instance.IsEnabled)
            {
                Intent enableIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableIntent, (int)BluetoothRequestType.REQUEST_ENABLE_BT);
                // Otherwise, setup the chat session
            }
            else
            {
                //Set up communications
            }
        }
   
     
        public void AddTab(string tabText, int iconResourceId, Fragment view)
        {
            var tab = this.ActionBar.NewTab();
            tab.SetText(tabText);
            //tab.SetIcon(iconResourceId);

            tab.TabSelected += delegate(object sender, ActionBar.TabEventArgs e)
            {
                var fragment = this.FragmentManager.FindFragmentById(Resource.Id.fragmentContainer);
                if (fragment != null)
                    e.FragmentTransaction.Remove(fragment);
                e.FragmentTransaction.Add(Resource.Id.fragmentContainer, view);
            };
            tab.TabUnselected += delegate(object sender, ActionBar.TabEventArgs e)
            {
                e.FragmentTransaction.Remove(view);
            };
            this.ActionBar.AddTab(tab);
        }


        // The Handler that gets information back from the BluetoothChatService
        //private class MyHandler : Handler
        //{
        //    MainActivity bluetoothChat;

        //    public MyHandler(MainActivity chat)
        //    {
        //        bluetoothChat = chat;
        //    }

        //    public override void HandleMessage(Message msg)
        //    {
        //        switch (msg.What)
        //        {
        //            case (int)BluetoothType.MESSAGE_STATE_CHANGE:
        //                if (Debug)
        //                    Log.Info(TAG, "MESSAGE_STATE_CHANGE: " + msg.Arg1);
        //                switch (msg.Arg1)
        //                {
        //                    case (int)BluetoothConnectionState.STATE_CONNECTED:
        //                        //bluetoothChat.title.SetText("Connected To ");
        //                        //bluetoothChat.title.Append(bluetoothChat.connectedDeviceName);
        //                        bluetoothChat.conversationArrayAdapter.Clear();
        //                        break;
        //                    case (int)BluetoothConnectionState.STATE_CONNECTING:
        //                        // bluetoothChat.title.SetText("Connecting");
        //                        break;
        //                    case (int)BluetoothConnectionState.STATE_LISTEN:
        //                    case (int)BluetoothConnectionState.STATE_NONE:
        //                        //bluetoothChat.title.SetText("Not Connected");
        //                        break;
        //                }
        //                break;
        //            case (int)BluetoothType.MESSAGE_WRITE:
        //                byte[] writeBuf = (byte[])msg.Obj;
        //                // construct a string from the buffer
        //                var writeMessage = new Java.Lang.String(writeBuf);
        //                bluetoothChat.conversationArrayAdapter.Add("Me: " + writeMessage);
        //                break;
        //            case (int)BluetoothType.MESSAGE_READ:
        //                byte[] readBuf = (byte[])msg.Obj;
        //                // construct a string from the valid bytes in the buffer
        //                var readMessage = new Java.Lang.String(readBuf, 0, msg.Arg1);
        //                bluetoothChat.conversationArrayAdapter.Add(bluetoothChat.connectedDeviceName + ":  " + readMessage);
        //                break;
        //            case (int)BluetoothType.MESSAGE_DEVICE_NAME:
        //                // save the connected device's name
        //                bluetoothChat.connectedDeviceName = msg.Data.GetString(DEVICE_NAME);
        //                Toast.MakeText(Application.Context, "Connected to " + bluetoothChat.connectedDeviceName, ToastLength.Short).Show();
        //                break;
        //            case (int)BluetoothType.MESSAGE_TOAST:
        //                Toast.MakeText(Application.Context, msg.Data.GetString(TOAST), ToastLength.Short).Show();
        //                break;
        //        }
        //    }
        //}


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
           
            switch (requestCode)
            {
                case (int)BluetoothRequestType.REQUEST_CONNECTED_DEVICE:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Result.Ok)
                    {
                        // Get the device MAC address
                        var address = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                        // Get the BLuetoothDevice object
                        BluetoothManager.Instance.Connect(address);
                    }
                    break;
                case (int)BluetoothRequestType.REQUEST_ENABLE_BT:
                    // When the request to enable Bluetooth returns
                    if (resultCode == Result.Ok)
                    {
                        // Bluetooth is now enabled, so set up a chat session
                        //SetupChat();
                        Toast.MakeText(this, "Bluetooth is Enabled", ToastLength.Short).Show();
                    }
                    else
                    {
                        // User did not enable Bluetooth or an error occured
                        
                        Toast.MakeText(this,"Bluetooth not Enabled" , ToastLength.Short).Show();
                        Finish();
                    }
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.options_menu, menu);

            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.scan:
                    // Launch the DeviceListActivity to see devices and do scan
                    var serverIntent = new Intent(this, typeof(DeviceListActivity));
                    StartActivityForResult(serverIntent, (int)BluetoothRequestType.REQUEST_CONNECTED_DEVICE);
                    return true;
                case Resource.Id.upload:
                    BluetoothManager.Instance.UploadSettings();
                    return true;
            }
            return false;
        }
    }
}

