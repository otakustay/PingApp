using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.LayoutRenderers;

namespace PingApp.Schedule.Infrastructure {
    [LayoutRenderer("NewLinedExceptionLayout")]
    class NewLinedExcecptionLayoutRenderer : ExceptionLayoutRenderer {
        protected override void Append(StringBuilder builder, NLog.LogEventInfo logEvent) {
            if (logEvent.Exception != null) {
                builder.AppendLine();
                base.Append(builder, logEvent);
            }
        }
    }
}
