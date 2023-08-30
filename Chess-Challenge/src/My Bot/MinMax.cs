using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class MinMaxBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        int infinity = 1000000000;

        int GetAttacks(Board board, bool isWhite, ulong occupiedSquares)
        {
            int squares_attacked = 0;
            foreach (Piece pawn in board.GetPieceList(PieceType.Pawn, isWhite))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(pawn.Square, isWhite));
            }
            foreach (Piece knight in board.GetPieceList(PieceType.Knight, isWhite))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(knight.Square));
            }
            foreach (Piece bishop in board.GetPieceList(PieceType.Bishop, isWhite))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Bishop, bishop.Square, occupiedSquares));
            }
            foreach (Piece rook in board.GetPieceList(PieceType.Rook, isWhite))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Rook, rook.Square, occupiedSquares));
            }
            foreach (Piece queen in board.GetPieceList(PieceType.Queen, isWhite))
            {
                squares_attacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Queen, queen.Square, occupiedSquares));
            }
            return squares_attacked;
        }

        int GetUniqueAttacks(Board board, bool isWhite, ulong occupiedSquares)
        {
            ulong squares_attacked = 0;
            foreach (Piece pawn in board.GetPieceList(PieceType.Pawn, isWhite))
            {
                squares_attacked |= BitboardHelper.GetPawnAttacks(pawn.Square, isWhite);
            }
            foreach (Piece knight in board.GetPieceList(PieceType.Knight, isWhite))
            {
                squares_attacked |= BitboardHelper.GetKnightAttacks(knight.Square);
            }
            foreach (Piece bishop in board.GetPieceList(PieceType.Bishop, isWhite))
            {
                squares_attacked |= BitboardHelper.GetSliderAttacks(PieceType.Bishop, bishop.Square, occupiedSquares);
            }
            foreach (Piece rook in board.GetPieceList(PieceType.Rook, isWhite))
            {
                squares_attacked |= BitboardHelper.GetSliderAttacks(PieceType.Rook, rook.Square, occupiedSquares);
            }
            foreach (Piece queen in board.GetPieceList(PieceType.Queen, isWhite))
            {
                squares_attacked |= BitboardHelper.GetSliderAttacks(PieceType.Queen, queen.Square, occupiedSquares);
            }

            return BitboardHelper.GetNumberOfSetBits(squares_attacked);
        }

        //     Console.WriteLine(squares_attacked);
        //     return squares_attacked;
        // }

        int evaluate(Board board)
        {
            if (board.IsDraw())
            {
                return 0;
            }

            if (board.IsInCheckmate())
            {
                return board.IsWhiteToMove ? -infinity : infinity;
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
            // score += (GetAttacks(board, true, board.AllPiecesBitboard) - GetAttacks(board, false, board.AllPiecesBitboard))*25;
            score += (GetUniqueAttacks(board, true, board.AllPiecesBitboard) - GetUniqueAttacks(board, false, board.AllPiecesBitboard))*25;


            if (board.IsDraw())
            {
                return board.IsWhiteToMove ? 0 : -10;
            }

            return score; //* (board.IsWhiteToMove ? -1 : 1);

        }

        public int MinMax(Board board, int depth, bool isMaximizingPlayer)
        {
            if (depth == 0 || board.IsInCheckmate())
            {
                return evaluate(board);
            }

            int bestValue;
            if (isMaximizingPlayer)
            {
                bestValue = int.MinValue;
                foreach (Move move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    int score = MinMax(board, depth - 1, false);
                    board.UndoMove(move);
                    bestValue = Math.Max(bestValue, score);
                }
            }
            else
            {
                bestValue = int.MaxValue;
                foreach (Move move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    int score = MinMax(board, depth - 1, true);
                    board.UndoMove(move);

                    bestValue = Math.Min(bestValue, score);
                }
            }
            return bestValue;
        }

        public Move FirstMinMax(Board board, int depth, bool isMaximizingPlayer)
        {
            int bestValue;
            Move bestMove = new Move();
            if (isMaximizingPlayer)
            {
                bestValue = int.MinValue;
                foreach (Move move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    int score = MinMax(board, depth - 1, false);
                    if( score > bestValue)
                    {
                        bestValue = score;
                        bestMove = move;
                    }
                    board.UndoMove(move);
                }
            }
            else
            {
                bestValue = int.MaxValue;
                foreach (Move move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    int score = MinMax(board, depth - 1, true);
                    board.UndoMove(move);

                    if (score < bestValue)
                    {
                        bestValue = score;
                        bestMove = move;
                    }

                }
            }
            return bestMove;
        }

        public Move Think(Board board, Timer timer)
        {
            return FirstMinMax(board, 3, board.IsWhiteToMove);
        }
    }
}