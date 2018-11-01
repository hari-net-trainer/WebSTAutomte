using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAutomate
{

    public class Rootobject
    {
        public D d { get; set; }
    }

    public class D
    {
        public string __type { get; set; }
        public Todaytasklist[] TodayTaskLists { get; set; }
        public bool IsClearContent { get; set; }
        public string AlertMsg { get; set; }
        public object Massage { get; set; }
        public int ClickCount { get; set; }
        public bool RequestForTodayTask { get; set; }
    }

    public class Todaytasklist
    {
        public string WorkID { get; set; }
        public string Stage { get; set; }
        public string Mandatory { get; set; }
        public string Serialnumber { get; set; }
        public string CampaignID { get; set; }
        public string CampaignName { get; set; }
        public string Link { get; set; }
        public string CampaignTypeID { get; set; }
        public object Flag { get; set; }
    }


    public class WorkPayLoad
    {
        public string pageNumber { get; set; } = "1";
        public string pageSize { get; set; } = "20";
        public string userId { get; set; }
    }

    public class UserPayLoad
    {
        public string Username { get; set; }
        public int WorkID { get; set; }
        public string CurrentFlag { get; set; }
        public int PointsType { get; set; }
        public int Password { get; set; }
        public string Flag { get; set; }
        public string NextWorkID { get; set; }
    }

    public class ResponseObject
    {
        public string __type { get; set; }
        public string Username { get; set; }
        public string WorkID { get; set; }
        public string CurrentFlag { get; set; }
        public string PointsType { get; set; }
        public string Password { get; set; }
        public string Flag { get; set; }
        public string NextWorkID { get; set; }
        public bool IsUpdate { get; set; }
        public int TaskCount { get; set; }
        public object FreePoints { get; set; }
        public object PaidPoints { get; set; }
        public object CampaignID { get; set; }
    }

}
