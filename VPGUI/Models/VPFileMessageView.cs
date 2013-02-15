using System;
using VPSharp;

namespace VPGUI.Models
{
    public class VPFileMessageView
    {
        private VPFileMessage msg;

        public VPFileMessageView(VPFileMessage msg)
        {
            this.msg = msg;
        }

        public MessageType Type
        {
            get { return this.msg.Type; }
        }

        public string Message
        {
            get { return this.msg.Message; }
        }
    }
}
