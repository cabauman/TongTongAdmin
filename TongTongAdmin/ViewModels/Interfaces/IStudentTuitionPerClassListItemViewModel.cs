using System;
using System.Collections.Generic;

namespace TongTongAdmin.ViewModels.Interfaces
{
    public interface IStudentTuitionPerClassListItemViewModel
    {
        DateTime Date { get; set; }

        string StudentName { get; set; }

        string TeacherNamesCsv { get; set; }

        uint ClassDuration { get; set; }

        uint HourlyRate { get; set; }

        uint TotalCost { get; set; }

        uint AmountPaid { get; set; }

        uint AmountUnpaid { get; set; }
    }
}
