
# OpenSeaDragon Tile Generator Class
Class that tile a image for the openseadragon library. It works in **LINUX**

## Main requeriments
Use the library **SixLabors** to work

  
## Way to use
Inizialize the constuctor passing the next data:

 - (string) sourceImagePath => The url of the image
 - (int) tileSize = 256 // The size of the tile (default 256)
 - (int) overlap = 0 // The overlap of the image, usually 0
  

## Result
Generate a folder with the files of jsop and DZI and the folder with the tiles into the main folder.
