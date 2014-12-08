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
using RemoteLapse.Data;

namespace RemoteLapse.Droid
{
    public class PreferenceFragment : Fragment
    {
        TextView txtRailLength;
        TextView txtRPMs;
        Button btnSave;
        int length = 1500;
        int rpms = 35;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);



            var view = inflater.Inflate(Resource.Layout.PreferenceFragment, container, false);

            txtRailLength = view.FindViewById<TextView>(Resource.Id.txtRailLength);
            txtRailLength.Text = rpms.ToString();
           
            txtRPMs = view.FindViewById<TextView>(Resource.Id.txtRPMs);
            txtRPMs.Text = length.ToString();
            btnSave = view.FindViewById<Button>(Resource.Id.btnSave);
            return view;
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtRPMs.Text, out rpms))
                LapseSettings.Instance.RPMs = rpms;

            if (int.TryParse(txtRailLength.Text, out length))
                LapseSettings.Instance.RailLength = length;
        }
    }
}