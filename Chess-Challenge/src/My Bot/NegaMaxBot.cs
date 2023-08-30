using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class NegaMaxBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        int infinity = 1000000000;

        int GetAttacks(Board board, bool isWhite, ulong occupiedSquares)
        {
            int squares_attacked = 0;
            foreach (Piece pawn in board.GetPieceList(PieceType.Pawn, true))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(pawn.Square, true));
            }
            foreach (Piece knight in board.GetPieceList(PieceType.Knight, true))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(knight.Square));
            }
            foreach (Piece bishop in board.GetPieceList(PieceType.Bishop, true))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Bishop, bishop.Square, occupiedSquares));
            }
            foreach (Piece rook in board.GetPieceList(PieceType.Rook, true))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Rook, rook.Square, occupiedSquares));
            }
            foreach (Piece queen in board.GetPieceList(PieceType.Queen, true))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Queen, queen.Square, occupiedSquares));
            }

            Console.WriteLine(squares_attacked);
            return squares_attacked;
        }

        // int GetUniqueAttacks(Board board, bool isWhite, ulong occupiedSquares)
        // {
        //     ulong squares_attacked = 0;
        //     foreach (Piece pawn in board.GetPieceList(PieceType.Pawn, true))
        //     {
        //         squares_attacked = BitboardHelper.GetPawnAttacks(pawn.Square, true));
        //     }
        //     foreach (Piece knight in board.GetPieceList(PieceType.Knight, true))
        //     {
        //         squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(knight.Square));
        //     }
        //     foreach (Piece bishop in board.GetPieceList(PieceType.Bishop, true))
        //     {
        //         squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Bishop, bishop.Square, occupiedSquares));
        //     }
        //     foreach (Piece rook in board.GetPieceList(PieceType.Rook, true))
        //     {
        //         squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Rook, rook.Square, occupiedSquares));
        //     }
        //     foreach (Piece queen in board.GetPieceList(PieceType.Queen, true))
        //     {
        //         squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Queen, queen.Square, occupiedSquares));
        //     }

        //     Console.WriteLine(squares_attacked);
        //     return squares_attacked;
        // }

        int evaluate(Board board)
        {
            if (board.IsInCheckmate())
            {
                return infinity;
            }
            // evaluate a positive score for the player who made the move
            int score = ((
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Pawn, false))
            ) * pieceValues[1] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Knight, false))
            ) * pieceValues[2] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Bishop, false))
            ) * pieceValues[3] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Rook, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Rook, false))
            ) * pieceValues[4] +
            (
                BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, true))
                - BitboardHelper.GetNumberOfSetBits(board.GetPieceBitboard(PieceType.Queen, false))
            ) * pieceValues[5]) ;
            // number of squares controlled

            // number of squares attacked
            score += (GetAttacks(board, true, board.AllPiecesBitboard) - GetAttacks(board, false, board.AllPiecesBitboard))*50;
            return score * (board.IsWhiteToMove ? -1 : 1);

        }


        public int NegaMax(Board board, Move madeMove, int depth)
        {
            // if ( depth == 0 ) return evaluate();
            // int max = -infinity;
            // for ( all moves)  {
            //     score = -negaMax( depth - 1 );
            //     if( score > max )
            //         max = score;
            // }
            // return max;

            if (depth == 0) return evaluate(board);

            Move[] allMoves = board.GetLegalMoves();
            int max = -infinity;

            foreach (Move move in allMoves)
            {
                board.MakeMove(move);
                int score = -NegaMax(board, move, depth - 1);
                board.UndoMove(move);

                if (score > max)
                {
                    max = score;
                }
            }
            return max;
        }

        public Move FirstNegaMax(Board board, int depth)
        {
            // Pick a random move to play if nothing better is found

            Console.WriteLine("Evalutate postition: " + evaluate(board));

            Move[] allMoves = board.GetLegalMoves();
            Move moveToPlay = allMoves[0];
            int max = -infinity;

            foreach (Move move in allMoves)
            {
                board.MakeMove(move);
                int score = -NegaMax(board, move, depth);
                board.UndoMove(move);

                if (score > max)
                {
                    max = score;
                    moveToPlay = move;
                }
            }
            return moveToPlay;
        }

        public Move Think(Board board, Timer timer)
        {
            return FirstNegaMax(board, 2);
        }
    }
}