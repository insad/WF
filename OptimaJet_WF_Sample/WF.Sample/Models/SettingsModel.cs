﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WF.Sample.Business;

namespace WF.Sample.Models
{
    public class SettingsModel
    {
        [AllowHtml]
        public string WFSchema { get; set; }

        public List<Employee> Employees { get; set; }
        public List<Role> Roles { get; set; }
        public List<StructDivision> StructDivision { get; set; }
    }
}