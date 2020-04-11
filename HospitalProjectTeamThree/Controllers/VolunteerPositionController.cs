﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Net;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using HospitalProjectTeamThree.Models;
using HospitalProjectTeamThree.Models.ViewModels;
using HospitalProjectTeamThree.Data;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security; 

namespace HospitalProjectTeamThree.Controllers
{
    public class VolunteerPositionController : Controller
    {
        //will be using managers to deal with login 
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        private HospitalProjectTeamThreeContext db = new HospitalProjectTeamThreeContext();
        public VolunteerPositionController() { }
        // GET: VolunteerPosition
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Editor"))
                {
                    return RedirectToAction("List");
                }
                else
                {
                    return RedirectToAction("Add");
                }
            }
            else
            {
                return View();
            }
        }
        public ActionResult List (string volunteerpositionsearchkey, int pagenum = 0)
        {
            Debug.WriteLine("we are searching for " + volunteerpositionsearchkey);

            string query = "Select * from VolunteerPositions";
            List<SqlParameter> sqlparams = new List<SqlParameter>();

            //  if(jobsearchkey!="")
            // {
            //    query = query + "where JobTitle like @searchkey";
            //    sqlparams.Add(new SqlParameter("@searchkey", "%" + jobsearchkey + "%"));
            //  Debug.WriteLine("updated search should be looking for" + query);
            // }

            List<VolunteerPosition> volunteerPositions = db.VolunteerPositions.SqlQuery(query, sqlparams.ToArray()).ToList();

            //pagination
            int perpage = 5;
            int positioncount = volunteerPositions.Count();
            int maxpage = (int)Math.Ceiling((decimal)positioncount / perpage) - 1;
            if (maxpage < 0) maxpage = 0;
            if (pagenum < 0) pagenum = 0;
            if (pagenum > maxpage) pagenum = maxpage;
            int start = (int)(perpage * pagenum);
            ViewData["pagenum"] = pagenum;
            ViewData["pagesummary"] = "";
            if (maxpage > 0)
            {
                ViewData["pagesummary"] = (pagenum + 1) + "of" + (maxpage + 1);
                List<SqlParameter> newparams = new List<SqlParameter>();

                // if (jobsearchkey!="")
                // {
                //   newparams.Add(new SqlParameter("@searchkey", "%" + jobsearchkey + "%"));
                // ViewData["jobsearchkey"] = jobsearchkey;
                // }
                newparams.Add(new SqlParameter("@start", start));
                newparams.Add(new SqlParameter("@perpage", perpage));
                string pagedquery = query + "order by VolunteerPositionID offset @start rows fetch first @perpage rows only";
                Debug.WriteLine(pagedquery);
                Debug.WriteLine("offset" + start);
                Debug.WriteLine("fetch first" + perpage);

                volunteerPositions = db.VolunteerPositions.SqlQuery(pagedquery, newparams.ToArray()).ToList();
            }

            return View(volunteerPositions);
        }
        public ActionResult Add()
        {
            List<Department> Departments = db.Departments.SqlQuery("select * from departments").ToList();
            
            AddVolunteerPosition viewModel = new AddVolunteerPosition();
            viewModel.Departments = Departments;
           
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Add(string VolunteerPositionTitle, string VolunteerPositionDescription, DateTime StartDate, int DepartmentID)
        {
            string query = "insert into VolunteerPositions (VolunteerPositionTitle, VolunteerPositionDescription, StartDate, DepartmentID)values (@VolunteerPositionTitle, @VolunteerPositionDescription, @StartDate, @DepartmentID)";
            SqlParameter[] sqlparams = new SqlParameter[4];

            sqlparams[0] = new SqlParameter("@VolunteerPositionTitle", VolunteerPositionTitle);
            sqlparams[1] = new SqlParameter("@VolunteerPositionDescription", VolunteerPositionDescription);
            sqlparams[2] = new SqlParameter("@StartDate", StartDate);
            sqlparams[3] = new SqlParameter("@DepartmentID", DepartmentID);

            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("List");
        }
        public ActionResult Show(int? id)
        {
            VolunteerPosition volunteerPosition = db.VolunteerPositions.SqlQuery("select * from VolunteerPositions where VolunteerPositionID = @VolunteerPositionID", new SqlParameter("@VolunteerPositionID", id)).FirstOrDefault();
            List<Department> department = db.Departments.SqlQuery("select * from Departments inner join VolunteerPositions on VolunteerPositions.DepartmentID = Departments.DepartmentID where VolunteerPositionID = @id", new SqlParameter("@id", id)).ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (volunteerPosition == null)
            {
                return HttpNotFound();
            }
            ShowVolunteerPosition ShowVolunteerPositionViewModel = new ShowVolunteerPosition();
            ShowVolunteerPositionViewModel.VolunteerPosition = volunteerPosition;
            ShowVolunteerPositionViewModel.Departments = department;


            return View(ShowVolunteerPositionViewModel);

        }
        public ActionResult Update(int id)
        {
            VolunteerPosition selectedVolunteerPosition = db.VolunteerPositions.SqlQuery("select * from VolunteerPositions where VolunteerPositionID=@VolunteerPositionID", new SqlParameter("@VolunteerPositionID", id)).FirstOrDefault();
            List<Department> departments = db.Departments.SqlQuery("select * from Departments").ToList();
           // string userId = User.Identity.GetUserId();
            //ApplicationUser currentUser = db.Users.FirstOrDefault(x => x.Id == userId);

            UpdateVolunteerPosition UpdateVolunteerPositionViewModel = new UpdateVolunteerPosition();
            UpdateVolunteerPositionViewModel.VolunteerPosition = selectedVolunteerPosition;
            UpdateVolunteerPositionViewModel.Departments = departments;
           // UpdateJobListingViewModel.User = currentUser;

            return View(UpdateVolunteerPositionViewModel);

        }
        [HttpPost]
        public ActionResult Update(int id, string VolunteerPositionTitle, string VolunteerPositionDescription, DateTime StartDate, int DepartmentID )
        {
            string query = "update VolunteerPositions set VolunteerPositionTitle=@VolunteerPositionTitle, VolunteerPositionDescription=@VolunteerPositionDescription, StartDate=@StartDate, DepartmentID=@DepartmentID where VolunteerPositionID=@id";
            SqlParameter[] sqlparams = new SqlParameter[5];
            sqlparams[0] = new SqlParameter("@VolunteerPositionTitle", VolunteerPositionTitle);
            sqlparams[1] = new SqlParameter("@VolunteerPositionDescription", VolunteerPositionDescription);
            sqlparams[2] = new SqlParameter("@StartDate", StartDate);
            sqlparams[3] = new SqlParameter("@DepartmentID", DepartmentID);
          //  sqlparams[6] = new SqlParameter("@UserID", UserID);
            sqlparams[4] = new SqlParameter("@id", id);

            db.Database.ExecuteSqlCommand(query, sqlparams);

            return RedirectToAction("List");
        }
        public ActionResult Delete(int id)
        {
            VolunteerPosition position = db.VolunteerPositions.SqlQuery("select * from VolunteerPositions where VolunteerPositionID = @id", new SqlParameter("@id", id)).FirstOrDefault();
            List<Department> department = db.Departments.SqlQuery("select * from Departments inner join VolunteerPositions on VolunteerPositions.DepartmentID = Departments.DepartmentID where VolunteerPositionID = @id", new SqlParameter("@id", id)).ToList();

            ShowVolunteerPosition ShowVolunteerPositionViewModel = new ShowVolunteerPosition();
            ShowVolunteerPositionViewModel.VolunteerPosition = position;
            ShowVolunteerPositionViewModel.Departments = department;

            return View(ShowVolunteerPositionViewModel);
        }
        [HttpPost]
        public ActionResult Delete(int id, int VolunteerPositionID)
        {
            string query = "delete from VolunteerPositions where VolunteerPositionID = @VolunteerPositionID";
            SqlParameter param = new SqlParameter("@VolunteerPositionID", VolunteerPositionID);
            db.Database.ExecuteSqlCommand(query, param);

            return RedirectToAction("List");
        }

    }
}