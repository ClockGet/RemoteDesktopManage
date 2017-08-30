using System.Drawing;
using System.Windows.Forms;

namespace 远程桌面管理器
{
    public class Drag
    {
        public static Control Dragged;
        static Cursor _move;
        public static Cursor DragCursorMove
        {
            get
            {
                if(_move==null)
                {
                    using (Bitmap bm = CursorUtil.AsBitmap(Dragged))
                    {
                        _move = CursorUtil.CreateCursor((Bitmap)bm.Clone(), 0, 0);
                    }
                }
                return _move;
            }
        }
        public static void StartDragging(Control c)
        {
            Dragged = c;
            DisposeOldCursors();
        }
        public static void StopDragging()
        {
            Dragged = null;
        }
        public static void UpdataCursor(object sender, GiveFeedbackEventArgs fea)
        {
            fea.UseDefaultCursors = false;
            if(fea.Effect==DragDropEffects.Move)
            {
                Cursor.Current = DragCursorMove;
            }
        }
        private static void DisposeOldCursors()
        {
            if(_move!=null)
            {
                _move.Dispose();
                _move = null;
            }
        }
    }
}
