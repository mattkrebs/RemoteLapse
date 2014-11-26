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
    public class FPSFragment : Fragment, View.IOnTouchListener
    {
        int sensitivity = 5;
        TextView txtSetting;
        float _lastX;
        float _lastY;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
           base.OnCreateView(inflater, container, savedInstanceState);
           
            var view = inflater.Inflate(Resource.Layout.SettingFragment, container, false);

           txtSetting = view.FindViewById<TextView>(Resource.Id.txtSetting);
           UpdateSetting();

           view.SetOnTouchListener(this);

           return view;
        }

        public void UpdateSetting()
        {
            txtSetting.Text = String.Format("{0} fps", LapseSettings.Instance.FPS);
        }

        public bool OnTouch(View v, MotionEvent ev)
        {
            float y = ev.GetY();
            switch (ev.Action)
            {
                case MotionEventActions.Down:
                    _lastY = ev.GetY();
                    break;
                case MotionEventActions.Move:
                    if (y - sensitivity > _lastY)
                    {
                        _lastY = y;
                        LapseSettings.Instance.DecreaseFPS();
                        UpdateSetting();
                        Log.Debug("TOUCH POINT","Going Down");                        
                    }
                    else if (y + sensitivity < _lastY)
                    {
                        _lastY = y;
                        LapseSettings.Instance.IncreaseFPS();
                        UpdateSetting();
                        Log.Debug("TOUCH POINT", "Going up");
                    }
                    break;
            }
            return true;
        }

     
    }
}