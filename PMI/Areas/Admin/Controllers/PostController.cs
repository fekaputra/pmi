﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PMI.Models;
using PMI.Application.Utils;
using PMI.Application.Mvc.Controller;
using PMI.Application.Mvc.Controller.Extension;
using PagedList;

namespace PMI.Areas.Admin.Controllers
{ 
    [Authorize(Roles="CanPostNews")]
    public class PostController : PMIController
    {
        private pmiEntities db = new pmiEntities();
        private const int POST_PER_PAGE = 5;

        //
        // GET: /Admin/Post/

        public ViewResult Index(string sort, string currentFilter, string filter, int? page)
        {
            ViewBag.UpdatedSort = String.IsNullOrEmpty(sort) ? "updated-asc" : "";
            ViewBag.CreatedSort = sort == "created-desc" ? "created-asc" : "created-desc";
            ViewBag.TitleSort = sort == "title-desc" ? "title-asc" : "title-desc";
            ViewBag.CurrentSort = sort;

            if (Request.HttpMethod == "GET")
            {
                filter = currentFilter;
            }
            else
            {
                page = 1;
            }
            ViewBag.CurrentFilter = filter;

            var posts = db.Posts.Include(p => p.Category1);
            posts = sortPost(sort, posts);
            if(!String.IsNullOrEmpty(filter))
                posts = posts.Where(p => p.title.Contains(filter));

            var pageNumber = page ?? 1;
            return View(posts.ToPagedList(pageNumber, POST_PER_PAGE));
        }

        private IQueryable<Post> sortPost(string sortOrder, IQueryable<Post> posts)
        {
            IQueryable<Post> result;
            
            switch (sortOrder)
            {
                case "updated-asc":
                    result = posts.OrderBy(p => p.updated);
                    break;
                case "created-desc":
                    result = posts.OrderByDescending(p => p.created);
                    break;
                case "created-asc":
                    result = posts.OrderBy(p => p.created);
                    break;
                case "title-desc":
                    result = posts.OrderByDescending(p => p.title);
                    break;
                case "title-asc":
                    result = posts.OrderBy(p => p.title);
                    break;
                default:
                    result = posts.OrderByDescending(p => p.updated);
                    break;
            }

            return result;
        }

        //
        // GET: /Admin/Post/Details/5

        public ViewResult Details(long id)
        {
            Post post = db.Posts.Find(id);
            return View(post);
        }

        //
        // GET: /Admin/Post/Create

        public ActionResult Create()
        {
            ViewBag.category = new SelectList(db.Categories, "id", "culturedDesc");
            return View();
        } 

        //
        // POST: /Admin/Post/Create

        [HttpPost]
        public ActionResult Create(Post post, HttpPostedFileBase postImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    post.SaveImage(postImage);
                    //post.writer = (Guid)Membership.GetUser().ProviderUserKey;
                    post.writer = Membership.GetUser().UserName;
                    db.Posts.Add(post);
                    db.SaveChanges();
                    return RedirectToAction("Index");  
                }
                catch (DbEntityValidationException ex)
                {
                    // ingat, setelah ini akan langsung keluar dari blok (sampai lewat if)
                    // dan jadinya otomatis error tersimpan di model. Tidak perlu return di sini.
                    var errors = ex.EntityValidationErrors.First().ValidationErrors.First();
                    this.ModelState.AddModelError(errors.PropertyName, errors.ErrorMessage);
                }
            }

            //ViewBag.writer = new SelectList(db.aspnet_Users, "UserId", "UserName", post.writer);
            ViewBag.category = new SelectList(db.Categories, "id", "culturedDesc", post.category);
            return View(post);
        }
        
        //
        // GET: /Admin/Post/Edit/5
 
        public ActionResult Edit(long id)
        {
            Post post = db.Posts.Find(id);
            ViewBag.category = new SelectList(db.Categories, "id", "culturedDesc", post.category);

            // if someone got here, they're trying to hack. No need to be nice.
            //if (post.writer != (Guid)Membership.GetUser().ProviderUserKey) 
            if (post.writer != Membership.GetUser().UserName) 
                throw new HttpException(503, "Tidak boleh melakukan edit terhadap tulisan yang dibuat oleh orang lain");

            return View(post);
        }

        //
        // POST: /Admin/Post/Edit/5

        [HttpPost]
        public ActionResult Edit(Post post, HttpPostedFileBase postImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    post.SaveImage(postImage);
                    db.Entry(post).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    // see: Create (POST)
                    var errors = ex.EntityValidationErrors.First().ValidationErrors.First();
                    this.ModelState.AddModelError(errors.PropertyName, errors.ErrorMessage);
                }
            }
            //ViewBag.writer = new SelectList(db.aspnet_Users, "UserId", "UserName", post.writer);
            ViewBag.category = new SelectList(db.Categories, "id", "culturedDesc", post.category);
            return View(post);
        }

        //
        // GET: /Admin/Post/Delete/5
 
        public ActionResult Delete(long id)
        {
            Post post = db.Posts.Find(id);
            return View(post);
        }

        //
        // POST: /Admin/Post/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {            
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
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