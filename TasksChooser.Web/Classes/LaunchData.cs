using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amporis.TasksChooser;

namespace Amporis.TasksChooser.Web
{
    public class LaunchData
    {
        public string File { get; set; } // oauth_consumer_key
        public string Password { get; set; } // custom_password
        public bool AddHtmlCode { get; set; } // custom_html
        public bool AddCopyProtection { get; set; } // custom_nocopy
        public TaskSetting Settings { get; set; } = new TaskSetting();      
    }

    public class MoodleData
    {
        public string Key { get; set; } // oauth_consumer_key
        public string UserEmail { get; set; } // lis_person_contact_email_primary
        public string UserFullName { get; set; } // lis_person_name_full
        public string UserLogin { get; set; } // ext_user_username
        public string ReturnUrl { get; set; } // launch_presentation_return_url

        public string File { get; set; } // custom_file
        public string Password { get; set; } // custom_password
        public string Seed { get; set; } // custom_seed
        public string Round { get; set; } // custom_round
        public string Level { get; set; } // custom_level
        public string Count { get; set; } // custom_count
        public string TimeOut { get; set; } // custom_timeout
        public string RandomOrder { get; set; } // custom_random
        public string SeparatePreviousPairs { get; set; } // custom_separate
    }
}
