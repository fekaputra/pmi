﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PMI.Models;

namespace PMI.Areas.Admin.Controllers
{ 
    [Authorize(Roles="CanPostNews")]
    public class CategoryController : Controller
    {
        private pmiEntities db = new pmiEntities();

        //
        // GET: /Admin/Category/

        public ViewResult Index()
        {
            return View(db.Categories.ToList());
        }

        //
        // GET: /Admin/Category/Details/5

        public ViewResult Details(long id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        //
        // GET: /Admin/Category/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Admin/Category/Create

        [HttpPost]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(category);
        }
        
        //
        // GET: /Admin/Category/Edit/5
 
        public ActionResult Edit(long id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        //
        // POST: /Admin/Category/Edit/5

        [HttpPost]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        //
        // GET: /Admin/Category/Delete/5
 
        public ActionResult Delete(long id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        //
        // POST: /Admin/Category/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}