using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ImageResizer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Program();
        }

        Program(float max_width = 2000, long quality = 90L)
        {
            target_dir = set_target_dir();

            this.max_width = max_width;
            this.quality = quality;

            if (target_dir == null)
                return;

            ThreadPool.SetMaxThreads(20, 20);

            if (!Directory.Exists(output_dir))
            {
                Directory.CreateDirectory(output_dir);
            }

            var paths = glob(target_dir, "*.jpg");
            int count = paths.Length;
           
            foreach (var path in paths)
            {
                ThreadPool.QueueUserWorkItem((o) => {
                    minify_img(path);

                    Console.WriteLine("Converted: " + path);

                    if (Interlocked.Decrement(ref count) == 0)
                    {
                        all_done_event.Set();
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });
            }
            WaitHandle.WaitAll(new ManualResetEvent[] { all_done_event });
        }

        const string output_dir = "resized";
        ManualResetEvent all_done_event = new ManualResetEvent(false);
        string target_dir;
        float max_width;
        long quality;

        string set_target_dir()
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "Select a folder to batch convert.";
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = Environment.CurrentDirectory;
            if (fbd.ShowDialog() == DialogResult.OK)
                return fbd.SelectedPath;
            else
                return null;
        }

        String[] glob(String path, String pattern)
        {
            return Directory.GetFiles(path, pattern, SearchOption.AllDirectories).Select(
                s => Path.GetFileName(s)
            ).ToArray();       
        }

        void minify_img(string path)
        {
            using (FileStream stream = new FileStream(
                Path.Combine(target_dir, path), FileMode.Open, FileAccess.Read)
            )
            {
                var img = Image.FromStream(stream);
                save_img(
                    resize_img(img),
                    Path.Combine(output_dir, path)
                );
            }
        }

        Image resize_img(Image img)
        {
            var ratio = (float) img.Width / img.Height;
            var width = (float) img.Width;
            if (img.Width > max_width)
            {
                width = max_width;
            }
            var new_img = new Bitmap(
                (int) width,
                (int) (width / ratio)
            );
            Graphics.FromImage(new_img).DrawImage(img, 0, 0, new_img.Width, new_img.Height);
            return new_img;
        }

        void save_img(Image img, String save_to)
        {
            var jpg_codec = ImageCodecInfo.GetImageDecoders().First(
                el => el.FormatID == ImageFormat.Jpeg.Guid
            );

            var encoder = System.Drawing.Imaging.Encoder.Quality;

            var eparam = new EncoderParameter(encoder, quality);

            var eparams = new EncoderParameters(1);

            eparams.Param[0] = eparam;

            using (Stream stream = new FileStream(save_to, FileMode.OpenOrCreate, FileAccess.Write))
            {
                img.Save(stream, jpg_codec, eparams);
            }
        }
    }
}
