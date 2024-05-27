using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QWindowToHttp.WindowTools
{
    public class GdiTool
    {
        public static MemoryStream GetScreenImageMemoryJpeg()
        {
            var size=Computer.ScreenSize;
            Bitmap bmp = new Bitmap(size.Width,size.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0,0,0,0,size);
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms,System.Drawing.Imaging.ImageFormat.Jpeg);
            g.Dispose();
            bmp.Dispose();
            return ms;
        }
    }
}
