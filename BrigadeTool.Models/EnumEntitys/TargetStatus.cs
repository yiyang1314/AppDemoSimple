using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.Brigade.Models.EnumEntitys
{
    public enum TargetStatus
    {
        Normal,     // 默认/静止 - 白色
        Moving,     // 运动 - 绿色
        Danger,     // 危险 - 红色
        Selected    // 选中 - 黄色（优先级最高）
    }
}
