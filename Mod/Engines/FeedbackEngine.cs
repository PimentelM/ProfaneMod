using Profane.Core.Chat.Enums;
using Profane.Core.Chat.Message;
using Profane.GUI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Engines
{
    public static class FeedbackEngine
    {

        public static void WriteMessage(string content)
        {
            Trace.WriteLine(content);

            return;
            MessageWindow.Current.Insert(MessageWindow.Current.CurrentGroup, new ChatMessageData(content, ChatMessageType.SYSTEM));

        }

        internal static void TraceException(Exception ex)
        {
            Trace.WriteLine(ex.Message);
            Trace.WriteLine(ex.StackTrace);
        }
    }
}
