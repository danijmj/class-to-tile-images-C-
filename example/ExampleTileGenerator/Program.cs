// See https://aka.ms/new-console-template for more information
using OpenSeaDragon;

string urlImage = "./pexels-mile-ribeiro-6930033-6497794.jpg";
OSTileGenerator opSDragonTilGen = new(urlImage);
try
{
    // Generamos el teselado de las imágenes del OpenSeaDragon
    opSDragonTilGen.GenerateTiles();
    Console.WriteLine("Image tiles generated successfully");
}
catch (Exception e)
{
    Console.WriteLine("Error when I try generate the telesate: " + e.Message);
}