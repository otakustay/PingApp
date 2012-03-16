using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingApp.Entity {
    public enum AppUpdateType {
        /// <summary>
        /// 新进应用
        /// </summary>
        New,

        /// <summary>
        /// 添加到站点
        /// </summary>
        AddToNote,
        
        /// <summary>
        /// 价格提升
        /// </summary>
        PriceIncrease,

        /// <summary>
        /// 价格下降
        /// </summary>
        PriceDecrease,

        /// <summary>
        /// 变为免费
        /// </summary>
        PriceFree,

        /// <summary>
        /// 更新版本
        /// </summary>
        NewRelease,

        /// <summary>
        /// 应用下架
        /// </summary>
        Off
    }
}
