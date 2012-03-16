using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Schedule.Task {
    enum DbCheckType {
        /// <summary>
        /// 只添加，不检查重复
        /// </summary>
        ForceInsert,

        /// <summary>
        /// 检查添加或更新
        /// </summary>
        CheckForUpdate,

        /// <summary>
        /// 只添加，检查重复
        /// </summary>
        DiscardUpdate
    }
}
