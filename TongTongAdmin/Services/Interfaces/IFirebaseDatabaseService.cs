using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TongTongAdmin.Models;

namespace TongTongAdmin.Services.Interfaces
{
    public interface IFirebaseDatabaseService
    {
        Task<ObservableCollection<Course>> GetCourses();

        Task<ObservableCollection<CourseRegistration>> GetCourseRegistrations();

        Task<ObservableCollection<SyllabusItem>> GetSyllabusItems(string courseId);

        Task<ObservableCollection<ScheduleItem>> GetScheduleItems(string courseId);

        Task<ObservableCollection<User>> GetUsersThatHaveRegisteredForACourseBefore();

        Task<IReadOnlyList<Teacher>> GetTeachers();

        Task SaveCourse(Course course);

        Task SavePastCourse(PastCourse course, string uid);

        Task AddPastCourseSyllabusItem(SyllabusItem item, string uid, string courseId);

        Task SaveCourseRegistration(CourseRegistration registration);

        Task SaveSyllabusItem(SyllabusItem item, string courseId);

        Task SaveScheduleItem(ScheduleItem item, string courseId);

        Task SaveUser(User user);

        Task SaveNewUser(User user);

        Task SaveUserPartial(User user);

        Task DeleteCourse(Course course, bool updateUsers = true);

        Task DeletePastCourse(PastCourse course, string uid);

        Task DeleteSyllabusItem(SyllabusItem item, string courseId);

        Task DeleteScheduleItem(ScheduleItem item, string courseId);

        Task RemoveStudentFromCourse(User user, string courseId);

        Task DeleteUser(User user);

        Task StartNewSemester();

        Task AddToCourseHistory(Course course);

        Task<ObservableCollection<PastCourse>> GetPastCourses(string uid);

        Task RenewAllCourses();

        Task RenewCourse(Course course);
    }
}