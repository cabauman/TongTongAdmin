using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TongTongAdmin.Helpers;
using Windows.UI.Xaml.Media;

namespace TongTongAdmin.Models
{
    public class User : INotifyPropertyChanged
    {
        private string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("phoneNum")]
        public string PhoneNum { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("skypeId")]
        public string SkypeId { get; set; }

        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; set; }

        [JsonProperty("studyPoints")]
        public int StudyPoints { get; set; }

        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("instanceIdToken")]
        public string InstanceIdToken { get; set; }

        // ------------------------------------------

        [JsonIgnore]
        public bool PhotoUrlIsNull { get; set; }

        public async Task IsPhotoUrlNull()
        {
            if(string.IsNullOrEmpty(PhotoUrl))
            {
                PhotoUrlIsNull = true;
                return;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PhotoUrl);
            request.Method = "HEAD";

            try
            {
                var response = await request.GetResponseAsync() as HttpWebResponse;

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    PhotoUrlIsNull = false;
                }
                else
                {
                    PhotoUrlIsNull = true;
                }
            }
            catch
            {
                PhotoUrlIsNull = true;
            }
        }

        [JsonIgnore]
        public string Initials
        {
            get
            {
                var nameParts = Name.Split(' ');
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < Math.Min(nameParts.Length, 2); ++i)
                {
                    sb.Append(nameParts[i].Substring(0, 1).ToUpper());
                }

                return sb.ToString();
            }
        }

        [JsonIgnore]
        public Brush InitialsFillBrush
        {
            get { return BrushHelper.PickRandomDarkBrush(); }
        }

        [JsonIgnore]
        public uint AmountPaid
        {
            get
            {
                uint amountPaid = 0;
                Course.UidToAmountPaidDict?.TryGetValue(Uid, out amountPaid);

                return amountPaid;
            }
            set
            {
                if(Course.UidToAmountPaidDict == null)
                {
                    Course.UidToAmountPaidDict = new Dictionary<string, uint>(1);
                }

                Course.UidToAmountPaidDict[Uid] = value;
            }
        }

        [JsonIgnore]
        public Course Course { get; set; }

        [JsonProperty("courseId")]
        public string CourseId { get; set; }

        [JsonProperty("courseType")]
        public string CourseType { get; set; }

        [JsonProperty("nextSemesterCourseId")]
        public string NextSemesterCourseId { get; set; }

        [JsonProperty("nextSemesterCourseType")]
        public string NextSemesterCourseType { get; set; }

        [JsonProperty("didPayTuition")]
        public bool DidPayTuition { get; set; }

        [JsonProperty("didRegister")]
        public bool DidRegister { get; set; }

        [JsonProperty("hasCurrentOrUpcomingCourse")]
        public bool HasCurrentOrUpcomingCourse { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        public void RemoveCourse(bool thisSemester)
        {
            bool registeredForBothThisAndNextSemester = !string.IsNullOrEmpty(CourseId) &&
                                                        !string.IsNullOrEmpty(NextSemesterCourseId);

            if(!registeredForBothThisAndNextSemester)
            {
                HasCurrentOrUpcomingCourse = false;
            }

            if(thisSemester)
            {
                CourseId = null;
                CourseType = null;
            }
            else
            {
                NextSemesterCourseId = null;
                NextSemesterCourseType = null;
            }
        }

        public void StartNextSemester()
        {
            if(!string.IsNullOrEmpty(NextSemesterCourseId))
            {
                // User registered for next semester, so transfer that info to the upcoming semester.
                CourseId = NextSemesterCourseId;
                CourseType = NextSemesterCourseType;
                NextSemesterCourseId = null;
                NextSemesterCourseType = null;
            }
            else
            {
                // User didn't register for next semester, so just remove the course info.
                CourseId = null;
                CourseType = null;
                HasCurrentOrUpcomingCourse = false;
            }

            DidPayTuition = false;
        }
    }
}