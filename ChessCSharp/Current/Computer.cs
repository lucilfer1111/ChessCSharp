using System;
using System.Linq;
using System.Collections.Generic;

namespace Chess
{
    class Computer
    {
        public Computer()
        {
        }
        public static int Depth = 3; 
        public static bool MovePutKingInCheck = false;

        public static int EvaluatePos()
        {
            int Evaluation = 0;
            int EndGameWeight = GameControl.GetEndGameWeight();
            if (MovePutKingInCheck == true)
            {
                Evaluation += 150 * GameControl.GetEndGameWeight() > 4 ? 3 : 1; 
            }
            int OpposingKingSquare = PieceLocator.GetLocationsOf(GameControl.OpposingSide() | Piece.King)[0];
            int FriendlyKingSquare = PieceLocator.GetLocationsOf(GameControl.computerSide | Piece.King)[0];

            int OpposingKingRow = OpposingKingSquare % 8;
            int OpposingKingCol = OpposingKingSquare / 8;

            int FriendlyKingRow = FriendlyKingSquare % 8;
            int FriendlyKingCol = FriendlyKingSquare / 8;

            
            int DistanceBetweenKingsCols = Math.Abs(FriendlyKingCol - OpposingKingCol);
            int DistanceBetweenKingsRows = Math.Abs(FriendlyKingRow - OpposingKingRow);
            int DistanceBetweenKings = DistanceBetweenKingsCols + DistanceBetweenKingsRows;

            Evaluation += 14 - (DistanceBetweenKings * EndGameWeight);

            
            int OpposingKingsDistanceFromCentreRow = Math.Max(3 - OpposingKingRow, OpposingKingRow - 4);
            int OpposingKingsDistanceFromCentreCol = Math.Max(3 - OpposingKingCol, OpposingKingCol - 4);
            int OpponsingKingsDistanceFromCentre = OpposingKingsDistanceFromCentreRow + OpposingKingsDistanceFromCentreCol;

            Evaluation += (OpponsingKingsDistanceFromCentre * EndGameWeight);

            
            Evaluation += PieceLocator.GetPosEval();

            return Evaluation;
        }

        
        public static Move GenerateMove()
        {
           
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Generating Computer Move");

            
            Move moveToPlay = new Move { Piece = 0, MoveFrom = -1, MoveTo = -1 };
            List<Move> AvailableMoves = RuleBook.GenerateLegalMoves();

           
            if (AvailableMoves.Count == 0) 
            {
                return moveToPlay;
            }
            if (GameControl.Moves < 8)
            {
                Console.WriteLine("Move within first four, getting opening move");
                moveToPlay = OpeningBook.GetMove(AvailableMoves);

                if (moveToPlay.MoveFrom != -1)
                {
                    Console.WriteLine("Move found, not calculating own move\n");
                    return moveToPlay;
                }
            }

          
            int BestEval = -999999999;
            AvailableMoves = sortMoves(AvailableMoves);
            foreach (Move move in AvailableMoves.ToArray())
            { 
                GameControl.makeTestMove(move);
                int moveEval = (GetMoveEvaluationToDepthOf(Depth, -100000000, 100000000)*-1) + LocationInscentives.GetLocationInscentiveFor(move.Piece, move.MoveTo)*-1;

                Console.WriteLine($"Best eval for move {Piece.PieceToFullName(move.Piece)} moves from {move.MoveFrom} to {move.MoveTo}: {moveEval} "); 
                if (moveEval > BestEval)
                {
                    BestEval = moveEval;
                    moveToPlay = move;
                }

                GameControl.unmakeTestMove(move);
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            GameWindow.BlackTimeLeft -= Convert.ToInt32(elapsedMs) / 1000;

            Console.WriteLine($"\nTime For Computer to generate move: {elapsedMs}ms");
            Console.WriteLine($"Minusing {Convert.ToInt32(elapsedMs) / 1000} seconds from computer clock. New time: {GameWindow.BlackTimeLeft}");
            Console.WriteLine($"Returning move {Piece.PieceToFullName(moveToPlay.Piece)} moves from {moveToPlay.MoveFrom} to {moveToPlay.MoveTo}, eval: {BestEval}");

            return moveToPlay;
        }
        public static int GetMoveEvaluationToDepthOf(int depth, int alpha, int beta)
        {
            if(depth == 0) 
            {
                return EvaluatePos();
            }

            List<Move> responses = RuleBook.GenerateLegalMoves(); 
            MovePutKingInCheck = false;
            if(RuleBook.KingInCheck == true)
            {
                MovePutKingInCheck = true;
            }

            if(responses.Count == 0)
            {
                if(MovePutKingInCheck == true)
                {
                    return -10000 * depth; 
                }
                return 0;
            }

            responses = sortMoves(responses); 

            foreach(Move response in responses.ToArray()) 
            {
                GameControl.makeTestMove(response); 
                int MoveEval = (GetMoveEvaluationToDepthOf(depth - 1, -beta, -alpha) * -1); // Recursiveley call function
                GameControl.unmakeTestMove(response); // Unmake response at the end of recursion
                if(MoveEval >= beta) // If move eval is better than last best move
                {
                    return beta; // Update beta
                }
                alpha = Math.Max(alpha, MoveEval); 
            }
            return alpha; 
        }
        public static List<Move> sortMoves(List<Move> moveList)
        {
            // assign move score guesses
            foreach (Move move in moveList)
            {
                int moveScore = 0;
                int piece = move.Piece;
                int capturedPiece = GameControl.Board[move.MoveTo].PieceOnSquare;

                // Console.WriteLine($"Assigning Move Score To {Piece.PieceToFullName(move.Piece)} moves from {move.MoveFrom} to {move.MoveTo}"); 

                if (capturedPiece != 0)
                {
                    moveScore = Piece.Value(capturedPiece) - Piece.Value(piece); // Capturing piece of lower value is good other way is bad
                }

                if(MovePutKingInCheck == true)
                {
                    moveScore += 300; // Checks are often good as they can lead to forks
                }

                if (RuleBook.SquaresToMoveToThatStopCheck.Contains(move.MoveTo))
                {
                    moveScore += 100; // Blocking check is often good (as opposed to moving the king)
                }

                if (RuleBook.EnemyAttacks.Contains(move.MoveTo))
                {
                    moveScore -= 250; // Moving to position where enemy attacks is often bad
                }

                if (Piece.Type(piece) == Piece.Pawn && Piece.Type(RuleBook.CheckPromotion(move)) == Piece.Queen)
                {
                    moveScore += 500; // Trying to promote is good
                }

                move.MoveScore = moveScore;
            }

            moveList = QuickSort(moveList.ToArray(), 0, moveList.Count - 1).ToList();
            moveList.Reverse();

            return moveList;
        }

        public static Move[] QuickSort(Move[] unsortedList, int leftStartPointer, int rightStartPointer)
        {
            int leftPointer = leftStartPointer; 
            int rightPointer = rightStartPointer; 
            int pivotNumber = unsortedList[leftPointer].MoveScore; 

            while (leftPointer <= rightPointer) 
            {
                while (unsortedList[leftPointer].MoveScore < pivotNumber) 
                {
                    leftPointer++; 
                }
                while (unsortedList[rightPointer].MoveScore > pivotNumber) 
                {
                    rightPointer--; 
                }

                if (leftPointer <= rightPointer)
                {
                   
                    Move moveHolder = unsortedList[leftPointer]; 
                    unsortedList[leftPointer] = unsortedList[rightPointer]; 
                    unsortedList[rightPointer] = moveHolder;
                    leftPointer++; 
                    rightPointer--; 
                }
            }

            if (leftStartPointer < rightPointer) 
            {
                QuickSort(unsortedList, leftStartPointer, rightPointer); 
            }
            if (leftPointer < rightStartPointer) 
            {
                QuickSort(unsortedList, leftPointer, rightStartPointer); 
            }

            Move[] sortedList = unsortedList; 

            return sortedList;

        }
    }
}

