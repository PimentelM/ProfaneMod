using Profane.Core.Chat.Enums;
using Profane.Core.Chat.Message;
using Profane.GUI.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.ScriptingEngine
{
    class Writter
    {
        public class ScriptOutputStream : Stream
        {


            #region Constructors
            public ScriptOutputStream()
            {
            }
            #endregion

            #region Properties
            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get { throw new NotImplementedException(); }
            }

            public override long Position
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
            #endregion

            #region Exposed Members
            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var msg =  Encoding.GetEncoding(1252).GetString(buffer, offset, count);
                MessageWindow.Current.Insert(MessageWindow.Current.CurrentGroup, new ChatMessageData(msg, ChatMessageType.COMMAND));
            }
            #endregion
        }
    }
}
