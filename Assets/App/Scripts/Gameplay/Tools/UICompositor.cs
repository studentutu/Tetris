using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICompositor
{
    private readonly MyStack<BasePanel> mPanelStack;
    public UICompositor(MyStack<BasePanel> panelStack)
    {
        mPanelStack = panelStack;
    }

    public BasePanel PushPanel(BasePanel targetPanel)
    {
        if (targetPanel == mPanelStack.Peek())
        {
            return targetPanel;
        }
        mPanelStack.Remove(targetPanel);
        var tempPanel = mPanelStack.Peek();
        if (tempPanel != null)
        {
            //If the new window is closed, it does not need to be displayed again
            if (!tempPanel.stack)
            {
                mPanelStack.Pop();
            }
            //Whether you pop the stack or not, you need to hide it
            tempPanel.Hide();
        }
        targetPanel.Show();
        return mPanelStack.Push(targetPanel);
    }

    public void PopPanel()
    {
        mPanelStack.Pop()?.Hide();
        mPanelStack.Peek()?.Show();
    }

    public void ClearAllPanel()
    {
        mPanelStack.Peek()?.Hide();
        mPanelStack.Clear();
    }
}