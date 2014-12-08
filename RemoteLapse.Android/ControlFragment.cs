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
    public class ControlFragment : Fragment
    {
        TextView txtCount;
        ImageButton btnStart;
        ImageButton btnPause;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.ControlFragment, container, false);

            txtCount = view.FindViewById<TextView>(Resource.Id.txtCount);
            btnPause = view.FindViewById<ImageButton>(Resource.Id.btnPause);
            btnPause.Click += btnPause_Click;
            btnStart = view.FindViewById<ImageButton>(Resource.Id.btnStart);
            btnStart.Click += btnStart_Click;
          

            return view;
        }

        void btnStart_Click(object sender, EventArgs e)
        {
            BluetoothManager.Instance.UploadSettings();
            //start 
        }

        void btnPause_Click(object sender, EventArgs e)
        {
            //pause
        }

    }
}