using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace OpenSeaDragon
{
    /// <summary>
    /// Class that generates the tiling of an image to be used with the js plugin openSeadragon
    /// </summary>
    public class OSTileGenerator
    {
        private readonly int _tileSize;
        private readonly int _overlap;
        private string _tileFormatTxt;
        internal string _pathCreated;
        internal string _jsonFileName;
        internal string _dziFileName;
        private readonly string _sourceImage;

        /// <summary>
        /// Constructor of the OSTileGenerator class
        /// </summary>
        /// <param name="sourceImagePath">Indicates the image resource</param>
        /// <param name="tileSize">Indicates the size of the tiles</param>
        /// <param name="overlap">Overlap</param>
        public OSTileGenerator(string sourceImagePath, int tileSize = 256, int overlap = 0)
        {
            _sourceImage = sourceImagePath;
            _tileSize = tileSize;
            _overlap = overlap;
        }

        /// <summary>
        /// Method that starts the tiling of the images
        /// </summary>
        public bool GenerateTiles()
        {
            try
            {
                // We get the image
                var imageFile = File.OpenRead(_sourceImage);
                using (Image<Rgb24>? image = Image.Load<Rgb24>(imageFile))
                {
                    // We get the image format
                    _tileFormatTxt = Path.GetExtension(_sourceImage);

                    // We get the URL where to save the images and the (clean) name
                    string? originalPath = Path.GetDirectoryName(_sourceImage);
                    string fileName = Path.GetFileNameWithoutExtension(_sourceImage);
                    fileName = CleanupFileName(fileName);

                    string path = originalPath + Path.DirectorySeparatorChar + fileName + "_files" + Path.DirectorySeparatorChar;

                    // We create the directory
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        Console.WriteLine("Directory created: " + path);
                    }
                    else
                    {
                        // We delete the files and folders in this folder
                        var files = Directory.EnumerateFiles(path).ToList();
                        var folders = Directory.EnumerateDirectories(path).ToList();
                        // WE DELETE THE FILES INSIDE THE FOLDER!!!
                        files.ForEach(e => File.Delete(e));
                        // WE DELETE THE DIRECTORIES RECURSIVELY INSIDE THE FOLDER!!!
                        folders.ForEach(e => Directory.Delete(e, true));
                    }

                    // We get the dimensions of the images
                    int width = image.Width;
                    int height = image.Height;

                    int levels = GetNumLevels(width, height);

                    // We create the tiling (Recursive iteration)
                    for (int i = levels - 1; i >= 0; i--)
                    {
                        // We determine the levels
                        double scale = GetScaleForLevel(levels, i);
                        // We calculate the dimensions of the image for each level
                        Dictionary<string, int>? dimension = GetDimensionForLevel(width, height, scale);
                        // We generate the tiling indicating the dimensions of the image for that level
                        CreateLevelTiles(dimension["width"], dimension["height"], image, path, i);

                    }

                    // We generate the tiling information files
                    if (CheckJsonFileName(fileName))
                    {
                        // We create the JSONP file
                        string jsonP = CreateJSONP(fileName, height, width);
                        File.WriteAllText(originalPath + Path.DirectorySeparatorChar + fileName + ".js", jsonP, Encoding.UTF8);


                        // We create the DZI file (XML)
                        string DZI = CreateDZI(height, width);
                        File.WriteAllText(originalPath + Path.DirectorySeparatorChar + fileName + ".dzi", DZI, Encoding.UTF8);


                        _jsonFileName = originalPath + Path.DirectorySeparatorChar + fileName + ".js";
                        _dziFileName = originalPath + Path.DirectorySeparatorChar + fileName + ".dzi";

                    }

                    _pathCreated = path;

                }


                imageFile.Close();

            }
            catch (Exception ex) 
            {
                return false; 
            }

            return true;
        }

        /// <summary>
        /// We get the number of levels needed according to the size of the image
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <returns>Integer indicating the number of levels to develop</returns>
        private int GetNumLevels(int width, int height)
        {
            int maxDimension = Math.Max(width, height);

            var log = Math.Log(maxDimension, 2);

            var levels = (int)Math.Ceiling(log + 1);

            return levels;

        }

        /// <summary>
        /// Get the scale to reduce the image for each level
        /// </summary>
        /// <param name="numLevels">Number of levels</param>
        /// <param name="level">Current level</param>
        /// <returns>Number indicating the scale</returns>
        private double GetScaleForLevel(int numLevels, double level)
        {
            var maxLevel = numLevels - 1;

            return Math.Pow(0.5, maxLevel - level);
        }

        /// <summary>
        /// Method that returns the dimensions of the image given the original dimensions and the scale to reduce
        /// </summary>
        /// <param name="width">Width of the original image</param>
        /// <param name="height">Height of the original image</param>
        /// <param name="scale">Scale to reduce</param>
        /// <returns>Dictionary with the width and height values of the reduced image</returns>
        private Dictionary<string, int> GetDimensionForLevel(int width, int height, double scale)
        {
            width = (int)Math.Ceiling(width * scale);
            height = (int)Math.Ceiling(height * scale);

            return new Dictionary<string, int>() { { "width", width }, { "height", height } };

        }

        /// <summary>
        /// Method that creates the tiling of the images at each level
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="image">Original image</param>
        /// <param name="path">Path to save the tiling</param>
        /// <param name="level">Tiling level</param>
        private void CreateLevelTiles(int width, int height, Image image, string path, int level)
        {
            // We generate the directory with the level
            string levelPath = path + level + Path.DirectorySeparatorChar;
            // We create the directory
            if (!Directory.Exists(levelPath))
            {
                Directory.CreateDirectory(levelPath);
                Console.WriteLine("Directory created: " + levelPath);
            }

            // We resize the image and create a copy of it
            // using Image imageBitMap = new Image<Rgb24>(width, height);
            using (Image<Rgb24>? cloneResized = image.CloneAs<Rgb24>())
            {
                cloneResized.Mutate(x => x.Resize(width, height));

                // We calculate the number of rows the tiling will have
                var rows = (int)Math.Ceiling((double)height / (_tileSize - _overlap));
                // We calculate the number of columns the tiling will have
                var cols = (int)Math.Ceiling((double)width / (_tileSize - _overlap));
                // We create the images for each row and column
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < cols; col++)
                    {
                        // We get the width and the hight of the crop
                        var tileWidth = Math.Min(_tileSize, width - col * (_tileSize - _overlap));
                        var tileHeight = Math.Min(_tileSize, height - row * (_tileSize - _overlap));

                        // Crop with the required positions into the image with the required sizes
                        var tile = cloneResized.Clone(ctx => ctx.Crop(new Rectangle(col * (_tileSize - _overlap), row * (_tileSize - _overlap), tileWidth, tileHeight)));
                        tile.Save($"{levelPath}{col}_{row}{_tileFormatTxt}", new JpegEncoder());
                    }
                }
            }
            
            Console.WriteLine("Created level images: " + level);

        }

        /// <summary>
        /// Method that creates a JSON (in text) with the image data.
        /// This method will be called at the end of the tiling to obtain information
        /// about the image
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <param name="height">Height of the image</param>
        /// <param name="width">Width of the image</param>
        /// <returns>String with the jsonp content</returns>
        private string CreateJSONP(string filename, int height, int width)
        {
            return @$"{filename}(
            {{
                Image:
                {{
                    xmlns: 'http://schemas.microsoft.com/deepzoom/2008',
                    Format: '{_tileFormatTxt}',
                    Overlap: {_overlap},
                    TileSize: {_tileSize},
                    Size:
                    {{
                        Width: {width},
                        Height: {height}
                    }}
                }}
            }});";
        }

        /// <summary>
        /// Method that creates an XML (in text) with the image data.
        /// This method will be called at the end of the tiling to obtain information
        /// about the image
        /// </summary>
        /// <param name="height">Height of the image</param>
        /// <param name="width">Width of the image</param>
        /// <returns>The DZI string to print</returns>
        private string CreateDZI(int height, int width)
        {
            return @$"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <Image xmlns=""http://schemas.microsoft.com/deepzoom/2008""
                    Format=""{_tileFormatTxt}""
                    Overlap=""{_overlap}""
                    TileSize=""{_tileSize}"">
                    <Size Height=""{height}""
                        Width=""{width}"" />
                </Image>
            ";
        }

        /// <summary>
        /// Method that cleans the image name to use it in the tiling
        /// </summary>
        /// <param name="name">Name of the image</param>
        /// <returns>The string cleaned</returns>
        private string CleanupFileName(string name)
        {
            name = name.Trim();
            // Deletes all non-alphanumeric characters and spaces
            name = Regex.Replace(name, "[^a-zA-Z0-9 ]", "");
            // Allows only one space
            name = Regex.Replace(name, " +", "_");
            return name;
        }

        /// <summary>
        /// Checks if it is a suitable name for naming a JSON file
        /// </summary>
        /// <param name="name">Name of the image</param>
        /// <returns>Bool if the json file has a suitable name</returns>
        public bool CheckJsonFileName(string name)
        {
            // for JSONP filename cannot contain special characters
            string specialCharRegex = "/[\'^£%&*()}{@#~?><> ,|=+¬-]/";

            if (Regex.IsMatch(name, specialCharRegex))
            {
                return false;
            }
            // for JSONP filename cannot start with a number
            string stringFirstChar = name.Substring(0, 1);
            // if numeric add 'a' to beginning of filename
            int numb = 0;
            if (int.TryParse(stringFirstChar, out numb))
            {
                return false;
            }

            return true;
        }



    }
}

