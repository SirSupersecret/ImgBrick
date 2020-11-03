using System;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace ImgBrick
{
    class Program
    {
        //version: major_version.minor_version.build
        static readonly string version = "0.2.2";
        static readonly int defaultRes = 32;
        static readonly List<string> imageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".PNG", ".GIF", ".TIFF" };
        static List<Vector3> palette = new List<Vector3>();

        static string path_image;
        static string path_palette;
        static int resX; 
        static int resY = defaultRes;
        static Image image;
        static bool demoMode, defaultPalette;
        static void Main(string[] args)
        {

            PrintVersion();
            ReadArguments(args);

            var assembly = Assembly.GetEntryAssembly();
            var integratedPalette = assembly.GetManifestResourceStream("ImgBrick.resources.ColorPalette.csv");
            var demoImage = assembly.GetManifestResourceStream("ImgBrick.resources.ImgBrick.png");

            if(demoMode || defaultPalette){
                System.Console.WriteLine("Loading color palette form internal");
                ReadPalette(integratedPalette);
            }else{
                System.Console.WriteLine("Loading color palette form " + path_palette);
                ReadPalette(path_palette);
            }

            if(demoMode){
                System.Console.WriteLine("Loading image from internal");
                image = Image.FromStream(demoImage);
            }else{
                System.Console.WriteLine("Loading image from " + path_image);
                image = Image.FromFile(path_image);
            }
            

            ProcessImage();

            Console.WriteLine("Conversion succesful!");
            SaveImage(ImageFormat.Png);
        }

        static void ReadPalette(Stream path){

            var reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(",");
                var colors = values[0].Split(" ");
                palette.Add(new Vector3(int.Parse(colors[0]), int.Parse(colors[1]), int.Parse(colors[2])));
            }
        }
        static void ReadPalette(string path){

            var reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(",");
                var colors = values[0].Split(" ");
                palette.Add(new Vector3(int.Parse(colors[0]), int.Parse(colors[1]), int.Parse(colors[2])));
            }
        }
        static void SaveImage(ImageFormat f){
            string path_result;
            if(demoMode){
                path_result = Directory.GetParent(Environment.CurrentDirectory).FullName;
            }else{
                path_result = Path.GetDirectoryName(path_image);
            }
            
            path_result += "/";
            path_result += Path.GetFileNameWithoutExtension(path_image);
            if(demoMode) path_result += "ImgBrick";
            path_result += "_BRICKIFIED.";
            path_result += f.ToString().ToLowerInvariant();

            System.Console.WriteLine("Saving new image as " + path_result);

            image.Save(path_result, f);
        }

        private static void ProcessImage(){
            float ratio = image.Size.Width * 1f / image.Size.Height * 1f;
            resX = Convert.ToInt32(ratio * resY);
            image = ResizeImage(image, new Size(resX, resY));

            System.Console.Write("New Resolution is ");
            System.Console.WriteLine(image.Size.ToString());


            image = LimitImageColors(image, palette);
        }

        static Image LimitImageColors(Image imgToEdit, List<Vector3> colors){
            Bitmap map = (Bitmap)imgToEdit;

            //
            //improvement idea: use Bitmap.Lockbits and Marshal.Copy;
            //requires further research
            //

            int width = map.Width;
            int height = map.Height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color oldPixel = map.GetPixel(x, y);
             
                    // calculate new pixel value

                    Color newPixel = GetClosestColor(oldPixel, palette);
 
                    map.SetPixel(x, y, newPixel);
                }
            }

            return (Image) map;
        }

        static Color GetClosestColor(Color pixel, List<Vector3> colors){
            Vector3 pixelColor = new Vector3(pixel.R, pixel.G, pixel.B);
            Vector3 newPixelColor = new Vector3();
            float bestDistance = -1f;
            
            foreach(var c in colors){
                var distance = Vector3.Distance(c, pixelColor);

                if(bestDistance < 0){
                    Vector3.Distance(c, pixelColor);
                    newPixelColor = c;
                    bestDistance = distance;
                }

                if(distance <= bestDistance){
                    bestDistance = distance;
                    newPixelColor = c;
                }
            }

            Color newColor = Color.FromArgb((int)newPixelColor.X, (int)newPixelColor.Y, (int)newPixelColor.Z);
            return newColor;
        }

        public static Image ResizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private static void ReadArguments(String[] args){
            //check if enough arguments are given
            if(args.Length < 1){
                Console.WriteLine("No arguments given - DEMO mode activated.");
                System.Console.WriteLine("Using default image and color palette at fallback resolution (" + defaultRes + ").");
                System.Console.WriteLine("'imgbrick -h' for more info.");

                demoMode = true;
                
                return;
            }

            if(args[0] == "-h" || args[0] == "--help" || args[0] == "help"){
                Help();
                Environment.Exit(0);
            }

            //check if selected file exists
            if(!File.Exists(args[0])){
                Console.Error.WriteLine("ERROR: This path does not exist.");
                System.Console.WriteLine("'imgbrick -h' for more info.");
                Environment.Exit(0);
            }

            //check if selected file is supported
            if (!imageExtensions.Contains(Path.GetExtension(args[0]).ToUpperInvariant()))
            {
                Console.Error.WriteLine("ERROR: This filetype is not supported.");
                PrintSupportedFiletypes();
                System.Console.WriteLine("'imgbrick -h' for more info.");
                Environment.Exit(0);
            }
            
            if(args.Length < 2){
                resY = defaultRes;
                System.Console.WriteLine("No resolution target given, hence falling back to default resolution: " + defaultRes);
                System.Console.WriteLine("'imgbrick -h' for more info.");
            }
            
            else if(!int.TryParse(args[1], out resY))
            {
                Console.Error.WriteLine("ERROR: NaN");
                System.Console.WriteLine("'imgbrick -h' for more info.");
                Environment.Exit(0);
            }

            path_image = args[0];
        }

        static void Help(){
            Console.WriteLine("ImgBrick converts normal images into low-res version reduced to a given color palette.");
            Console.WriteLine("This can be used to easily plan a LEGO-picture (thus the name \"ImgBrick\").\n\n");
            PrintUsage();
        }

        static void PrintVersion(){
            Console.WriteLine("ImgBrick v" + version);
        }

        static void PrintUsage(){
            Console.WriteLine("Usage: imgbrick [path-to-source] [target-resolution-vertical]\n");
            PrintSupportedFiletypes();
            System.Console.Write("target-resolution-vertical:  ");
            System.Console.WriteLine("Defines y-resolution.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Aspect ratio is matched automatically.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Fallback to 32 if no value is given.\n");
            System.Console.Write("path-to-color_palette:       ");
            System.Console.WriteLine("Path to the color palette containing the allowed colors for a given image.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Non-allowed colors get changed to the mathematically closest allowed color.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Formatting: [file-name].csv\n");
            System.Console.Write("                                         ");
            System.Console.WriteLine("[r] [g] [b],[ignored...]");
            System.Console.Write("                                         ");
            System.Console.WriteLine("one color per row, color values seperated by [space]; second column and onward are ignored");
            System.Console.Write("                             ");
            System.Console.WriteLine("Fallback to integrated color palette configured for locally avaliable 1x1 LEGO bricks.");
            System.Console.Write("                             ");
            System.Console.WriteLine("Custom color palettes are currently not supported.");
        }

        static void PrintSupportedFiletypes(){
            Console.Error.Write("Supported source filetypes:  ");
            foreach (var element in imageExtensions)
            {
                Console.Error.WriteLine(element);
                Console.Error.Write("                             ");    
            }
            Console.Error.WriteLine();
        }
    }
}
