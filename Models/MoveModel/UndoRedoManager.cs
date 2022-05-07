using System.Collections.Generic;

namespace ChessWebApp.Models.MoveModel
{
    public class UndoRedoManager
    {
        private MoveManager moveManager;
        public Stack<Move> UndoStack { get; set; }
        public UndoRedoManager(MoveManager moveManager)
        {
            this.moveManager = moveManager;
            UndoStack = moveManager.UndoStack;
        }

        public void UndoMove(Board board)
        {
            

        }


    }
}
