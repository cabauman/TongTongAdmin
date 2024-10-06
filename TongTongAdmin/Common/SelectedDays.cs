using System;

namespace TongTongAdmin.Common
{
    [Flags]
    public enum SelectedDays
    {
        Mo = 1 << 0,
        Tu = 1 << 1,
        We = 1 << 2,
        Th = 1 << 3,
        Fr = 1 << 4,
        Sa = 1 << 5,
    }
}