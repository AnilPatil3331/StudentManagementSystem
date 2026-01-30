using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using StudentManagementSystem.DAL;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Student> GetAll() => _context.Students;

        public Student GetById(int id) => _context.Students.Find(id);

        public void Add(Student student) => _context.Students.Add(student);

        public void Update(Student student)
        {
            var local = _context.Set<Student>()
                                .Local
                                .FirstOrDefault(f => f.Id == student.Id);

            if (local != null)
            {
                // detach old tracked entity
                _context.Entry(local).State = EntityState.Detached;
            }

            // attach new object safely
            _context.Entry(student).State = EntityState.Modified;
        }



        public void Delete(int id)
        {
            var s = _context.Students.Find(id);
            _context.Students.Remove(s);
        }

        public void Save() => _context.SaveChanges();
    }
}