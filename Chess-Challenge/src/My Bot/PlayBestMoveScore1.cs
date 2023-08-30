using ChessChallenge.API;
using System;


// Decision tree bot
// look for checks
// look for captures
// highest scoring move = defensive fortress

// [V] don't capture if can be recaptured
// don't check opp, if can be captured (not checking for check atm)

// Bad approach

// Eval board = score for white - score for black (* depth)

// for each moving piece, get the squares it was defending
// for each of those squares, get the pieces that are on those squares
// get the scores of the pieces on those squares
// move the piece that is defending the least
// move to undefended squares


public class PlayBestMoveScore1 : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {

        Move[] allMoves = board.GetLegalMoves();

        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        int highestValue = 0;

        foreach (Move move in allMoves)
        {
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                moveToPlay = move;
                break;
            }

            if (OpponentMoveIsCheckMate(board, move))
            {
                // This is not the move to make
                continue;
            }

            int value = MoveScore(board, move, true);
            // Find highest scoring move
            if ((value > highestValue))
            {
                moveToPlay = move;
                highestValue = value;
                // Console.WriteLine("New highest value: " + move.ToString());
                // break;
            }
        }

        return moveToPlay;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    bool OpponentMoveIsCheckMate(Board board, Move move)
    {
        // don't play into checkmate
        board.MakeMove(move);
        Move[] opponentMoves = board.GetLegalMoves();
        bool opponentMoveIsCheckmate = false;

        foreach (Move opponentMove in opponentMoves)
        {
            if (MoveIsCheckmate(board, opponentMove))
            {
                opponentMoveIsCheckmate = true;
                break;
            }
        }
        board.UndoMove(move);
        return opponentMoveIsCheckmate;
    }

    // Test if this move gives checkmate
    int MoveScore(Board board, Move move, bool recursive = false)
    {

        // Need to eval board, or find bad moves

        // don't capture if can be recaptured
        // don't check if can be checked

        PieceType movedPiece = move.MovePieceType;

        int capPieceValue = pieceValues[(int)move.CapturePieceType];
        int movedPieceValue = pieceValues[(int)movedPiece];
        int defensiveScore = BitBoardScore(board, move, true);
        int offensiveScore = BitBoardScore(board, move, false);

        int sacrificedPieceValue = board.SquareIsAttackedByOpponent(move.TargetSquare)  ? movedPieceValue : 0;

        if (capPieceValue == 0)
        {
            // Console.WriteLine("No capture" + defensiveScore + "-" + offensiveScore  );
        }

        return capPieceValue + defensiveScore + offensiveScore - sacrificedPieceValue;
    }

    int[] pieceDefensiveValues = { 5, 10, 30, 30, 50, 90, 0 };

    int BitBoardScore(Board board, Move move, bool defensive)
    {
        PieceType movedPiece = move.MovePieceType;
        ulong moveAttacks = BitboardHelper.GetPieceAttacks(movedPiece, move.StartSquare, board, board.IsWhiteToMove);
        ulong defensiveBitboard = defensive & board.IsWhiteToMove ? board.WhitePiecesBitboard & moveAttacks : board.BlackPiecesBitboard & moveAttacks;

        int score = 0;
        foreach (int piece in Enum.GetValues(typeof(PieceType))) {
            int numberOfPieceInBoard = BitboardHelper.GetNumberOfSetBits(
                board.GetPieceBitboard((PieceType)piece, defensive & board.IsWhiteToMove) & defensiveBitboard
            );
            score += (pieceDefensiveValues[piece] * numberOfPieceInBoard);
        }

        return score;

    }

}
