using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedoBoost : PowerUpBase
{
    private Stack<ICommand> undoStack = new Stack<ICommand>();
    public bool CanUndo => undoStack.Count > 0;

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        undoStack.Push(command);
    }

    public bool Undo()
    {
        if (!CanUndo) return false;
        ICommand command = undoStack.Pop();
        Debug.Log("thuc hien thuc undo"+undoStack.Count);
        command.Undo();
        return true;
    }

    protected override void ExecutePowerUp() => Undo();

}