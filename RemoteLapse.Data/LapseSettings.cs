using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteLapse.Data
{
    public class LapseSettings
    {
        //Limit in seconds
        private const int FPSLimit = 100;
        //limit in minutes
        private const int LengthLimit = 60;
        //limit in minutes
        private const int DurationLimit = 240;

        private static readonly LapseSettings instance = new LapseSettings();

        private LapseSettings() { }

        public static LapseSettings Instance
       {
          get 
          {
             return instance; 
          }
       }


        private int _fps = 30;
        private int _length = 4;
        private int _duration = 15;

        private int myVar;

        public int FPS
        {
            get { return _fps; }
            set { _fps = value; }
        }        
        public int Lenght
        {
            get { return _length; }
            set { _length = value; }
        }
        public int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
      


        public void IncreaseFPS()
        {
            if (FPS < FPSLimit)
                FPS++;
        }
        public void DecreaseFPS()
        {
            if (FPS > 1)
                FPS--;
        }

        public void IncreaseLenght()
        {
            if (Lenght < LengthLimit)
                Lenght++;
        }
        public void DecreaseLenght()
        {
            if (Lenght > 1)
                Lenght--;
        }

        public void IncreaseDuration()
        {
            if (Duration < DurationLimit)
                Duration++;
        }
        public void DecreaseDuration()
        {
            if (Duration > 1)
                Duration--;
        }


        // *START|PULSE|INTERVAL|FRAMES|DIRECTION#
        public string SettingsString()
        {
            


            // (FPS * Lenght
            var frames = (FPS * Lenght;

            var pulse = 0;
            var interval = Math.Abs(frames/ (Duration * 60));


             // *START|PULSE|INTERVAL|FRAMES|DIRECTION#
            return string.Format("*{0}|{1}|{2}|{3}|{4}#", 1, 300, 2000, 150, 0);
        }

    }
}
