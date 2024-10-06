using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TongTongAdmin.Models;
using TongTongAdmin.Services.Interfaces;

namespace TongTongAdmin.Services
{
    public sealed class FirebaseDatabaseService : IFirebaseDatabaseService
    {
        private const string BASE_URL = "https://tt-korean-academy.firebaseio.com/";
        private const string PATH_USERS = "users";
        private const string PATH_USER = "users/{0}";
        private const string PATH_COURSES = "courses";
        private const string PATH_COURSE = "courses/{0}";
        private const string PATH_PAST_COURSES = "courses-past/{0}";
        private const string PATH_PAST_COURSE = "courses-past/{0}/{1}";                     // uid/courseId
        private const string PATH_PAST_COURSE_SYLLABUS = "courses-past/{0}/{1}/syllabus";   // uid/courseId/syllabus
        private const string PATH_COURSE_REGISTRATIONS = "course-registrations";
        private const string PATH_COURSE_REGISTRATION = "course-registrations/{0}";
        private const string PATH_COURSE_STUDENT = "courses/{0}/students/{1}";
        private const string PATH_SYLLABUS = "syllabuses/{0}";
        private const string PATH_SYLLABUS_ITEM = "syllabuses/{0}/{1}";
        private const string PATH_SCHEDULE = "schedules/{0}";
        private const string PATH_SCHEDULE_ITEM = "schedules/{0}/{1}";
        private const string PATH_TEACHERS = "teachers";

        private readonly FirebaseClient _client;
        private readonly Dictionary<string, ObservableCollection<SyllabusItem>> _syllabusItemsByCourseId;
        private readonly Dictionary<string, ObservableCollection<ScheduleItem>> _scheduleItemsByCourseId;
        private readonly Dictionary<string, ObservableCollection<PastCourse>> _pastCoursesByUid;

        private ObservableCollection<Course> _courses;
        private ObservableCollection<CourseRegistration> _courseRegistrations;
        private ObservableCollection<Models.User> _usersThatHaveRegisteredForACourseBefore;
        private IReadOnlyList<Teacher> _teachers;

        public FirebaseDatabaseService(string authToken)
        {
            _client = new FirebaseClient(
                BASE_URL,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(authToken)
                });

            _syllabusItemsByCourseId = new Dictionary<string, ObservableCollection<SyllabusItem>>();
            _scheduleItemsByCourseId = new Dictionary<string, ObservableCollection<ScheduleItem>>();
            _pastCoursesByUid = new Dictionary<string, ObservableCollection<PastCourse>>();
        }


        // ====================================== GET DATA ====================================== //

        public async Task<ObservableCollection<Course>> GetCourses()
        {
            if(_courses == null)
            {
                await LoadCourses();
            }

            return _courses;
        }

        public async Task<ObservableCollection<PastCourse>> GetPastCourses(string uid)
        {
            ObservableCollection<PastCourse> pastCourses = null;
            bool found = _pastCoursesByUid.TryGetValue(uid, out pastCourses);
            if(!found)
            {
                pastCourses = await LoadPastCourses(uid);
                _pastCoursesByUid.Add(uid, pastCourses);
            }

            return pastCourses;
        }

        public async Task<ObservableCollection<CourseRegistration>> GetCourseRegistrations()
        {
            if(_courseRegistrations == null)
            {
                await LoadCourseRegistrations();
            }

            return _courseRegistrations;
        }

        public async Task<ObservableCollection<SyllabusItem>> GetSyllabusItems(string courseId)
        {
            ObservableCollection<SyllabusItem> syllabusItems = null;
            bool found = _syllabusItemsByCourseId.TryGetValue(courseId, out syllabusItems);
            if(!found)
            {
                syllabusItems = await LoadSyllabusItems(courseId);
                _syllabusItemsByCourseId.Add(courseId, syllabusItems);
            }

            return syllabusItems;
        }

        public async Task<ObservableCollection<ScheduleItem>> GetScheduleItems(string courseId)
        {
            ObservableCollection<ScheduleItem> scheduleItems = null;
            bool found = _scheduleItemsByCourseId.TryGetValue(courseId, out scheduleItems);
            if(!found)
            {
                scheduleItems = await LoadScheduleItems(courseId);
                _scheduleItemsByCourseId.Add(courseId, scheduleItems);
            }

            return scheduleItems;
        }

        public async Task<ObservableCollection<Models.User>> GetUsersThatHaveRegisteredForACourseBefore()
        {
            if(_usersThatHaveRegisteredForACourseBefore == null)
            {
                await LoadUsersThatHaveRegisteredForACourseBefore();
            }

            return _usersThatHaveRegisteredForACourseBefore;
        }

        public async Task<IReadOnlyList<Teacher>> GetTeachers()
        {
            if(_teachers == null)
            {
                await LoadTeachers();
            }

            return _teachers;
        }


        // ====================================== SAVE DATA ====================================== //

        public static void Patch<T>(T obj, string patch)
        {
            var serializer = new JsonSerializer();
            using(var reader = new StringReader(patch))
            {
                serializer.Populate(reader, obj);
            }
        }

        public async Task SaveCourse(Course course)
        {
            if(course.Id == null)
            {
                var addedCourseObj = await _client
                    .Child(PATH_COURSES)
                    .PostAsync(course);

                course.Id = addedCourseObj.Key;
                var dict = new Dictionary<string, string> { { "id", course.Id } };

                string path = string.Format(PATH_COURSE, course.Id);
                await _client
                    .Child(path)
                    .PatchAsync(dict);
            }
            else
            {
                string path = string.Format(PATH_COURSE, course.Id);
                await _client
                    .Child(path)
                    .PutAsync(course);
            }
        }

        public async Task SaveNewCourse(Course course)
        {
            var addedCourseObj = await _client
                .Child(PATH_COURSES)
                .PostAsync(course);

            course.Id = addedCourseObj.Key;
            var dict = new Dictionary<string, string> { { "id", course.Id } };

            string path = string.Format(PATH_COURSE, course.Id);
            await _client
                .Child(path)
                .PatchAsync(dict);

            _courses.Add(course);
        }

        public async Task AddPastCourseSyllabusItem(SyllabusItem item, string uid, string courseId)
        {
            string path = string.Format(PATH_PAST_COURSE_SYLLABUS, uid, courseId);
            var result = await _client
                .Child(path)
                .PostAsync(item);

            item.Id = result.Key;
        }

        public async Task SavePastCourse(PastCourse course, string uid)
        {
            string path = string.Format(PATH_PAST_COURSE, uid, course.Id);
            await _client
                .Child(path)
                .PutAsync(course);
        }

        public async Task SaveCourseRegistration(CourseRegistration registration)
        {
            if(string.IsNullOrEmpty(registration.Uid))
            {
                // Dangerous because it would overwrite all the other registrations.
                return;
            }

            string path = string.Format(PATH_COURSE_REGISTRATION, registration.Uid);
            await _client
                .Child(path)
                .PutAsync(registration);
        }

        public async Task SaveSyllabusItem(SyllabusItem item, string courseId)
        {
            if(string.IsNullOrEmpty(courseId))
            {
                // Dangerous because it would overwrite all the other syllabus items.
                return;
            }

            if(item.Id == null)
            {
                string path = string.Format(PATH_SYLLABUS, courseId);
                var result = await _client
                    .Child(path)
                    .PostAsync(item);

                item.Id = result.Key;
            }
            else
            {
                string path = string.Format(PATH_SYLLABUS_ITEM, courseId, item.Id);
                await _client
                    .Child(path)
                    .PutAsync(item);
            }
        }

        public async Task SaveScheduleItem(ScheduleItem item, string courseId)
        {
            if(item.Id == null)
            {
                string path = string.Format(PATH_SCHEDULE, courseId);
                var result = await _client
                    .Child(path)
                    .PostAsync(item);

                item.Id = result.Key;
            }
            else
            {
                string path = string.Format(PATH_SCHEDULE_ITEM, courseId, item.Id);
                await _client
                    .Child(path)
                    .PutAsync(item);
            }
        }

        public async Task SaveUser(Models.User user)
        {
            string path = string.Format(PATH_USER, user.Uid);
            await _client
                .Child(path)
                .PutAsync(user);
        }

        public async Task SaveUserPartial(Models.User user)
        {
            var dict = new Dictionary<string, object>
            {
                { "didPayTuition", user.DidPayTuition },
                { "notes", user.Notes },
            };

            string path = string.Format(PATH_USER, user.Uid);
            await _client
                .Child(path)
                .PatchAsync(dict);

            //if(user.Course != null)
            //{
            //    await SaveCourse(user.Course);
            //}
        }

        public async Task SaveNewUser(Models.User user)
        {
            var addedUserObj = await _client
                .Child(PATH_USERS)
                .PostAsync(user);

            user.Uid = addedUserObj.Key;
            var dict = new Dictionary<string, string> { { "uid", user.Uid } };

            string path = string.Format(PATH_USER, user.Uid);
            await _client
                .Child(path)
                .PatchAsync(dict);

            _usersThatHaveRegisteredForACourseBefore.Add(user);
        }


        // ====================================== OTHER ====================================== //

        public async Task StartNewSemester()
        {
            List<Task> tasks = new List<Task>();

            var thisSemesterCourses = _courses
                .Where(course => course.ThisSemester == true);

            foreach(var course in thisSemesterCourses)
            {
                tasks.Add(AddToCourseHistory(course));
            }

            await Task.WhenAll(tasks);
            tasks.Clear();

            // Delete the courses that just ended and set the upcoming courses as current.
            foreach(var course in _courses.ToList())
            {
                if(course.ThisSemester)
                {
                    _courses.Remove(course);
                    // We don't need to remove/update the users, because that's taken care of, above.
                    tasks.Add(DeleteCourse(course, false));
                }
                else
                {
                    course.ThisSemester = true;
                    tasks.Add(SaveCourse(course));
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task AddToCourseHistory(Course course)
        {
            // Need to load the syllabus items directly, instead of via the course property,
            // because they may not have been loaded yet.
            var syllabusItems = await GetSyllabusItems(course.Id);
            var pastCourse = new PastCourse(course, syllabusItems);

            var students = _usersThatHaveRegisteredForACourseBefore
                .Where(user => user.CourseId == course.Id);

            List<Task> tasks = new List<Task>();
            foreach(var student in students)
            {
                pastCourse.AmountPaid = course.UidToAmountPaidDict[student.Uid];

                string coursePath = string.Format(PATH_PAST_COURSE, student.Uid, course.Id);
                Task pastCourseTask = _client
                    .Child(coursePath)
                    .PutAsync(pastCourse);

                tasks.Add(pastCourseTask);

                student.StartNextSemester();
                tasks.Add(SaveUser(student));
            }

            await Task.WhenAll(tasks);
        }

        public async Task RenewAllCourses()
        {
            var thisSemesterCourses = _courses
                .Where(course => course.ThisSemester);

            List<Task> tasks = new List<Task>();
            foreach(var course in thisSemesterCourses)
            {
                tasks.Add(RenewCourse(course));
            }

            await Task.WhenAll(tasks);
        }

        public async Task RenewCourse(Course course)
        {
            var courseCopy = new Course(course);
            courseCopy.ThisSemester = false;
            await SaveNewCourse(courseCopy);

            var students = _usersThatHaveRegisteredForACourseBefore
                .Where(user => user.CourseId == course.Id);
            courseCopy.StudentUsers = new ObservableCollection<User>(students);

            List<Task> tasks = new List<Task>();
            foreach(var student in courseCopy.StudentUsers)
            {
                // Make sure the user isn't already registered for next semester.
                if(string.IsNullOrEmpty(student.NextSemesterCourseId))
                {
                    student.NextSemesterCourseId = courseCopy.Id;
                    student.NextSemesterCourseType = student.CourseType;
                    tasks.Add(SaveUser(student));
                }
            }

            var scheduleItems = await GetScheduleItems(course.Id);
            courseCopy.ScheduleItems = new ObservableCollection<ScheduleItem>(scheduleItems.Select(x => new ScheduleItem(x)));
            _scheduleItemsByCourseId.Add(courseCopy.Id, courseCopy.ScheduleItems);
            // Write the whole dictionary, instead of looping through each schedule item.
            var scheduleCopyDict = courseCopy.ScheduleItems.ToDictionary(x => x.Id);
            //var scheduleCopyJson = JsonConvert.SerializeObject(scheduleCopyDict);

            string path = string.Format(PATH_SCHEDULE, courseCopy.Id);
            Task scheduleTask = _client
                .Child(path)
                .PutAsync(scheduleCopyDict);

            tasks.Add(scheduleTask);
            await Task.WhenAll(tasks);
        }


        // ====================================== DELETE DATA ====================================== //

        public async Task DeleteCourse(Course course, bool updateUsers = true)
        {
            List<Task> tasks = new List<Task>();

            string path = string.Format(PATH_COURSE, course.Id);
            Task courseTask = _client
                .Child(path)
                .DeleteAsync();

            path = string.Format(PATH_SYLLABUS, course.Id);
            Task syllabusTask = _client
                .Child(path)
                .DeleteAsync();

            path = string.Format(PATH_SCHEDULE, course.Id);
            Task scheduleTask = _client
                .Child(path)
                .DeleteAsync();

            tasks.Add(courseTask);
            tasks.Add(syllabusTask);
            tasks.Add(scheduleTask);

            if(updateUsers)
            {
                foreach(var user in course.StudentUsers)
                {
                    user.RemoveCourse(course.ThisSemester);

                    path = string.Format(PATH_USER, user.Uid);
                    Task userTask = _client
                        .Child(path)
                        .PutAsync(user);

                    tasks.Add(userTask);
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task DeletePastCourse(PastCourse course, string uid)
        {
            string path = string.Format(PATH_PAST_COURSE, uid, course.Id);
            await _client
                .Child(path)
                .DeleteAsync();
        }

        public async Task DeleteSyllabusItem(SyllabusItem item, string courseId)
        {
            string path = string.Format(PATH_SYLLABUS_ITEM, courseId, item.Id);
            await _client
                .Child(path)
                .DeleteAsync();
        }

        public async Task DeleteScheduleItem(ScheduleItem item, string courseId)
        {
            string path = string.Format(PATH_SCHEDULE_ITEM, courseId, item.Id);
            await _client
                .Child(path)
                .DeleteAsync();
        }

        public async Task RemoveStudentFromCourse(Models.User user, string courseId)
        {
            string path = string.Format(PATH_COURSE_STUDENT, courseId, user.Uid);
            Task courseTask = _client
                .Child(path)
                .DeleteAsync();

            path = string.Format(PATH_USER, user.Uid);
            Task userTask = _client
                .Child(path)
                .PutAsync(user);

            await Task.WhenAll(courseTask, userTask);
        }

        public async Task DeleteUser(Models.User user)
        {
            string path = string.Format(PATH_USER, user.Uid);
            Task userTask = _client
                .Child(path)
                .DeleteAsync();

            path = string.Format(PATH_COURSE_REGISTRATION, user.Uid);
            Task registrationTask = _client
                .Child(path)
                .DeleteAsync();

            await Task.WhenAll(userTask, registrationTask);
        }


        // ====================================== LOAD DATA ====================================== //

        private async Task LoadCourses()
        {
            var items = await _client
                .Child(PATH_COURSES)
                .OnceAsync<Course>();

            _courses = new ObservableCollection<Course>(
                items.Select(item =>
                {
                    item.Object.Id = item.Key;

                    if(item.Object.Students?.Count > 0)
                    {
                        item.Object.StudentCsv = string.Join(", ", item.Object.Students.Select(x => x.Value));
                    }
                    else
                    {
                        item.Object.StudentCsv = "No Students";
                        item.Object.Students = new Dictionary<string, string>(1);
                    }

                    return item.Object;
                }));
        }

        private async Task<ObservableCollection<PastCourse>> LoadPastCourses(string uid)
        {
            string path = string.Format(PATH_PAST_COURSES, uid);
            var items = await _client
                .Child(path)
                .OnceAsync<PastCourse>();

            return new ObservableCollection<PastCourse>(
                items.Select(item =>
                {
                    item.Object.Id = item.Key;
                    var users = _usersThatHaveRegisteredForACourseBefore
                        .Where(user => user.Uid == uid);
                    item.Object.StudentUsers = new ObservableCollection<User>(users);

                    foreach(var syllabusItem in item.Object.Syllabus)
                    {
                        syllabusItem.Value.Id = syllabusItem.Key;
                        syllabusItem.Value.ToTime = null;
                    }

                    return item.Object;
                }));
        }

        private async Task LoadCourseRegistrations()
        {
            var items = await _client
                .Child(PATH_COURSE_REGISTRATIONS)
                .OnceAsync<CourseRegistration>();

            _courseRegistrations = new ObservableCollection<CourseRegistration>(
                items.Select(item =>
                {
                    item.Object.Uid = item.Key;
                    return item.Object;
                }));
        }

        private async Task<ObservableCollection<SyllabusItem>> LoadSyllabusItems(string courseId)
        {
            string path = string.Format(PATH_SYLLABUS, courseId);
            var items = await _client
                .Child(path)
                .OnceAsync<SyllabusItem>();

            return new ObservableCollection<SyllabusItem>(
                items.Select(item =>
                {
                    item.Object.Id = item.Key;
                    return item.Object;
                }));
        }

        private async Task<ObservableCollection<ScheduleItem>> LoadScheduleItems(string courseId)
        {
            string path = string.Format(PATH_SCHEDULE, courseId);
            var items = await _client
                .Child(path)
                .OnceAsync<ScheduleItem>();

            return new ObservableCollection<ScheduleItem>(
                items.Select(item =>
                {
                    // The database entries may not have an Id property, so let's create one. 
                    item.Object.Id = item.Key;
                    item.Object.InitTeacherOptions(_teachers);
                    item.Object.InitDays();
                    return item.Object;
                }));
        }

        private async Task LoadUsersThatHaveRegisteredForACourseBefore()
        {
            if(_courseRegistrations == null)
            {
                await LoadCourseRegistrations();
            }

            var usersToLoad = new List<Task<Models.User>>();
            foreach(var registration in _courseRegistrations)
            {
                string path = string.Format(PATH_USER, registration.Uid);
                var task = _client
                    .Child(path)
                    .OnceSingleAsync<Models.User>();

                usersToLoad.Add(task);
            }

            var taskList = Task.WhenAll(usersToLoad);

            Models.User[] users = await taskList;
            _usersThatHaveRegisteredForACourseBefore = new ObservableCollection<Models.User>(users);
        }

        private async Task LoadTeachers()
        {
            var items = await _client
                .Child(PATH_TEACHERS)
                .OnceAsync<Teacher>();

            _teachers = new List<Teacher>(items.Select(item => item.Object));
        }
    }
}