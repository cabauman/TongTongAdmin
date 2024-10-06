using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TongTongAdmin.Models
{
    public interface ICourse
    {
        uint HourlyRate { get; }

        string Teachers { get; }

        ObservableCollection<User> StudentUsers { get; }

        ObservableCollection<SyllabusItem> SyllabusItems { get; }

        uint GetPaidTuition(string uid);
    }
}