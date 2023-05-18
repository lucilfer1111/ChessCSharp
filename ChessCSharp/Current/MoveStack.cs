using System;
using System.Collections.Generic;

namespace Chess
{
    class MoveStack
    {
        public static List<Move> moveStack = new List<Move>();
        public static List<Move> PoppedMoves = new List<Move>();
        public static string MoveToDescriptiveNotation(Move move)
        {
            if (Piece.Type(move.Piece) == Piece.King && Math.Abs((move.MoveFrom % 8) - (move.MoveTo % 8)) > 1) 
            {
                if (move.MoveTo == 2) // BQS
                {
                    return "O-O-O";
                }
                else if (move.MoveTo == 6) // BKS
                {
                    return "O-O";
                }
                else if (move.MoveTo == 58) // WQS
                {
                    return "O-O-O";
                }
                else if (move.MoveTo == 62) // WKS
                {
                    return "O-O";
                }
            }
            string DescriptiveNotation = "";
            string pieceDesc = "";
            string capture = "";
            string check = "";

            if (Piece.Type(move.Piece) == Piece.Knight) 
            {
                pieceDesc = "N";
            }
            else if(Piece.Type(move.Piece) == Piece.Pawn)
            {
                pieceDesc = "";
            }
            else
            {
              
                pieceDesc = Piece.PieceToFullName(move.Piece).Split(' ')[1][0].ToString();
            }
            int row = move.MoveTo / 8;
            int col = move.MoveTo % 8;
            string letter = (Convert.ToChar(col + 97)).ToString();
            string number = (8 - row).ToString();

            if(GameControl.Board[move.MoveTo].PieceOnSquare != 0) 
            {
                int moveFromCol = move.MoveFrom % 8;
                string moveFromLetter="";

                if (pieceDesc != "q" && pieceDesc != "k") 
                {
                    moveFromLetter = char.ToLower(Convert.ToChar(moveFromCol + 65)).ToString();
                }

                capture = "x"; 
                pieceDesc = pieceDesc+moveFromLetter; 
            }

            if(GameControl.KingInCheck == true)
            {
                check = "+";
            }

            DescriptiveNotation += pieceDesc + capture + letter + number + check; 

            return DescriptiveNotation;
        }

        public static void Pop() 
        {
            Move move = moveStack[moveStack.Count];
            PoppedMoves.Add(move);
            moveStack.RemoveAt(moveStack.Count);
        }

        public static void Push(Move move) 
        {
            moveStack.Add(move);
            GameWindow.UpdateMoveStackDisplay(MoveToDescriptiveNotation(move));
        }
    }
}
