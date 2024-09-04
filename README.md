# OpenSeaDragon Tile Generator Class in C#
Class that tile a image for the Openseadragon library. Runs in **LINUX** 

Openseadragon library: https://openseadragon.github.io/

## Main requeriments
Use the library **SixLabors.ImageSharp** to work (From nuget: https://www.nuget.org/packages/SixLabors.ImageSharp)

## Way to use
Inizialize the constuctor (OSTileGenerator) passing the next data:

 - (string) sourceImagePath => The url of the image
 - (int) tileSize = 256 // The size of the tile (default 256)
 - (int) overlap = 0 // The overlap of the image, usually 0

## Result
Generate a folder with the JSONP and DZI files and the folder with the tiles (in deep) into the main folder in jpg format.

The tiles folder generated is the name of the image plus "_files".

