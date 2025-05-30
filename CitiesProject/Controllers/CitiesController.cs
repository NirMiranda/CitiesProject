﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CitiesProject.Models;
using PagedList;

namespace CitiesProject.Controllers
{
    public class CitiesController : Controller
    {
        private CitiesProjectDBEntities db = new CitiesProjectDBEntities();

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var cities = from c in db.Cities select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                string convertedHebrew = ConvertEngToHebKeyboard(searchString);
                cities = cities.Where(c =>
                    c.CityName.Contains(searchString) ||
                    c.CityName.Contains(convertedHebrew));
            }

            cities = sortOrder == "name_desc"
                ? cities.OrderByDescending(c => c.CityName)
                : cities.OrderBy(c => c.CityName);

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(cities.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            City city = db.Cities.Find(id);
            if (city == null)
                return HttpNotFound();

            return View(city);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CityName")] City city)
        {
            if (db.Cities.Any(c => c.CityName == city.CityName))
            {
                ModelState.AddModelError("CityName", "העיר כבר קיימת במערכת.");
            }

            if (ModelState.IsValid)
            {
                db.Cities.Add(city);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(city);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            City city = db.Cities.Find(id);
            if (city == null)
                return HttpNotFound();

            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CityName")] City city)
        {
            if (db.Cities.Any(c => c.CityName == city.CityName && c.Id != city.Id))
            {
                ModelState.AddModelError("CityName", "שם העיר כבר קיים.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(city).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(city);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            City city = db.Cities.Find(id);
            if (city == null)
                return HttpNotFound();

            return View(city);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            City city = db.Cities.Find(id);
            db.Cities.Remove(city);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private string ConvertEngToHebKeyboard(string input)
        {
            var map = new Dictionary<char, char>()
            {
                {'q','/'}, {'w','\''}, {'e','ק'}, {'r','ר'}, {'t','א'}, {'y','ט'}, {'u','ו'}, {'i','ן'}, {'o','ם'}, {'p','פ'},
                {'a','ש'}, {'s','ד'}, {'d','ג'}, {'f','כ'}, {'g','ע'}, {'h','י'}, {'j','ח'}, {'k','ל'}, {'l','ך'}, {';','ף'},
                {'z','ז'}, {'x','ס'}, {'c','ב'}, {'v','ה'}, {'b','נ'}, {'n','מ'}, {'m','צ'}, {',','ת'}, {'.','ץ'}
            };

            var converted = new StringBuilder();
            foreach (var c in input.ToLower())
            {
                converted.Append(map.ContainsKey(c) ? map[c] : c);
            }
            return converted.ToString();
        }
        private bool IsHebrewOnly(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[\u0590-\u05FF\s\-]+$");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
