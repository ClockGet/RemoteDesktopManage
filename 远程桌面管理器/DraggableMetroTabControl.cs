using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Wisder.W3Common.WMetroControl.Controls;

namespace 远程桌面管理器
{
    public class DragDoneEventArgs:EventArgs
    {
        private int x;
        private int y;
        private bool leave;
        public DragDoneEventArgs(int x,int y,bool leave)
        {
            this.x = x;
            this.y = y;
            this.leave = leave;
        }
        public int X
        {
            get
            {
                return x;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
        }
        public bool Leave
        {
            get
            {
                return leave;
            }
        }
    }
    public delegate void DragDoneHandler(object sender, DragDoneEventArgs args);
    public class DraggableMetroTabControl:MetroTabControl
    {
        private bool mouseDown = false;
        private bool isDragged = false;
        private Point originPoint;
        public event DragDoneHandler DragDone;
        public DraggableMetroTabControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint      //全部在窗口绘制消息中绘图
              | ControlStyles.OptimizedDoubleBuffer     //使用双缓冲
              | ControlStyles.UserPaint
              , true);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                originPoint = new Point(e.X, e.Y);
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown = false;
            if(isDragged)
            {
                isDragged = false;
                bool leave=GetTabPageByTab(new Point(e.X, e.Y)) == null;
                DragDone?.Invoke(Drag.Dragged, new DragDoneEventArgs(e.X, e.Y, leave));
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var distance = Math.Sqrt(Math.Pow((e.X - originPoint.X), 2) + Math.Pow((e.Y - originPoint.Y), 2));
            if(mouseDown && distance>10.0)
            {
                mouseDown = false;
                TabPage tabPage = GetTabPageByTab(new Point(e.X, e.Y));
                if (tabPage != null)
                {
                    Drag.StartDragging(tabPage);
                    isDragged = true;
                    this.DoDragDrop(tabPage, DragDropEffects.All);
                    Drag.StopDragging();
                }
            }

        }
        private TabPage GetTabPageByTab(Point point)
        {
            //排除首页
            for (int i = 1; i < this.TabPages.Count; i++)
            {
                if (GetTabRect(i).Contains(point))
                {
                    return this.TabPages[i];
                }
            }
            return null;
        }
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            TabPage source = (TabPage)drgevent.Data.GetData(typeof(TabPage));
            if(source!=null)
            {
                TabPage target = GetTabPageByTab(PointToClient(new Point(drgevent.X, drgevent.Y)));
                if(target!=null)
                {
                    drgevent.Effect = DragDropEffects.Move;
                    MoveTabPage(source, target);
                }
                else
                {
                    drgevent.Effect = DragDropEffects.Move;
                }
            }
            else
            {
                drgevent.Effect = DragDropEffects.Move;
            }
        }
        private void MoveTabPage(TabPage source, TabPage target)
        {
            if (source == target)
                return;
            int targetIndex = -1;
            List<TabPage> lstPages = new List<TabPage>();
            for (int i = 0; i < this.TabPages.Count; i++)
            {
                if (this.TabPages[i] == target)
                {
                    targetIndex = i;
                }
                if (this.TabPages[i] != source)
                {
                    lstPages.Add(this.TabPages[i]);
                }
            }
            this.TabPages.Clear();
            this.TabPages.AddRange(lstPages.ToArray());
            this.TabPages.Insert(targetIndex, source);
            this.SelectedTab = source;
        }
        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            base.OnQueryContinueDrag(qcdevent);
            Rectangle rect = new Rectangle(this.Location, new Size(this.Width, this.ItemSize.Height));
            if (Control.MouseButtons != MouseButtons.Left && !rect.Contains(PointToClient(Control.MousePosition)))
            {
                qcdevent.Action = DragAction.Cancel;
                Point point = PointToClient(Control.MousePosition);
                OnMouseUp(new MouseEventArgs(Control.MouseButtons, 0, point.X, point.Y, 0));
            }
        }
    }
}
