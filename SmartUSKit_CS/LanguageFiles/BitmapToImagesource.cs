using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SmartUSKit_CS.LanguageFiles
{
    public class BitmapToImagesource
    {
        public static BitmapSource ChangeBitmapToBitmapSource(Bitmap bmp)
        {
            BitmapSource returnSource;
            try
            {
                IntPtr ptr = bmp.GetHbitmap();
                returnSource = Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Converters.USImageConverter.DeleteObject(ptr);
  
            }
            catch
            {
                returnSource = null;
            }
            return returnSource;
        }
    }
}
