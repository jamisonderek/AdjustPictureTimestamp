using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdjustPictureTimestamps
{
    class Program
    {
        static void Main(string[] args)
        {
            AdjustPictures.AdjustDate(@"C:\europe\April 13", "2015*.jpg", new TimeSpan(3, 0, 0), 4);
            AdjustPictures.AdjustDate(@"C:\europe\April 13", "April*.jpg", new TimeSpan(3, 0, 0), 4);
            AdjustPictures.AdjustDate(@"C:\europe\April 13", "DSCN*.jpg", new TimeSpan(3, 0, 0), 4);
            AdjustPictures.AdjustDate(@"C:\europe\April 13", "WP_*.jpg", new TimeSpan(3, 0, 0), 4);
        }
    }

    class AdjustPictures
    {
        public static void AdjustDate(string folder, string fileSpec, TimeSpan adjustment, int maxPics)
        {
            var files = Directory.GetFiles(folder, fileSpec).Take(maxPics);
            foreach(var file in files)
            {
                Picture.SetDateTaken(file, adjustment);
            }
        }
    }

    class Picture
    {
        //https://msdn.microsoft.com/en-us/library/system.drawing.imaging.propertyitem.id(v=vs.110).aspx
        private const int PropertyTagExifDTOrig = 0x9003;
        private const int PropertyTagDateTime = 0x132;
        private const int PropertyTagExifUserComment = 0x9286;

        public static void SetDateTaken(string path, TimeSpan adjustment)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var image = Image.FromStream(fs, true, false))
            {
                var propItem = image.GetPropertyItem(PropertyTagExifDTOrig);
                if (propItem != null && propItem.Value.Length > 0)
                {
                    var dateTaken = GetDate(propItem).Add(adjustment);
                    propItem.Value = GetDateValue(dateTaken);
                    image.SetPropertyItem(propItem);

                    var newPath = GetNewPath(path);   
                    image.Save(newPath);
                    File.SetCreationTime(newPath, dateTaken);
                }
            }
        }

        private static string GetNewPath(string path)
        {
            return Path.Combine(Path.GetDirectoryName(path), '_' + Path.GetFileName(path));
        }

        private static DateTime GetDate(PropertyItem propItem)
        {
            var data = propItem.Value.TakeWhile(b => b != 0).ToArray();
            string dateTaken = Encoding.UTF8.GetString(data);
            var date = dateTaken.Split(' ')[0].Replace(':', '-');
            var time = dateTaken.Split(' ')[1];
            return DateTime.Parse(date + ' ' + time);
        }

        private static byte[] GetDateValue(DateTime date)
        {
            var exifFormattedDate = date.ToString("yyyy:MM:dd H:mm:ss");
            var nullTerminator = new byte[] { 0 };
            return Encoding.UTF8.GetBytes(exifFormattedDate).Concat(nullTerminator).ToArray();
        }
    }
}
