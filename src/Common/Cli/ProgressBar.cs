﻿/*
 * Copyright 2006-2010 Simon E. Silva Lauinger, Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Properties;
using Common.Tasks;

namespace Common.Cli
{
    /// <summary>
    /// A progress bar rendered on the <see cref="Console"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "IDisposable is only implemented here to support using() blocks.")]
    public class ProgressBar : IDisposable
    {
        #region Properties
        private TaskState _state;

        /// <summary>
        /// The current status of the task.
        /// </summary>
        [Description("The current status of the task.")]
        public TaskState State
        {
            get { return _state; }
            set
            {
                #region Sanity checks
                if (!Enum.IsDefined(typeof(TaskState), value))
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(TaskState));
                #endregion

                try
                {
                    UpdateHelper.Do(ref _state, value, Draw);
                }
                catch (IOException)
                {}
            }
        }

        private int _maximum = 20;

        /// <summary>
        /// The maximum valid value for <see cref="Value"/>; must be greater than 0. Determines the length of the progress bar in console characters.
        /// </summary>
        [DefaultValue(20), Description("The maximum valid value for Value; must be greater than 0. Determines the length of the progress bar in console characters.")]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                #region Sanity checks
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", Resources.ArgMustBeGreaterThanZero);
                #endregion

                try
                {
                    UpdateHelper.Do(ref _maximum, value, Draw);
                }
                catch (IOException)
                {}

                if (Value > Maximum) Value = Maximum;
            }
        }

        private int _value;

        /// <summary>
        /// The progress of the task as a value between 0 and <see cref="Maximum"/>; -1 when unknown.
        /// </summary>
        [DefaultValue(0), Description("The progress of the task as a value between 0 and Maximum; -1 when unknown.")]
        public int Value
        {
            get { return _value; }
            set
            {
                #region Sanity checks
                if (value < -1 || value > Maximum)
                    throw new ArgumentOutOfRangeException("value");
                #endregion

                try
                {
                    UpdateHelper.Do(ref _value, value, Draw);
                }
                catch (IOException)
                {}
            }
        }
        #endregion

        //--------------------//

        #region Draw
        /// <summary>
        /// Draws the progress-bar to <see cref="Console.Error"/>.
        /// </summary>
        /// <remarks>The current line is overwritten.</remarks>
        /// <exception cref="IOException">Thrown if the progress bar could not be drawn to the <see cref="Console"/> (e.g. if it isn't a TTY).</exception>
        public void Draw()
        {
            // Don't draw to console if the stream has been redirected
            if (CliUtils.StandardOutputRedirected || CliUtils.StandardErrorRedirected) return;

            // Draw start of progress bar
            Console.CursorLeft = 0;
            Console.Error.Write('[');

            // Draw filled part
            Console.ForegroundColor = ConsoleColor.DarkGray;
            for (int i = 0; i < Value; i++)
                Console.Error.Write('=');
            Console.ResetColor();

            // Draw end of progress bar
            Console.CursorLeft = Maximum + 1;
            Console.Error.Write(']');

            // Write status
            Console.Error.Write(' ');
            switch (State)
            {
                case TaskState.Header:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Error.Write(Resources.StateHeader);
                    break;

                case TaskState.Data:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Error.Write(Resources.StateData);
                    break;

                case TaskState.Complete:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Error.Write(Resources.StateComplete);
                    break;

                case TaskState.WebError:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write(Resources.StateWebError);
                    break;

                case TaskState.IOError:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write(Resources.StateIOError);
                    break;
            }
            Console.ResetColor();

            // Blanks at the end to overwrite any excess
            Console.Error.Write(@"          ");
            Console.CursorLeft -= 10;
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Stops the progress bar by writing a line break to the <see cref="Console"/>.
        /// </summary>
        public virtual void Done()
        {
            if (!CliUtils.StandardOutputRedirected && !CliUtils.StandardErrorRedirected)
                Console.Error.WriteLine();
        }

        /// <summary>
        /// Stops the progress bar by writing a line break to the <see cref="Console"/>.
        /// </summary>
        void IDisposable.Dispose()
        {
            Done();
        }
        #endregion
    }
}
