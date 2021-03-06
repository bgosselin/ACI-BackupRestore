﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACI_GetTenants.Models
{
    /// <summary>
    /// This is a model used for the RollBack page MVC view
    /// It collects a list of all tenants which have a SnapShot record saved
    /// and a list of all the SnapShots including the comments, GUID and checkin data
    /// </summary>
    public class TenantSnapShots
    {
        public List<SnapShotComment> CommentsByTenant { get; set; }
        public List<Tenant> Tenants { get; set; }
    }

    public class SnapShotComment
    {
        public SnapShotComment(string setGuid, string setComment)
        {
            guid = setGuid;
            comment = setComment;
        }
        public string guid { get; set; }
        public string comment { get; set; }
    }
    
    public class Tenant
    {
        public Tenant(string name, bool isSelected) 
        {
            tenantName = name;
            selected = isSelected;
        }
        public string tenantName { get; set; }
        public bool selected { get; set; }
    }
}