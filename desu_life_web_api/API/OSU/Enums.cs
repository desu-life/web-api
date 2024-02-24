using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebAPI.API.OSU;

public static class Enums
{
    public static string Mode2String(Mode mode)
    {
        return mode switch
        {
            Mode.OSU => "osu",
            Mode.Taiko => "taiko",
            Mode.Fruits => "fruits",
            Mode.Mania => "mania",
            _ => throw new NotSupportedException("未知的模式"),
        };
    }

    public static Mode? String2Mode(string? value)
    {
        value = value?.ToLower();
        return value switch
        {
            "osu" => Mode.OSU,
            "taiko" => Mode.Taiko,
            "fruits" => Mode.Fruits,
            "mania" => Mode.Mania,
            _ => null
        };
    }

    public static Mode? Int2Mode(int value)
    {
        return value switch
        {
            0 => Mode.OSU,
            1 => Mode.Taiko,
            2 => Mode.Fruits,
            3 => Mode.Mania,
            _ => null
        };
    }

    // 枚举部分
    [DefaultValue(Unknown)] // 解析失败就unknown
    public enum Mode
    {
        /// <summary>
        /// 未知，在转换错误时为此值
        /// </summary>
        [Description("")]
        Unknown,

        [Description("osu")]
        OSU,

        [Description("taiko")]
        Taiko,

        [Description("fruits")]
        Fruits,

        [Description("mania")]
        Mania,
    }

    // 成绩类型，用作API查询
    // 共可以是 best, firsts, recent
    // 默认为best（bp查询）
    [DefaultValue(Best)]
    public enum UserScoreType
    {
        [Description("best")]
        Best,

        [Description("firsts")]
        Firsts,

        [Description("recent")]
        Recent,
    }

    [DefaultValue(Unknown)]
    public enum Status
    {
        /// <summary>
        /// 未知，在转换错误时为此值
        /// </summary>
        [Description("")]
        Unknown,

        [Description("graveyard")]
        Graveyard,

        [Description("wip")]
        WIP,

        [Description("pending")]
        pending,

        [Description("ranked")]
        ranked,

        [Description("approved")]
        approved,

        [Description("qualified")]
        qualified,

        [Description("loved")]
        loved
    }
}
