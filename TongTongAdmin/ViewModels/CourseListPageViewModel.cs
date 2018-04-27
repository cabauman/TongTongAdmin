using System;
using System.Collections.Generic;
using System.ComponentModel;
using ReactiveUI;
using TongTongAdmin.ViewModels.Interfaces;

namespace TongTongAdmin.ViewModels
{
    public class CourseListPageViewModel : BaseViewModel, ICourseListPageViewModel
    {
        public CourseListPageViewModel()
        {
        }

        public CourseListPageViewModel(IScreen screen, string param)
        {
            HostScreen = screen;
        }

        public string UrlPathSegment
        {
            get { return "course-list-page"; }
        }

        public IScreen HostScreen { get; }
    }
}
