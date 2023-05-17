using System;
using System.Collections.Generic;

namespace Client
{
    
    [Serializable]
    public class JengaStackData
    {
        public List<JengaBlockData> jengaStackData;
    }
    
    [Serializable]
    public class JengaBlockData
    {
        public int id;
        public string subject;
        public string grade;
        public int mastery;
        public string domainid;
        public string domain;
        public string cluster;
        public string standardid;
        public string standarddescription;
    }
}