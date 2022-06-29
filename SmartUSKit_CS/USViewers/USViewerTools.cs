using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartUSKit_CS
{
   

    class BMMoveImage
    {
        public struct CurrentImage
        {
            public int ImageIndex;
            public DateTime Time;
        }
        private List<int> imageIndex = new List<int>();
        private List<DateTime> imageTime = new List<DateTime>();
        public CurrentImage GetCurrentImage(int index)
        {
            CurrentImage currentImage = new CurrentImage();
            currentImage.ImageIndex = imageIndex[index];
            currentImage.Time = imageTime[index];
            return currentImage;
        }
        public void Add(int index, DateTime currentTime)
        {
            if (imageIndex.Count > 100)
            {
                imageIndex.RemoveAt(0);
                imageIndex.Add(index);
            }
            else
            {
                imageIndex.Add(index);
            }
            if (imageTime.Count > 100)
            {
                imageTime.RemoveAt(0);
                imageTime.Add(currentTime);
            }
            else
            {
                imageTime.Add(currentTime);
            }
        }
        public void Clear()
        {
            imageIndex.Clear();
            imageTime.Clear();
        }
    }
}
