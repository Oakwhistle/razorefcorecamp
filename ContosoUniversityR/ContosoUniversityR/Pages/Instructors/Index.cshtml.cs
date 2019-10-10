using ContosoUniversityR.Models;
using ContosoUniversityR.Models.SchoolViewModels;  // Add VM
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversityR.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversityR.Pages.Instructors
{
    public class IndexModel : PageModel
    {
        private readonly ContosoUniversityR.Data.SchoolContext _context;

        public IndexModel(ContosoUniversityR.Data.SchoolContext context)
        {
            _context = context;
        }

        public InstructorIndexData Instructor { get; set; }
        public int InstructorID { get; set; }
        public int CourseID { get; set; }

        public async Task OnGetAsync(int? id, int? courseID)
        {
            Instructor = new InstructorIndexData();
            Instructor.Instructors = await _context.Instructors
                  .Include(i => i.OfficeAssignment)
                  .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Department)
                  //.Include(i => i.CourseAssignments)
                  //    .ThenInclude(i => i.Course)
                  //        .ThenInclude(i => i.Enrollments)
                  //            .ThenInclude(i => i.Student)
                  // .AsNoTracking()
                  .OrderBy(i => i.LastName)
                  .ToListAsync();


            if (id != null)
            {
                InstructorID = id.Value;
                Instructor instructor = Instructor.Instructors.Where(
                    i => i.ID == id.Value).Single();
                Instructor.Courses = instructor.CourseAssignments.Select(s => s.Course);
            }

            if (courseID != null)
            {
                CourseID = courseID.Value;
                var selectedCourse = Instructor.Courses.Where(x => x.CourseID == courseID).Single();
                await _context.Entry(selectedCourse).Collection(x => x.Enrollments).LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
                }
                Instructor.Enrollments = selectedCourse.Enrollments;
            }
        }
    }

}