// See https://aka.ms/new-console-template for more information
using OpenSeaDragon;

string urlImage = "./pexels-mile-ribeiro-6930033-6497794.jpg";
try
{
    // We generate the tiles for OpenSeaDragon
    OSDTileGenerator.GenerateTiles(urlImage);
    Console.WriteLine("Image tiles generated successfully");
}
catch (Exception e)
{
    Console.WriteLine("Error when I try generate the telesate: " + e.Message);
}