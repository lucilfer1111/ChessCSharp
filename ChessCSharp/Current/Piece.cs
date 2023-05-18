using System;
using System.Resources;
using System.Drawing;

namespace Chess
{
    public class Piece
    {

        public const int None = 0;
        public const int King = 1;
        public const int Pawn = 2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Rook = 5;
        public const int Queen = 6;

        public const int White = 8;
        public const int Black = 16;

        const int typeMask = 0b00111; 
        const int colourMask = 0b11000; 

        public static bool IsColour(int piece, int colour)
        {
            return (piece & colourMask) == colour; 
        }

        public static bool IsType(int piece, int type)
        {
            return (piece & typeMask) == type; 
        }

        public static int Colour(int piece)
        {
            return piece & colourMask;
        }

        public static int Type(int piece)
        {
            return piece & typeMask;
        }

        public static int Value(int piece) 
        {
            switch (Type(piece))
            {
                case Pawn:
                    return 130;
                case Knight:
                    return 300;
                case Bishop:
                    return 350;
                case Rook:
                    return 500;
                case Queen:
                    return 900;
            }

            return 1000; 

        }

        public static string PieceToImageName(int piece) 
        {
            string Colour = "W";
            string Type = "Pawn";

            if (Piece.Colour(piece) == 16)
            {
                Colour = "B"; 
            }

            switch (Piece.Type(piece))
            {
                case 1:
                    Type = "King";
                    break;
                case 2:
                    Type = "Pawn";
                    break;
                case 3:
                    Type = "Knight";
                    break;
                case 4:
                    Type = "Bishop";
                    break;
                case 5:
                    Type = "Rook";
                    break;
                case 6:
                    Type = "Queen";
                    break;
            }

            string pieceImageName = Colour + Type; 

            return pieceImageName;
        }

        public static string PieceToFullName(int piece) 
        {
            if(piece == 0)
            {
                return "null"; 
            }

            string pieceImageName = PieceToImageName(piece); 
            if(pieceImageName[0] == 'B')
            {
                return ($"Black {pieceImageName.Remove(0, 1)}"); 
            }
            else
            {
                return ($"White {pieceImageName.Remove(0, 1)}"); 
            }
        }

        public static Bitmap PieceToImage(int piece) 
        {
            if(piece == 0)
            {
                return null;
            }

            string pieceImageName = PieceToImageName(piece); 

            ResourceManager RecourceManager = Properties.Resources.ResourceManager; 
            Bitmap pieceImage = (Bitmap)RecourceManager.GetObject(pieceImageName); 

            return pieceImage;
        }

        public static string ColourNameFromPieceBin(int piece) 
        {
            string pieceImageName = PieceToImageName(piece);
            if (pieceImageName[0] == 'B')
            {
                return ("Black");
            }
            else
            {
                return ("White");
            }
        }

        public static string ColourNameFromColourBin(int colour)
        {
            return colour == 8 ? "White" : "Black";
        }

    }
}
