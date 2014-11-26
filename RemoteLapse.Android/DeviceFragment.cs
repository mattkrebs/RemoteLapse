using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace RemoteLapse.Droid
{
    public class DeviceFragment : DialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.DeviceDialogFragment, container, false);
        //    var deviceList = view.FindViewById<ListView>(Resource.Id.deviceList);

            
        //// Set up a handler to dismiss this DialogFragment when this button is clicked.
        //    view.FindViewById<Button>(Resource.Id.cancel).Click += (sender, args) => Dismiss();
            return view;
        
        }

       
    }
}