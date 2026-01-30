using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StudentManagementSystem.Models;
using StudentManagementSystem.Repository;

namespace StudentManagementSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;

        public StudentService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<Student> GetStudents(string search, string course)
        {
            var data = _repo.GetAll();

            if (!string.IsNullOrEmpty(search))
                data = data.Where(x => x.Name.Contains(search));

            if (!string.IsNullOrEmpty(course))
            {
                if (Enum.TryParse(course, out Course selectedCourse))
                {
                    data = data.Where(x => x.Course == selectedCourse);
                }
            }

            return data.ToList();
        }

        public Student Get(int id) => _repo.GetById(id);

        public void Create(Student s)
        {
            if (_repo.GetAll().Any(x => x.Email == s.Email))
                throw new Exception("Email already exists!");

            _repo.Add(s);
            _repo.Save();
        }

        public void Edit(Student s)
        {
            _repo.Update(s);
            _repo.Save();
        }


        public void Remove(int id) { _repo.Delete(id); _repo.Save(); }
    }
}