using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartUSKit_CS.Model
{
    public class Patient : INotifyPropertyChanged
    {
        private static Patient instance = null;
        public static Patient GetInstance()
        {
            if (instance==null)
            {
                instance = new Patient();
            }
            return instance;
        }
        private Patient()
        {

        }
        public event PropertyChangedEventHandler PropertyChanged;

        private string onlyName="";

        /// <summary>
        /// 仅显示姓名
        /// </summary>
        public string OnlyName
        {
            get { return onlyName; }
            //set { onlyName = value; }
        }


        private string name = Properties.Resources.Name;
        /// <summary>
        /// 带有前缀的姓名，用来绑定界面显示
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = Properties.Resources.Name  + value;
                onlyName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }

        private string onlyUserID= "";

        public string OnlyUserID
        {
            get { return onlyUserID; }
            //set { onlyUserID = value; }
        }


        private string userId = Properties.Resources.ID;
        /// <summary>
        /// 编号
        /// </summary>
        public string UserId
        {
            get
            {
                return this.userId;
            }

            set
            {
                this.userId = Properties.Resources.ID  + value;
                onlyUserID = value;
                
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("UserId"));
                }
            }
        }

        private string onlyGender="";

        public string OnlyGender
        {
            get { return onlyGender; }
            //set { onlyGender = value; }
        }


        private string gender = Properties.Resources.Sex;
        /// <summary>
        /// 0:女；1：男；2：其他
        /// </summary>
        public string Gender
        {
            get
            {
                return this.gender;
            }

            set
            {
                this.gender = Properties.Resources.Sex+  value;
                onlyGender = value;
                
            }
        }

        private string onlyAge="";

        public string OnlyAge
        {
            get { return onlyAge; }
            //set { onlyAge = value; }
        }


        private string age = Properties.Resources.Age;
        public string Age
        {
            get
            {
                return this.age;
            }

            set
            {
                this.age = Properties.Resources.Age + value;
                onlyAge = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }

        private string birthday = "";
        public string Birthday
        {
            get
            {
                return this.birthday;
            }

            set
            {
                this.birthday = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Birthday"));
                }
            }
        }
    }
}
