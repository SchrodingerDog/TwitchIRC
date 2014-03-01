using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchIRC
{
    /// <summary>
    /// 
    /// </summary>
    public static class ControlExtensions
    {
        private static Dictionary<Control, bool> draggables =
                   new Dictionary<Control, bool>();
        private static System.Drawing.Size mouseOffset;

        /// <summary>
        /// Enabling/disabling dragging for control
        /// </summary>
        public static void Draggable(this Control control, bool Enable)
        {
            if (Enable)
            {
                // enable drag feature
                if (draggables.ContainsKey(control))
                {   // return if control is already draggable
                    return;
                }
                // 'false' - initial state is 'not dragging'
                draggables.Add(control, false);

                // assign required event handlersnnn
                control.MouseDown += new MouseEventHandler(control_MouseDown);
                control.MouseUp += new MouseEventHandler(control_MouseUp);
                control.MouseMove += new MouseEventHandler(control_MouseMove);
            }
            else
            {
                // disable drag feature
                if (!draggables.ContainsKey(control))
                {  // return if control is not draggable
                    return;
                }
                // remove event handlers
                control.MouseDown -= control_MouseDown;
                control.MouseUp -= control_MouseUp;
                control.MouseMove -= control_MouseMove;
                draggables.Remove(control);
            }
        }
        static void control_MouseDown(object sender, MouseEventArgs e)
        {
            mouseOffset = new System.Drawing.Size(e.Location);
            // turning on dragging
            draggables[(Control)sender] = true;
        }
        static void control_MouseUp(object sender, MouseEventArgs e)
        {
            // turning off dragging
            draggables[(Control)sender] = false;
        }
        static void control_MouseMove(object sender, MouseEventArgs e)
        {
            // only if dragging is turned on
            if (draggables[(Control)sender] == true)
            {
                // calculations of control's new position
                System.Drawing.Point newLocationOffset = e.Location - mouseOffset;

                try
                {
                    ((Control)sender).Left += newLocationOffset.X;

                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    throw;
                }
                catch (Exception) { }
                try
                {

                    ((Control)sender).Top += newLocationOffset.Y;
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    throw;
                }
                catch (Exception) { }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <param name="action"></param>
        /// <param name="parameter"></param>
        public static void InvokeIfRequired<T>(this Control control, Action<T> action, T parameter)
        {
            if (control.InvokeRequired)
                control.Invoke(action, parameter);
            else
                action(parameter);
        }
    }
}
