using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartUSKit_CS.Model
{
    public class Preset : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static Preset instance=null;

        public static Preset GetInstance()
        {
            if (instance == null)
            {
                instance = new Preset();
            }
            return instance;
        }

        private Preset()
        {

        }

        private int imageMaxAmount = 100;
        public int ImageMaxAmount
        {
            get
            {
                return this.imageMaxAmount;
            }

            set
            {
                this.imageMaxAmount =value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ImageMaxAmount"));
                }
            }
        }

        private Visibility patientVisible = Visibility.Visible;
        public Visibility PatientVisible
        {
            get
            {
                return this.patientVisible;
            }

            set
            {
                this.patientVisible = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("PatientVisible"));
                }
            }
        }

        private Visibility infoVisible = Visibility.Visible;
        public Visibility InfoVisible
        {
            get
            {
                return this.infoVisible;
            }

            set
            {
                this.infoVisible = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InfoVisible"));
                }
            }
        }

        private double  stackpanelOpacity = 1;
        public double StackpanelOpacity
        {
            get
            {
                return this.stackpanelOpacity;
            }

            set
            {
                this.stackpanelOpacity = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StackpanelOpacity"));
                }
            }
        }
    }
}
